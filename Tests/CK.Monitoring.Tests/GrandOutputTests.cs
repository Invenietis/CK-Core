#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\GrandOutputTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
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
            Debug.Assert( def.AppDomainDefaultFilter == null );
            var route = new RouteConfiguration();
            route.ConfigData = new GrandOutputChannelConfigData();
            route.AddAction( new BinaryFileConfiguration( "All" ) { Path = subFolder } );
            def.ChannelsConfiguration = route;
            return def;
        }

        class RunInAnotherAppDomain : MarshalByRefObject, ISponsor
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
                GrandOutput.GrandOutputMinimalFilter = LogFilter.Debug;
                _callerMonitor = new ActivityMonitor( false );
                _bridgeToCallerMonitor = _callerMonitor.Output.CreateStrongBridgeTo( bridgeToConsole );
                GrandOutput.EnsureActiveDefaultWithDefaultSettings( _callerMonitor );
                _localMonitor = new ActivityMonitor();
            }

            public void RunNoConfigFileDefaultsToDebug()
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

            internal void SendLine( LogLevel level, string msg )
            {
                if( _localMonitor.ShouldLogLine( level ) ) _localMonitor.UnfilteredLog( ActivityMonitor.Tags.Empty, level | LogLevel.IsFiltered, msg, _localMonitor.NextLogTime(), null );
            }

            public void RunWithConfigFileReleaseFilter()
            {
                _localMonitor.Trace().Send( "ConfigFileReleaseFilter-NOSHOW" );
                _localMonitor.Info().Send( "ConfigFileReleaseFilter-NOSHOW" );
                _localMonitor.Warn().Send( "ConfigFileReleaseFilter-NOSHOW" );
                _localMonitor.Error().Send( "ConfigFileReleaseFilter1" );
                _localMonitor.Fatal().Send( "ConfigFileReleaseFilter2" );
            }

            public void Close()
            {
                if( _bridgeToCallerMonitor != null )
                {
                    _bridgeToCallerMonitor.Dispose();
                    _bridgeToCallerMonitor = null;
                }
            }

            public override object InitializeLifetimeService()
            {
                ILease lease = (ILease)base.InitializeLifetimeService();
                if( lease.CurrentState == LeaseState.Initial )
                {
                    lease.Register( this );
                }
                return lease;
            }

            public TimeSpan Renewal( ILease lease )
            {
                return _bridgeToCallerMonitor != null ? lease.InitialLeaseTime : TimeSpan.Zero;
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

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Debug ), "Default configuration is Debug." );

                exec.RunNoConfigFileDefaultsToDebug();

                // Gets the base number of configuration attempt.
                int confCount = exec.GetConfigurationAttemptCount();

                // 1 - Sets GrandOutputConfig to Release.
                SetDomainConfigTextFile( @"
<GrandOutputConfiguration>
    <Channel MinimalFilter=""Release"">
        <Add Type=""BinaryFile"" Name=""AllFromConfig""  Path=""GrandOutputDefault"" />
    </Channel>
</GrandOutputConfiguration>" );

                exec.WaitForNextConfiguration( confCount + 1 );

                Assert.That( exec.GetConfigurationAttemptCount(), Is.EqualTo( confCount + 1 ) );

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Release ) );
                exec.SendLine( LogLevel.Warn, "NOSHOW (since it now defaults to Release filter)" );
                exec.RunWithConfigFileReleaseFilter();

                // 2 - Removes GrandOutputConfig file.
                SetDomainConfigTextFile( null );

                exec.WaitForNextConfiguration( confCount + 2 );
                Assert.That( exec.GetConfigurationAttemptCount(), Is.EqualTo( confCount + 2 ) );

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Debug ) );
                exec.SendLine( LogLevel.Trace, "TraceSinceDebug1" );

                // 3 - Sets GrandOutputConfig to Terse.
                SetDomainConfigTextFile( @"
<GrandOutputConfiguration>
    <Channel MinimalFilter=""Terse"">
        <Add Type=""BinaryFile"" Name=""AllFromConfig""  Path=""GrandOutputDefault"" />
    </Channel>
</GrandOutputConfiguration>" );

                exec.WaitForNextConfiguration( confCount + 3 );
                Assert.That( exec.GetConfigurationAttemptCount(), Is.EqualTo( confCount + 3 ) );

                exec.SendLine( LogLevel.Warn, "NOSHOW (since it now defaults to Terse filter)" );
                exec.SendLine( LogLevel.Error, "ErrorWithTerseFilter1" );
                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Terse ) );

                // 4 - Renames GrandOutputConfig: it disapeared.
                SetDomainConfigTextFile( "rename" );

                exec.WaitForNextConfiguration( confCount + 4 );
                Assert.That( exec.GetConfigurationAttemptCount(), Is.EqualTo( confCount + 4 ) );

                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Debug ) );
                exec.SendLine( LogLevel.Trace, "TraceSinceDebug2" );

                // 5 - Restores the file.
                SetDomainConfigTextFile( "renameBack" );

                exec.WaitForNextConfiguration( confCount + 5 );
                Assert.That( exec.GetConfigurationAttemptCount(), Is.EqualTo( confCount + 5 ) );
                Assert.That( exec.GetLocalMonitorActualFilter(), Is.EqualTo( LogFilter.Terse ) );
                exec.SendLine( LogLevel.Warn, "NOSHOW (since it now defaults to Terse filter)" );
                exec.SendLine( LogLevel.Error, "ErrorWithTerseFilter2" );

            }
            finally
            {
                try { exec.Close(); }
                catch { }
                AppDomain.Unload( domain );
            }
            Thread.Sleep( 200 );
            List<StupidStringClient> logs = TestHelper.ReadAllLogs( new DirectoryInfo( RunInAnotherAppDomain.DomainRootLogPath + "GrandOutputDefault" ), false );

            Assert.That( logs.Count, Is.EqualTo( 6 ), "It contains the test monitor but also the monitoring of the reconfiguration due to the 5 file changes." );
            CollectionAssert.AreEqual(
                new[] { "NoConfigFile1", 
                        "NoConfigFile2", 
                        "NoConfigFile3", 
                        "NoConfigFile4", 
                        "NoConfigFile5", 
                        "ConfigFileReleaseFilter1", 
                        "ConfigFileReleaseFilter2", 
                        "TraceSinceDebug1",  
                        "ErrorWithTerseFilter1", 
                        "TraceSinceDebug2",
                        "ErrorWithTerseFilter2" },
                logs[0].Entries.Select( e => e.Text ), StringComparer.OrdinalIgnoreCase );
        }

        [Test]
        public void GrandOutputHasSameCompressedAndUncompressedLogs()
        {
            string rootPath = SystemActivityMonitor.RootLogPath + @"\GrandOutputGzip";

            TestHelper.CleanupFolder( rootPath );

            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( XDocument.Parse( @"
                <GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
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

        private static void SetDomainConfigTextFile( string config )
        {
            if( config != null )
            {
                Thread.Sleep( 100 );
                if( config.StartsWith( "rename" ) )
                {
                    if( config == "rename" )
                        File.Move( RunInAnotherAppDomain.DomainGrandOutputConfig, RunInAnotherAppDomain.DomainRootLogPath + "rename" );
                    else File.Move( RunInAnotherAppDomain.DomainRootLogPath + "rename", RunInAnotherAppDomain.DomainGrandOutputConfig );
                }
                else File.WriteAllText( RunInAnotherAppDomain.DomainGrandOutputConfig, config );
            }
            else File.Delete( RunInAnotherAppDomain.DomainGrandOutputConfig );
        }

    }
}
