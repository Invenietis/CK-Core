using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CK.Core;
using NUnit.Framework.Constraints;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
#if CSPROJ
    static class Does
    {
        public static SubstringConstraint Contain( string expected ) => Is.StringContaining( expected );

        public static EndsWithConstraint EndWith( string expected ) => Is.StringEnding( expected );

        public static StartsWithConstraint StartWith( string expected ) => Is.StringStarting( expected );

        public static ConstraintExpression Not => Is.Not;

        public static SubstringConstraint Contain( this ConstraintExpression @this, string expected ) => @this.StringContaining( expected );
    }
#else
    class TestAttribute : Xunit.FactAttribute
    {
    }
#endif

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

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitalizePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static List<StupidStringClient> ReadAllLogs( DirectoryInfo folder, bool recurse )
        {
            List<StupidStringClient> logs = new List<StupidStringClient>();
            ReplayLogs( folder, recurse, mon =>
            {
                var m = new ActivityMonitor( false );
                logs.Add( m.Output.RegisterClient( new StupidStringClient() ) );
                return m;
            }, TestHelper.ConsoleMonitor );
            return logs;
        }

        public static string[] WaitForCkmonFilesInDirectory( string directoryPath, int minFileCount )
        {
            string[] files;
            for( ; ; )
            {
                files = Directory.GetFiles( directoryPath, "*.ckmon", SearchOption.TopDirectoryOnly );
                if( files.Length >= minFileCount ) break;
                Thread.Sleep( 200 );
            }
            foreach( var f in files )
            {
                if( !FileUtil.CheckForWriteAccess( f, 3000 ) )
                {
                    throw new CKException( "CheckForWriteAccess exceeds 3000 milliseconds..." );
                }
            }
            return files;
        }

        public static void ReplayLogs( DirectoryInfo directory, bool recurse, Func<MultiLogReader.Monitor, ActivityMonitor> monitorProvider, IActivityMonitor m = null )
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

        public static void InitalizePaths()
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
            _testFolder = Path.Combine( _solutionFolder, "Tests", "CK.Monitoring.Tests", "TestFolder" );

            AppSettings.Default.Initialize( _ => null );
            SystemActivityMonitor.RootLogPath = Path.Combine( _testFolder, "RootLogPath" );
            ConsoleMonitor.Info().Send( "SolutionFolder is: {1}\r\nTestFolder is: {0}\r\nRootLogPath is: {2}", _testFolder, _solutionFolder, SystemActivityMonitor.RootLogPath );

            CleanupTestFolder();
        }

    }
}
