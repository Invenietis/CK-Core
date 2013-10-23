using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ActivityMonitorCrossAppDomain
    {
        class AppDomainCommunication : MarshalByRefObject
        {
            readonly object _locker = new object();
            bool _done;
            bool _success;

            public AppDomainCommunication( IActivityMonitor monitor )
            {
                _locker = new object();
                MonitorBridge = monitor.Output.BridgeTarget;
            }

            public ActivityMonitorBridgeTarget MonitorBridge { get; private set; }

            public bool WaitForResult()
            {
                lock( _locker )
                    while( !_done )
                        Monitor.Wait( _locker );
                return _success;
            }

            public void SetResult( bool success )
            {
                _success = success;
                lock( _locker )
                {
                    _done = true;
                    Monitor.Pulse( _locker );
                }
            }

            public void OpenWarnGroupFromOriginDomain( LogFilter expectedConfiguredAndActualFilter )
            {
                Assert.That( MonitorBridge.TargetMonitor, Is.Not.Null, "Since we are in the original App Domain." );
                Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( expectedConfiguredAndActualFilter ) );
                using( MonitorBridge.TargetMonitor.OpenGroup( LogLevel.Warn, "From Origin AppDomain: changing the Filter in a group (useless)." ) )
                {
                    // This change must be seen by the Bridge (but is restored).
                    MonitorBridge.TargetMonitor.MinimalFilter = MonitorBridge.TargetMonitor.MinimalFilter.SetLine( LogLevelFilter.Trace );
                    Assert.That( MonitorBridge.TargetMonitor.ActualFilter.Line, Is.EqualTo( LogLevelFilter.Trace ), "Configured filter is the only one." );
                    
                    Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.Not.EqualTo( expectedConfiguredAndActualFilter ), "This test is useless!" );
                }
                Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( expectedConfiguredAndActualFilter ), "Group closing restores the filter." );
            }

            public void SetFilterFromOriginDomain( LogFilter filter )
            {
                // This change WILL impact the Bridge if the ActualFilter changed (depending on the Client minimal filter).
                MonitorBridge.TargetMonitor.MinimalFilter = filter;
            }

            internal void DoAsyncSetFilterViaClientInOriginDomain( LogFilter logLevelFilter, LogFilter? resultingActualFilter )
            {
                var c = MonitorBridge.TargetMonitor.Output.Clients.OfType<ActivityMonitorClientTester>().Single();
                c.AsyncSetMinimalFilterBlock( logLevelFilter, 1 );
                if( resultingActualFilter.HasValue ) Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( resultingActualFilter.Value ) );
            }
        }

        [Test]
        [Category( "ActivityMonitor" )]
        [Category( "Console" )]
        public void TestCrossDomain()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.MinimalFilter = LogFilter.Terse;
            monitor.Output.RegisterClient( new ActivityMonitorClientTester() );
            StupidStringClient textDump = monitor.Output.RegisterClient( new StupidStringClient( true, true ) );
            monitor.Output.RegisterClient( new ActivityMonitorErrorCounter() { GenerateConclusion = true } );

            Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Terse ), "This is the original configuration." );
            Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Terse ), "This is the original configuration." );

            TestHelper.ConsoleMonitor.MinimalFilter = LogFilter.Undefined;

            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                using( monitor.OpenGroup( LogLevel.Info, "Launching Application Domain." ) )
                {
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Terse ), "This is the original configuration." );
                    var appDomain = AppDomain.CreateDomain( "ExternalDomainForTestCrossDomainMonitor", null, setup );
                    AppDomainCommunication appDomainComm = new AppDomainCommunication( monitor );
                    appDomain.SetData( "external-appDomainComm", appDomainComm );
                    appDomain.DoCallBack( new CrossAppDomainDelegate( LaunchRunCrossDomain ) );
                    Assert.That( appDomainComm.WaitForResult(), "There must be no error in LaunchRunCrossDomain." );
                    AppDomain.Unload( appDomain );
                }
            }
            string text = textDump.ToString();
            Assert.That( text, Is.Not.StringContaining( "#NOT APPEAR#" ) );

            Assert.That( text, Is.StringContaining( "In another AppDomain." ) );
            Assert.That( text, Is.StringContaining( "An error is logged|External App Domain|Marshalled trait|Test for fun" ) );
            Assert.That( text, Is.StringContaining( "From external world." ) );
            Assert.That( text, Is.StringContaining( "Exceptions are serialized as CKExceptionData." ) );
            Assert.That( text, Is.StringContaining( "Name of the AppDomain is 'ExternalDomainForTestCrossDomainMonitor'." ) );
            Assert.That( text, Is.StringContaining( "Everything is fine.-/[/c:User/]/" ) );
            Assert.That( text, Is.StringContaining( "1 Fatal error, 2 Errors, 1 Warning-/[/c:ErrorCounter/]/" ), "There must be only one Warning since the second call to OpenWarnGroupFromOriginDomain must be filtered." );

            Assert.That( text, Is.StringContaining( R.ClosedByBridgeRemoved ) );
            Assert.That( text, Is.StringContaining( ActivityMonitorBridge.TagBridgePrematureClose.ToString() ) );

        }

        private static void LaunchRunCrossDomain()
        {
            AppDomainCommunication appDomainComm = (AppDomainCommunication)AppDomain.CurrentDomain.GetData( "external-appDomainComm" );

            // Creates a ActivityMonitor in order to have automatic ErrorCounter conclusions on Closed groups.
            IActivityMonitor monitor = new ActivityMonitor( applyAutoConfigurations: false );
            monitor.Output.RegisterClient( new ActivityMonitorErrorCounter() { GenerateConclusion = true } );

            using( monitor.Output.CreateBridgeTo( appDomainComm.MonitorBridge ) )
            {
                try
                {
                    monitor.AutoTags = ActivityMonitor.RegisteredTags.FindOrCreate( "External App Domain|Test for fun" );
                    monitor.OpenGroup( LogLevel.Info, "In another AppDomain." );

                    monitor.Trace( "This will #NOT APPEAR# (Filter is LogFilter.Terse: only errors are captured)." );
                    monitor.Warn( () => { Assert.Fail( "This will never be called." ); return null; } );

                    monitor.Error( "From external world." );
                    monitor.Error( ActivityMonitor.RegisteredTags.FindOrCreate( "An error is logged|Marshalled trait" ), new Exception( "Exceptions are serialized as CKExceptionData." ), "From external world." );
                    monitor.Fatal( "Name of the AppDomain is '{0}'.", AppDomain.CurrentDomain.FriendlyName );
                    monitor.CloseGroup( "Everything is fine." );

                    Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Undefined ) );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Terse ) );
                    // This will create a Warn (Terse is Info on Groups).
                    appDomainComm.OpenWarnGroupFromOriginDomain( LogFilter.Terse );

                    appDomainComm.SetFilterFromOriginDomain( LogFilter.Debug );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Debug ), "Changing Filter in the Origin Domain impacts this monitor since there is no other config here (explicit on the local monitor or by other client)." );
                    Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Undefined ), "This monitor's configured filter did not change." );

                    appDomainComm.SetFilterFromOriginDomain( LogFilter.Undefined );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Undefined ), "This monitor has NO more configuration." );

                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogFilter.Release, resultingActualFilter: LogFilter.Release );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Release ), "The client requires Release, no one else requires something else: the target monitor's Actual Filter is Release ==> the bridge has been warned and its own monitor is now on Release..." );

                    // This has no effect (since we are in Release).
                    appDomainComm.OpenWarnGroupFromOriginDomain( LogFilter.Release );

                    monitor.Warn( () => { Assert.Fail( "This will never be called." ); return null; } );

                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogFilter.Debug, resultingActualFilter: LogFilter.Debug );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Debug ) );

                    // Back to release without any sollicitation of the ActualFilter from the other AppDomain.
                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogFilter.Release, null );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Release ), "Our ActualFilter is dirty: it triggers an update inside the other AppDomain." );

                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogFilter.Debug, resultingActualFilter: LogFilter.Debug );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogFilter.Debug ) );

                    monitor.OpenGroup( LogLevel.Info, "Opened but not closed Group..." );

                    appDomainComm.SetResult( true );
                }
                catch( Exception ex )
                {
                    monitor.Fatal( ex );
                    appDomainComm.SetResult( false );
                }
            }
        }


    }
}
