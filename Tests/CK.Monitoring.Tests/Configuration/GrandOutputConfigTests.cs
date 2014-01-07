using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests.Configuration
{
    [TestFixture]
    public class GrandOutputConfigTests
    {

        [Test]
        public void InvalidRootNode()
        {
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"<root><Add Type=""BinaryFile"" /></root>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
        }
        
        [Test]
        public void ConfigObjectAttributeRequired()
        {
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Add /></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Add Type=""BinaryFile"" /></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Add Type=""BinaryFile"" Name=""GlobalCatch"" /></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
            // This is okay: Type, Name and Path for BinaryFile.
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""In-Root-Log-Path"" /></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ) );
        }

        [Test]
        public void ApplyConfigWithError()
        {
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" ><Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""Configuration/ApplyConfig"" /></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ) );
            Assert.That( c.RouteConfiguration.Configurations.Count, Is.EqualTo( 1 ) );

            SystemActivityMonitor.RootLogPath = null;

            GrandOutput g = new GrandOutput();
            Assert.That( g.SetConfiguration( c, TestHelper.ConsoleMonitor ), Is.False );
            Assert.That( g.IsDisposed, Is.False );
            g.Dispose( TestHelper.ConsoleMonitor );
            Assert.That( g.IsDisposed );

        }

        [Test]
        public void ApplyConfigSimple()
        {
            GrandOutputConfiguration c = new GrandOutputConfiguration();

            Assert.That( c.Load( XDocument.Parse( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""Configuration/ApplyConfig"" />
</GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ) );

            Assert.That( c.RouteConfiguration.Configurations.Count, Is.EqualTo( 1 ) );

            SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;

            ActivityMonitor m = new ActivityMonitor( false );
            using( GrandOutput g = new GrandOutput() )
            {
                m.Info().Send( "Before Registering - NOSHOW" );
                g.Register( m );
                m.Info().Send( "Before configuration - NOSHOW" );
                Assert.That( g.SetConfiguration( c, TestHelper.ConsoleMonitor ) );
                m.Info().Send( "After configuration. INFO1" );
                m.Trace().Send( "TRACE1" );
                g.Dispose( TestHelper.ConsoleMonitor );
                m.Info().Send( "After disposing - NOSHOW." );
            }
        }

    }
}
