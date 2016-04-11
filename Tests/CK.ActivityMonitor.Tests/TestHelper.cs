using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
{
    class TestAttribute : Xunit.FactAttribute
    {
    }

    static class TestHelper
    {
        static string _testFolder;
        static string _solutionFolder;
        
        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            // Do not pollute the console by default...
            // ... but this may be useful sometimes: LogsToConsole does the job.
            _console = new ActivityMonitorConsoleClient();
        }

        public static IActivityMonitor ConsoleMonitor
        {
            get { return _monitor; }
        }

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value ) _monitor.Output.RegisterUniqueClient( c => c == _console, () => _console );
                else _monitor.Output.UnregisterClient( _console );
            }
        }

        #if NET451 || NET46
        /// <summary>
        /// Use reflection to actually set <see cref="System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> to 5 milliseconds.
        /// This triggers an immediate polling from the internal .Net framework LeaseManager.
        /// Note that the LeaseManager is per AppDomain.
        /// </summary>
        public static void SetRemotingLeaseManagerVeryShortPollTime()
        {
            System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime = TimeSpan.FromMilliseconds( 5 );
            object remotingData = typeof( AppDomain ).GetProperty( "RemotingData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod().Invoke( System.Threading.Thread.GetDomain(), null );
            if( remotingData != null )
            {
                object leaseManager = remotingData.GetType().GetProperty( "LeaseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod().Invoke( remotingData, null );
                if( leaseManager != null )
                {
                    System.Threading.Timer timer = (System.Threading.Timer)leaseManager.GetType().GetField( "leaseTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue( leaseManager );
                    Assert.That( timer, Is.Not.Null );
                    timer.Change( 0, -1 );
                }
            }
        }
        #endif

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitializePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitializePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            CleanupFolder( TestFolder );
        }

        public static void CleanupFolder( string folder )
        {
            int tryCount = 0;
            for( ; ; )
            {
                try
                {
                    if( Directory.Exists( folder ) ) Directory.Delete( folder, true );
                    Directory.CreateDirectory( folder );
                    File.WriteAllText( Path.Combine( folder, "TestWrite.txt" ), "Test write works." );
                    File.Delete( Path.Combine( folder, "TestWrite.txt" ) );
                    return;
                }
                catch( Exception ex )
                {
                    if( ++tryCount == 20 ) throw;
                    ConsoleMonitor.Info().Send( ex, "While cleaning up test directory. Retrying." );
                    System.Threading.Thread.Sleep( 100 );
                }
            }
        }

        static void InitializePaths()
        {
            if( _solutionFolder != null ) return;
            #if NET451
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            #else
            string p = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath;
            #endif
            do
            {
                p = Path.GetDirectoryName( p );
            }
            while( !File.Exists( Path.Combine( p, "CK-Core.sln" ) ) );
            _solutionFolder = p;
            _testFolder = Path.Combine( _solutionFolder, "Tests", "CK.ActivityMonitor.Tests", "TestFolder" );
            CleanupTestFolder();
        }

    }
}
