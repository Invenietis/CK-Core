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
    public class ActivityLoggerCrossAppDomain
    {
        class AppDomainCommunication : MarshalByRefObject
        {
            readonly object _locker = new object();
            bool _done;
            bool _success;

            public AppDomainCommunication( IActivityLogger logger )
            {
                _locker = new object();
                LoggerBridge = logger.Output.ExternalInput;
            }

            public ActivityLoggerBridgeTarget LoggerBridge { get; private set; }

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
        [Category( "ActivityLogger" )]
        [Category( "Console" )]
        public void TestCrossDomain()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Filter = LogLevelFilter.Info;
            logger.Output.BridgeTo( TestHelper.Logger );
            StringImpl textDump = new StringImpl( true, true );
            logger.Tap.Register( textDump );

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            using( logger.OpenGroup( LogLevel.Info, "Launching Application Domain." ) )
            {
                var appDomain = AppDomain.CreateDomain( "ExternalDomainForTestCrossDomainLogger", null, setup );
                AppDomainCommunication appDomainComm = new AppDomainCommunication( logger );
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
            Assert.That( text, Is.StringContaining( "Name of the AppDomain is 'ExternalDomainForTestCrossDomainLogger'." ) );
            Assert.That( text, Is.StringContaining( "Everything is fine.-/[/c:User/]/" ) );
            Assert.That( text, Is.StringContaining( "1 Error, 1 Warning-/[/c:ErrorCounter/]/" ) );

            Assert.That( text, Is.StringContaining( R.ClosedByBridgeRemoved ) );
            Assert.That( text, Is.StringContaining( ActivityLoggerBridge.TagBridgePrematureClose.ToString() ) );

        }

        private static void LaunchRunCrossDomain()
        {
            AppDomainCommunication appDomainComm = (AppDomainCommunication)AppDomain.CurrentDomain.GetData( "external-appDomainComm" );

            // Creates a DefaultActivityLogger in order to have automatic ErrorCounter conclusions on Closed groups.
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.ErrorCounter.GenerateConclusion = true;
            logger.Output.RegisterClient( new ActivityLoggerBridge( appDomainComm.LoggerBridge ) );
            logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "External App Domain|Test for fun" );

            try
            {
                logger.OpenGroup( LogLevel.Info, "In another AppDomain." );
                logger.Trace( "This will #NOT APPEAR# in calling domain (Filter is Info)." );
                logger.Info( "From external world." );
                logger.Error( ActivityLogger.RegisteredTags.FindOrCreate( "An error is logged|Marshalled trait" ), new Exception( "Exceptions are serializable." ), "From external world." );
                logger.Warn( "Name of the AppDomain is '{0}'.", AppDomain.CurrentDomain.FriendlyName );
                logger.CloseGroup( "Everything is fine." );
                
                logger.OpenGroup( LogLevel.Info, "Opened but not closed Group..." );
                logger.Output.UnregisterClient( logger.Output.RegisteredClients[0] );

                appDomainComm.SetResult( true );
            }
            catch( Exception ex )
            {
                logger.Fatal( ex );
                appDomainComm.SetResult( false );
            }
        }


    }
}
