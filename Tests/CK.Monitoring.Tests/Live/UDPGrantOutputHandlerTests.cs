using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring.GrandOutputHandlers;
using NUnit.Framework;
using CK.Core;
using CK.Monitoring.Udp;

namespace CK.Monitoring.Tests.Live
{
    [TestFixture( Category = "ActivityMonitor.Live" )]
    public class UDPGrantOutputHandlerTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
        }

        [Test]
        public void GrantOutputHandler_IsRegistered_InGrandOutput()
        {
            GrandOutput.EnsureActiveDefault( configurator =>
            {
                configurator.CommonSink.Add(
                    new UdpHandler(
                        new UdpHandlerConfiguration( "UDPConfiguration" ) { Port = 3712 } ) );
            } );

            IActivityMonitor m = new ActivityMonitor();
            m.Trace().Send( "Log entry" );
            
        }
    }
}
