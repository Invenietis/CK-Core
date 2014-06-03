using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;
using NUnit.Framework;

namespace CK.Monitoring.Tests.Configuration
{
    [TestFixture]
    public class GrandOutputConfigTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

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
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Channel><Add Type=""BinaryFile"" /></Channel></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Channel><Add Type=""BinaryFile"" Name=""GlobalCatch"" /></Channel></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ), Is.False );
            // This is okay: Type, Name and Path for BinaryFile.
            Assert.That( c.Load( XDocument.Parse( @"<GrandOutputConfiguration><Channel><Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""In-Root-Log-Path"" /></Channel></GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ) );
        }

        [Test]
        public void ApplyConfigWithError()
        {
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""Configuration/ invalid path? (? is forbidden)"" />
    </Channel>
</GrandOutputConfiguration>"
                ).Root, TestHelper.ConsoleMonitor ) );
            Assert.That( c.ChannelsConfiguration.Configurations.Count, Is.EqualTo( 1 ) );

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
    <Channel MinimalFilter=""{Trace,Info}"">
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""Configuration/ApplyConfig"" />
    </Channel>
</GrandOutputConfiguration>" ).Root, TestHelper.ConsoleMonitor ) );

            Assert.That( c.ChannelsConfiguration.Configurations.Count, Is.EqualTo( 1 ) );

            ActivityMonitor m = new ActivityMonitor( false );
            using( GrandOutput g = new GrandOutput() )
            {
                m.Info().Send( "Before Registering - NOSHOW" );
                g.Register( m );
                m.Info().Send( "Before configuration - NOSHOW" );
                Assert.That( g.SetConfiguration( c, TestHelper.ConsoleMonitor ) );
                m.Info().Send( "After configuration. INFO1" );

                Assert.That( m.ActualFilter, Is.EqualTo( new LogFilter( LogLevelFilter.Trace, LogLevelFilter.Info ) ) ); 
                m.Trace().Send( "TRACE1-NOSHOW (MinimalFilter of the Channel)." );
                
                Assert.That( g.SetConfiguration( new GrandOutputConfiguration(), TestHelper.ConsoleMonitor ) );
                g.Dispose( TestHelper.ConsoleMonitor );

                m.Info().Send( "After disposing - NOSHOW." );

                Assert.That( m.ActualFilter, Is.EqualTo( LogFilter.Undefined ) ); 
            }
            
        }

    }
}
