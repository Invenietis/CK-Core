using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    [TestFixture]
    public class GrandOutputTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
            GrandOutput.GrandOutputMinimalFilter = LogFilter.Debug;
        }

        [TearDown]
        public void Teardown()
        {
            GrandOutput.GrandOutputMinimalFilter = LogFilter.Release;
        }

        [Test]
        public void ApplyEmptyAndDefaultConfig()
        {
            TestHelper.CleanupFolder( SystemActivityMonitor.RootLogPath + "ApplyEmptyAndDefaultConfig" );

            using( GrandOutput g = new GrandOutput() )
            {
                var m = new ActivityMonitor( false );
                g.Register( m );
                m.Trace().Send( "NoShow-1" );
                Assert.That( g.SetConfiguration( new GrandOutputConfiguration(), TestHelper.ConsoleMonitor ) );
                m.Trace().Send( "NoShow-2" );
                Assert.That( g.SetConfiguration( CreateDefaultConfig( "ApplyEmptyAndDefaultConfig" ), TestHelper.ConsoleMonitor ) );
                m.Trace().Send( "Show-1" );
                Assert.That( g.SetConfiguration( new GrandOutputConfiguration(), TestHelper.ConsoleMonitor ) );
                m.Trace().Send( "NoShow-3" );
            }
            var replayed = new ActivityMonitor( false );
            var c = replayed.Output.RegisterClient( new StupidStringClient() );
            TestHelper.ReplayLogs( new DirectoryInfo( SystemActivityMonitor.RootLogPath + "ApplyEmptyAndDefaultConfig" ), true, mon => replayed, TestHelper.ConsoleMonitor );
            CollectionAssert.AreEqual( new[] { "<Missing log data>", "Show-1" }, c.Entries.Select( e => e.Text ), StringComparer.OrdinalIgnoreCase );
        }

        static GrandOutputConfiguration CreateDefaultConfig( string subFolder )
        {
            GrandOutputConfiguration def = new GrandOutputConfiguration();
            Debug.Assert( def.SourceOverrideFilterApplicationMode == SourceFilterApplyMode.None );
            Debug.Assert( def.GlobalDefaultFilter == null );
            var route = new RouteConfiguration();
            route.ConfigData = new GrandOutputChannelConfigData();
            route.AddAction( new BinaryFileConfiguration( "All" ) { Path = subFolder } );
            def.ChannelsConfiguration = route;
            return def;
        }


        [Test]
        public void GrandOutputHasSameCompressedAndUncompressedLogs()
        {
            string rootPath = SystemActivityMonitor.RootLogPath + @"\GrandOutputGzip";

            TestHelper.CleanupFolder( rootPath );

            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"
                <GrandOutputConfiguration GlobalDefaultFilter=""Release"" >
                    <Channel>
                        <Add Type=""BinaryFile"" Name=""GzipGlobalCatch"" Path=""" + rootPath + @"\OutputGzip"" MaxCountPerFile=""200000"" UseGzipCompression=""True"" />
                        <Add Type=""BinaryFile"" Name=""RawGlobalCatch"" Path=""" + rootPath + @"\OutputRaw"" MaxCountPerFile=""200000"" UseGzipCompression=""False"" />
                    </Channel>
                </GrandOutputConfiguration>"
                ).Root, TestHelper.ConsoleMonitor ) );
            Assert.That( c.ChannelsConfiguration.Configurations.Count, Is.EqualTo( 2 ) );

            using( GrandOutput g = new GrandOutput() )
            {
                Assert.That( g.SetConfiguration( c, TestHelper.ConsoleMonitor ), Is.True );

                var taskA = Task.Factory.StartNew<int>( () => { DumpMonitorOutput( CreateMonitorAndRegisterGrandOutput( "Task A", g ) ); return 1; } );
                var taskB = Task.Factory.StartNew<int>( () => { DumpMonitorOutput( CreateMonitorAndRegisterGrandOutput( "Task B", g ) ); return 1; } );
                var taskC = Task.Factory.StartNew<int>( () => { DumpMonitorOutput( CreateMonitorAndRegisterGrandOutput( "Task C", g ) ); return 1; } );

                Task.WaitAll( taskA, taskB, taskC );
            }

            string[] gzipCkmons = TestHelper.WaitForCkmonFilesInDirectory( rootPath + @"\OutputGzip", 1 );
            string[] rawCkmons = TestHelper.WaitForCkmonFilesInDirectory( rootPath + @"\OutputRaw", 1 );

            Assert.That( gzipCkmons, Has.Length.EqualTo( 1 ) );
            Assert.That( rawCkmons, Has.Length.EqualTo( 1 ) );

            FileInfo gzipCkmonFile = new FileInfo( gzipCkmons.Single() );
            FileInfo rawCkmonFile = new FileInfo( rawCkmons.Single() );

            Assert.That( gzipCkmonFile.Exists, Is.True );
            Assert.That( rawCkmonFile.Exists, Is.True );

            // Test file size
            Assert.That( gzipCkmonFile.Length, Is.LessThan( rawCkmonFile.Length ) );

            // Test de-duplication between Gzip and non-Gzip
            MultiLogReader mlr = new MultiLogReader();
            var fileList = mlr.Add( new string[] { gzipCkmonFile.FullName, rawCkmonFile.FullName } );
            Assert.That( fileList, Has.Count.EqualTo( 2 ) );

            var map = mlr.GetActivityMap();

            Assert.That( map.Monitors.Count, Is.EqualTo( 3 ) );

        }

        static IActivityMonitor CreateMonitorAndRegisterGrandOutput( string topic, GrandOutput go )
        {
            var m = new ActivityMonitor( topic );
            go.Register( m );
            return m;
        }

        static void DumpMonitorOutput( IActivityMonitor monitor )
        {
            Exception exception1;
            Exception exception2;

            try
            {
                throw new InvalidOperationException( "Exception!" );
            }
            catch( Exception e )
            {
                exception1 = e;
            }

            try
            {
                throw new InvalidOperationException( "Inception!", exception1 );
            }
            catch( Exception e )
            {
                exception2 = e;
            }

            for( int i = 0; i < 5; i++ )
            {
                using( monitor.OpenTrace().Send( "Dump output loop {0}", i ) )
                {
                    for( int j = 0; j < 1000; j++ )
                    {
                        monitor.Trace().Send( "Trace log! {0}", j );
                        monitor.Info().Send( "Info log! {0}", j );
                        monitor.Warn().Send( "Warn log! {0}", j );
                        monitor.Error().Send( "Error log! {0}", j );
                        monitor.Error().Send( "Fatal log! {0}", j );

                        monitor.Error().Send( exception2, "Exception log! {0}", j );

                    }
                }
            }
        }

    }
}
