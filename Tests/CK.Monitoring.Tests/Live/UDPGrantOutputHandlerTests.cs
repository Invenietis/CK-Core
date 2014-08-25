using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring.GrandOutputHandlers;
using NUnit.Framework;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers.UDP;

namespace CK.Monitoring.Tests.Live
{
    [TestFixture( Category = "ActivityMonitor.Live" )]
    public class UDPGrantOutputHandlerTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Test]
        public void GrantOutputHandler_IsRegistered_InGrandOutput()
        {
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
            GrandOutput.EnsureActiveDefault( configurator =>
            {
                configurator.CommonSink.Add(
                    new UDPGrantOutputHandler(
                        new UDPHandlerConfiguration( "UDPConfiguration" ) { Port = 3712 } ) );
            } );

            IActivityMonitor m = new ActivityMonitor();
            m.Trace().Send( "Log entry" );
        }


        class DummyLogSender : ILogSender
        {
            public void SendLog( IMulticastLogEntry entry )
            {
                Assert.That( entry.Text == "Log entry" );
            }

            public Task SendLogAsync( IMulticastLogEntry entry )
            {
                return Task.FromResult( 0 );
            }

            public void Dispose()
            {
            }

            public void Initialize( IActivityMonitor monitor )
            {
            }

            public void Close( IActivityMonitor monitor )
            {
            }
        }

        class UDPHandlerConfiguration : HandlerConfiguration
        {
            public UDPHandlerConfiguration( string name )
                : base( name )
            {
            }

            public int Port { get; set; }

            protected override void Initialize( Core.IActivityMonitor m, System.Xml.Linq.XElement xml )
            {
                Port = xml.GetAttributeInt( "Port", Port );
            }
        }

        class UDPGrantOutputHandler : HandlerBase
        {
            ILogSender _logSender;
            public UDPGrantOutputHandler( UDPHandlerConfiguration config )
                : base( config )
            {
                _logSender = new DummyLogSender();
            }

            public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
            {
                _logSender.SendLog( logEvent.Entry );
            }

            public override void Initialize( Core.IActivityMonitor monitor )
            {
                base.Initialize( monitor );
            }

            public override void Close( Core.IActivityMonitor monitor )
            {
                base.Close( monitor );
            }
        }
    }
}
