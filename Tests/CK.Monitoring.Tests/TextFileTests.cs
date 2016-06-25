using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CK.Core;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CK.Monitoring.Tests
{
    [TestFixture]
    public class TextFileTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Explicit]
        [Test]
        public void dumping_text_file_with_multiple_monitors()
        {
            TestHelper.CleanupFolder( SystemActivityMonitor.RootLogPath + "TextFileMulti" );
            Random r = new Random();
            using( GrandOutput g = new GrandOutput() )
            {
                GrandOutputConfiguration config = new GrandOutputConfiguration();
                config.Load( XDocument.Parse( @"
<GrandOutputConfiguration>
    <Channel MinimalFilter=""Debug"">
        <Add Type=""TextFile"" Name=""All"" Path=""TextFileMulti"" />
    </Channel>
</GrandOutputConfiguration>", LoadOptions.SetLineInfo ).Root, TestHelper.ConsoleMonitor );

                Assert.That( g.SetConfiguration( config, TestHelper.ConsoleMonitor ) );
                Parallel.Invoke(
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs2( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs2( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs1( r, g ),
                    () => DumpSampleLogs2( r, g )
                    );
            }
            FileInfo f = new DirectoryInfo( SystemActivityMonitor.RootLogPath + "TextFileMulti" ).EnumerateFiles().Single();
            string text = File.ReadAllText( f.FullName );
            Console.WriteLine( text );
        }

        [Test]
        public void dumping_text_file()
        {
            TestHelper.CleanupFolder( SystemActivityMonitor.RootLogPath + "TextFile" );
            Random r = new Random();
            using( GrandOutput g = new GrandOutput() )
            {
                GrandOutputConfiguration config = new GrandOutputConfiguration();
                config.Load( XDocument.Parse( @"
<GrandOutputConfiguration>
    <Channel MinimalFilter=""Debug"">
        <Add Type=""TextFile"" Name=""All"" Path=""TextFile"" />
    </Channel>
</GrandOutputConfiguration>", LoadOptions.SetLineInfo ).Root, TestHelper.ConsoleMonitor );

                Assert.That( g.SetConfiguration( config, TestHelper.ConsoleMonitor ) );

                DumpSampleLogs1( r, g );
                DumpSampleLogs2( r, g );
            }
            CheckSampleTextFile();
        }

        static void CheckSampleTextFile()
        {
            FileInfo f = new DirectoryInfo( SystemActivityMonitor.RootLogPath + "TextFile" ).EnumerateFiles().Single();
            string text = File.ReadAllText( f.FullName );
            Console.WriteLine( text );
            Assert.That( text, Does.Contain( "First Activity..." ) );
            Assert.That( text, Does.Contain( "End of first activity." ) );
            Assert.That( text, Does.Contain( "another one" ) );
            Assert.That( text, Does.Contain( "Something must be said" ) );
            Assert.That( text, Does.Contain( "My very first conclusion." ) );
            Assert.That( text, Does.Contain( "My second conclusion." ) );
        }

        static void DumpSampleLogs1( Random r, GrandOutput g )
        {
            var m = new ActivityMonitor( false );
            g.Register( m );
            m.SetTopic( "First Activity..." );
            if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
            using( m.OpenTrace().Send( "Opening trace" ) )
            {
                if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
                m.Trace().Send( "A trace in group." );
                if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
                m.Info().Send( "An info in group." );
                if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
                m.Warn().Send( "A warning in group." );
                if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
                m.Error().Send( "An error in group." );
                if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
                m.Fatal().Send( "A fatal in group." );
            }
            if( r.Next( 3 ) == 0 ) System.Threading.Thread.Sleep( 100 + r.Next( 2500 ) );
            m.Trace().Send( "End of first activity." );
        }

        static void DumpSampleLogs2( Random r, GrandOutput g )
        {
            var m = new ActivityMonitor( false );
            g.Register( m );

            m.Fatal().Send( ThrowExceptionWithInner( false ), "An error occured" );
            m.SetTopic( "This is a topic..." );
            m.Trace().Send( "a trace" );
            m.Trace().Send( "another one" );
            m.SetTopic( "Please, show this topic!" );
            m.Trace().Send( "Anotther trace." );
            using( m.OpenTrace().Send( "A group trace." ) )
            {
                m.Trace().Send( "A trace in group." );
                m.Info().Send( "An info..." );
                using( m.OpenInfo().Send( @"A group information... with a 
multi
-line
message. 
This MUST be correctly indented!" ) )
                {
                    m.Info().Send( "Info in info group." );
                    m.Info().Send( "Another info in info group." );
                    m.Error().Send( ThrowExceptionWithInner( true ), "An error." );
                    m.Warn().Send( "A warning." );
                    m.Trace().Send( "Something must be said." );
                    m.CloseGroup( "Everything is in place." );
                }
            }
            m.SetTopic( null );
            using( m.OpenTrace().Send( "A group with multiple conclusions." ) )
            {
                using( m.OpenTrace().Send( "A group with no conclusion." ) )
                {
                    m.Trace().Send( "Something must be said." );
                }
                m.CloseGroup( new[] {
                        new ActivityLogGroupConclusion( "My very first conclusion." ),
                        new ActivityLogGroupConclusion( "My second conclusion." ),
                        new ActivityLogGroupConclusion( @"My very last conclusion
is a multi line one.
and this is fine!" )
                    } );
            }
            m.Trace().Send( "This is the final trace." );
        }

        static Exception ThrowExceptionWithInner( bool loaderException = false )
        {
            Exception e;
            try { throw new Exception( "Outer", loaderException ? ThrowLoaderException() : ThrowSimpleException( "Inner" ) ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

        static Exception ThrowSimpleException( string message )
        {
            Exception e;
            try { throw new Exception( message ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

        static Exception ThrowLoaderException()
        {
            Exception e = null;
            try { Type.GetType( "A.Type, An.Unexisting.Assembly", true ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }
    }
}
