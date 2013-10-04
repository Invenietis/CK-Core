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

            public void DoSomethingInOriginDomainAndSetFilterTo( LogLevelFilter filter, bool emitLogs = false )
            {
                if( emitLogs )
                {
                    Assert.That( MonitorBridge.TargetMonitor, Is.Not.Null, "Since we are in the original App Domain." );
                    Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( LogLevelFilter.Info ), "This is the original configuration." );
                    using( MonitorBridge.TargetMonitor.OpenGroup( LogLevel.Info, "From Origin AppDomain: changing the Filter in a group (useless)." ) )
                    {
                        // This change must be seen by the Bridge (but is restored).
                        MonitorBridge.TargetMonitor.Filter = LogLevelFilter.Fatal;
                        Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( LogLevelFilter.Fatal ), "Configured filter is the only one." );
                    }
                    Assert.That( MonitorBridge.TargetMonitor.Filter, Is.EqualTo( LogLevelFilter.Info ), "Group closing restores the filter." );
                    Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( LogLevelFilter.Info ), "Group closing restores the filter." );
                }
                // This change WILL impact the Bridge if the ActualFilter changed (depending on the Client minimal filter).
                MonitorBridge.TargetMonitor.Filter = filter;
            }

            internal void DoAsyncSetFilterViaClientInOriginDomain( LogLevelFilter logLevelFilter, LogLevelFilter resultingActualFilter )
            {
                var c = MonitorBridge.TargetMonitor.Output.Clients.OfType<ActivityMonitorClientTester>().Single();
                c.AsyncSetMinimalFilterBlock( logLevelFilter, 1 );
                Assert.That( MonitorBridge.TargetMonitor.ActualFilter, Is.EqualTo( resultingActualFilter ) );
            }
        }

        [Test]
        [Category( "ActivityMonitor" )]
        [Category( "Console" )]
        public void TestCrossDomain()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Filter = LogLevelFilter.Info;
            monitor.Output.RegisterClient( new ActivityMonitorClientTester() );
            StupidStringClient textDump = monitor.Output.RegisterClient( new StupidStringClient( true, true ) );

            Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.Info ), "This is the original configuration." );
            Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Info ), "This is the original configuration." );
            
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                using( monitor.OpenGroup( LogLevel.Info, "Launching Application Domain." ) )
                {
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Info ), "This is the original configuration." );
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
            Assert.That( text, Is.StringContaining( "Exceptions are serializable." ) );
            Assert.That( text, Is.StringContaining( "Name of the AppDomain is 'ExternalDomainForTestCrossDomainMonitor'." ) );
            Assert.That( text, Is.StringContaining( "Everything is fine.-/[/c:User/]/" ) );
            Assert.That( text, Is.StringContaining( "1 Error, 1 Warning-/[/c:ErrorCounter/]/" ) );

            Assert.That( text, Is.StringContaining( R.ClosedByBridgeRemoved ) );
            Assert.That( text, Is.StringContaining( ActivityMonitorBridge.TagBridgePrematureClose.ToString() ) );

        }

        private static void LaunchRunCrossDomain()
        {
            AppDomainCommunication appDomainComm = (AppDomainCommunication)AppDomain.CurrentDomain.GetData( "external-appDomainComm" );

            // Creates a ActivityMonitor in order to have automatic ErrorCounter conclusions on Closed groups.
            IActivityMonitor monitor = new ActivityMonitor( applyAutoConfigurations: false );
            monitor.Output.RegisterClient( new ActivityMonitorErrorCounter() { GenerateConclusion = true } );

            try
            {
                using( monitor.Output.CreateBridgeTo( appDomainComm.MonitorBridge, applyTargetHonorMonitorFilterToOpenGroup: true ) )
                {
                    monitor.AutoTags = ActivityMonitor.RegisteredTags.FindOrCreate( "External App Domain|Test for fun" );
                    monitor.OpenGroup( LogLevel.Info, "In another AppDomain." );
                    
                    monitor.Trace( "This will #NOT APPEAR# (Filter is Info)." );
                    monitor.Trace( () => { Assert.Fail( "This will never be called." ); return null; } );

                    monitor.Info( "From external world." );
                    monitor.Error( ActivityMonitor.RegisteredTags.FindOrCreate( "An error is logged|Marshalled trait" ), new Exception( "Exceptions are serializable." ), "From external world." );
                    monitor.Warn( "Name of the AppDomain is '{0}'.", AppDomain.CurrentDomain.FriendlyName );
                    monitor.CloseGroup( "Everything is fine." );

                    Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.None ) );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Info ) );
                    appDomainComm.DoSomethingInOriginDomainAndSetFilterTo( LogLevelFilter.Trace, emitLogs: true );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Trace ), "Changing Filter in the Origin Domain impacts this monitor since there is no other config here (explicit on the local monitor or by other client)." );
                    Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.None ), "This monitor's configured filter did not change." );

                    appDomainComm.DoSomethingInOriginDomainAndSetFilterTo( LogLevelFilter.None );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.None ), "This monitor has NO more configuration." );

                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogLevelFilter.Error, resultingActualFilter: LogLevelFilter.Error );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Error ), "The client requires error, no one else requires something else: the target monitor's Actual Filter is Error ==> the bridge has been warned and its own monitor is now on Error..." );

                    monitor.Warn( () => { Assert.Fail( "This will never be called." ); return null; } );
                    
                    appDomainComm.DoAsyncSetFilterViaClientInOriginDomain( LogLevelFilter.Trace, resultingActualFilter: LogLevelFilter.Trace );
                    Assert.That( monitor.ActualFilter, Is.EqualTo( LogLevelFilter.Trace ) );

                    monitor.OpenGroup( LogLevel.Info, "Opened but not closed Group..." );
                }

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
