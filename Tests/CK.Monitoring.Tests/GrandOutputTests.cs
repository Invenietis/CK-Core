using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            Replay( new DirectoryInfo( SystemActivityMonitor.RootLogPath + "ApplyEmptyAndDefaultConfig" ), true, mon => replayed, TestHelper.ConsoleMonitor );
            CollectionAssert.AreEqual( new[] { "<Missing log data>", "Show-1" }, c.Entries.Select( e => e.Text ), StringComparer.OrdinalIgnoreCase );
        }

        static GrandOutputConfiguration CreateDefaultConfig( string subFolder )
        {
            GrandOutputConfiguration def = new GrandOutputConfiguration();
            Debug.Assert( def.SourceFilterApplicationMode == SourceFilterApplyMode.None );
            Debug.Assert( def.AppDomainDefaultFilter == null );
            var route = new RouteConfiguration();
            route.ConfigData = new GrandOutputChannelConfigData();
            route.AddAction( new BinaryFileConfiguration( "All" ) { Path = SystemActivityMonitor.RootLogPath + subFolder } );
            def.ChannelsConfiguration = route;
            return def;
        }

        class RunInAnotherAppDomain : MarshalByRefObject
        {
            static ActivityMonitor _callerMonitor;
            static IDisposable _bridgeToCallerMonitor;
            static ActivityMonitor _localMonitor;

            public static string DomainRootLogPath { get { return FileUtil.NormalizePathSeparator( Path.Combine( TestHelper.GetTestFolder(), "GrandOutputConfigTests" ), true ); } }

            public static string DomainGrandOutputConfig { get { return DomainRootLogPath + "GrandOutput.config"; } }

            public void Initialize( ActivityMonitorBridgeTarget bridgeToConsole )
            {
                SystemActivityMonitor.RootLogPath = DomainRootLogPath;
                if( File.Exists( DomainGrandOutputConfig ) ) File.Delete( DomainGrandOutputConfig );
                _callerMonitor = new ActivityMonitor( false );
                _bridgeToCallerMonitor = _callerMonitor.Output.CreateStrongBridgeTo( bridgeToConsole );
                GrandOutput.EnsureActiveDefaultWithDefaultSettings( _callerMonitor );
                _localMonitor = new ActivityMonitor();
            }

            public void RunNoConfigFile()
            {
                _localMonitor.Trace().Send( "NoConfigFile1" );
                _localMonitor.Info().Send( "NoConfigFile2" );
                _localMonitor.Warn().Send( "NoConfigFile3" );
                _localMonitor.Error().Send( "NoConfigFile4" );
                _localMonitor.Fatal().Send( "NoConfigFile5" );
            }

            public LogFilter GetLocalMonitorActualFilter()
            {
                return _localMonitor.ActualFilter;
            }

            public int GetConfigurationAttemptCount()
            {
                return GrandOutput.Default.ConfigurationAttemptCount;
            }

            public void WaitForNextConfiguration( int configurationAttemptCount )
            {
                GrandOutput.Default.WaitForNextConfiguration( configurationAttemptCount, -1 );
            }

            internal void Trace( string msg )
            {
                _localMonitor.Trace().Send( msg );
            }

            public void RunWithConfigFileMonitorFilter()
            {
                _localMonitor.Trace().Send( "ConfigFileMonitorFilter-NOSHOW" );
                _localMonitor.Info().Send( "ConfigFileMonitorFilter-NOSHOW" );
                _localMonitor.Warn().Send( "ConfigFileMonitorFilter1" );
                _localMonitor.Error().Send( "ConfigFileMonitorFilter2" );
                _localMonitor.Fatal().Send( "ConfigFileMonitorFilter3" );
            }

            public void Close()
            {
                _bridgeToCallerMonitor.Dispose();
            }

        }

        [Test]
        public void DefaultConfiguration()
        {
            TestHelper.CleanupFolder( RunInAnotherAppDomain.DomainRootLogPath );

            AppDomain domain;
            RunInAnotherAppDomain exec;
            CreateDomainAndExecutor( out domain, out exec );

            try
            {
                exec.Initialize( TestHelper.ConsoleMonitor.Output.BridgeTarget );

                exec.RunNoConfigFile();

                int confCount = exec.GetConfigurationAttemptCount();

                SetDomainConfigAndWaitForChanged( String.Format( @"
<GrandOutputConfiguration >
    <Channel MinimalFilter=""Monitor"">
        <Add Type=""BinaryFile"" Name=""AllFromConfig""  Path=""{0}GrandOutputDefault"" />
    </Channel>
</GrandOutputConfiguration>", RunInAnotherAppDomain.DomainRootLogPath ) );


                exec.WaitForNextConfiguration( confCount + 1 );

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Undefined ) );
                
                // This call triggers the update of the MinimalFilter.
                // For performance reason, changes to the GrandOutput are not synchronously propagated to the Monitor by the GrandOutputClient.
                // Changes (MinimalFilter or new Route obtention) are delayed until the next time the monitor is solicited.
                exec.Trace( "UpdateMinimalFilter" );

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Monitor ) );
                
                exec.RunWithConfigFileMonitorFilter();
            }
            finally
            {
                try { exec.Close(); } catch {}
                AppDomain.Unload( domain );
            }

            List<StupidStringClient> logs = new List<StupidStringClient>();
            Replay( new DirectoryInfo( RunInAnotherAppDomain.DomainRootLogPath + "GrandOutputDefault" ), false, mon =>
                {
                    var m = new ActivityMonitor( false );
                    logs.Add( m.Output.RegisterClient( new StupidStringClient() ) );
                    return m;
                }, TestHelper.ConsoleMonitor );

            Assert.That( logs.Count, Is.EqualTo( 2 ), "It contains the test monitor but also the monitoring of the reconfiguration due to the file changed." );
            CollectionAssert.AreEqual(
                new[] { "NoConfigFile1", "NoConfigFile2", "NoConfigFile3", "NoConfigFile4", "NoConfigFile5", "UpdateMinimalFilter", "ConfigFileMonitorFilter1", "ConfigFileMonitorFilter2", "ConfigFileMonitorFilter3" }, 
                logs[0].Entries.Select( e => e.Text ), StringComparer.OrdinalIgnoreCase );
        }

        private static void CreateDomainAndExecutor( out AppDomain domain, out RunInAnotherAppDomain exec )
        {
            AppDomainSetup setup = new AppDomainSetup()
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath
            };
            domain = AppDomain.CreateDomain( "GrandOutputConfigTests", null, setup );
            exec = (RunInAnotherAppDomain)domain.CreateInstanceAndUnwrap( typeof( RunInAnotherAppDomain ).Assembly.FullName, typeof( RunInAnotherAppDomain ).FullName );
        }

        private static void SetDomainConfigAndWaitForChanged( string config )
        {
            if( config != null )
            {
                if( config == "rename" ) File.Move( RunInAnotherAppDomain.DomainGrandOutputConfig, RunInAnotherAppDomain.DomainRootLogPath + Guid.NewGuid().ToString() );
                else File.WriteAllText( RunInAnotherAppDomain.DomainGrandOutputConfig, config );
            }
            else File.Delete( RunInAnotherAppDomain.DomainGrandOutputConfig );
        }

        static void Replay( DirectoryInfo directory, bool recurse, Func<MultiLogReader.Monitor, ActivityMonitor> monitorProvider, IActivityMonitor m = null )
        {
            var reader = new MultiLogReader();
            using( m != null ? m.OpenTrace().Send( "Reading files from '{0}' {1}.", directory.FullName, recurse ? "(recursive)" : null ) : null )
            {
                var files = reader.Add( directory.EnumerateFiles( "*.ckmon", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ).Select( f => f.FullName ) );
                if( files.Count == 0 )
                {
                    if( m != null ) m.Warn().Send( "No *.ckmon files found!" );
                }
                else
                {
                    var monitors = reader.GetActivityMap().Monitors;
                    if( m != null )
                    {
                        m.Trace().Send( String.Join( Environment.NewLine, files ) );
                        m.CloseGroup( String.Format( "Found {0} file(s) containing {1} monitor(s).", files.Count, monitors.Count ) );
                        m.OpenTrace().Send( "Extracting entries." );
                    }
                    foreach( var mon in monitors )
                    {
                        var replay = monitorProvider( mon );
                        if( replay == null )
                        {
                            if( m != null ) m.Info().Send( "Skipping activity from '{0}'.", mon.MonitorId );
                        }
                        else
                        {
                            mon.Replay( replay, m );
                        }
                    }
                }
            }
        }


    }
}
