using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
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
                MonitorBridge = monitor.Output.ExternalInput;
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
        }

        [Test]
        [Category( "ActivityMonitor" )]
        [Category( "Console" )]
        public void TestCrossDomain()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Filter = LogLevelFilter.Info;
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            StupidStringClient textDump = monitor.Output.RegisterClient( new StupidStringClient( true, true ) );

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            using( monitor.OpenGroup( LogLevel.Info, "Launching Application Domain." ) )
            {
                var appDomain = AppDomain.CreateDomain( "ExternalDomainForTestCrossDomainMonitor", null, setup );
                AppDomainCommunication appDomainComm = new AppDomainCommunication( monitor );
                appDomain.SetData( "external-appDomainComm", appDomainComm );
                appDomain.DoCallBack( new CrossAppDomainDelegate( LaunchRunCrossDomain ) );
                appDomainComm.WaitForResult();
                AppDomain.Unload( appDomain );
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
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.RegisterClient( new ActivityMonitorErrorCounter() { GenerateConclusion = true } );
            monitor.Output.RegisterClient( new ActivityMonitorBridge( appDomainComm.MonitorBridge ) );
            monitor.AutoTags = ActivityMonitor.RegisteredTags.FindOrCreate( "External App Domain|Test for fun" );

            try
            {
                monitor.OpenGroup( LogLevel.Info, "In another AppDomain." );
                monitor.Trace( "This will #NOT APPEAR# in calling domain (Filter is Info)." );
                monitor.Info( "From external world." );
                monitor.Error( ActivityMonitor.RegisteredTags.FindOrCreate( "An error is logged|Marshalled trait" ), new Exception( "Exceptions are serializable." ), "From external world." );
                monitor.Warn( "Name of the AppDomain is '{0}'.", AppDomain.CurrentDomain.FriendlyName );
                monitor.CloseGroup( "Everything is fine." );
                
                monitor.OpenGroup( LogLevel.Info, "Opened but not closed Group..." );
                monitor.Output.UnregisterClient( monitor.Output.Clients[0] );

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
