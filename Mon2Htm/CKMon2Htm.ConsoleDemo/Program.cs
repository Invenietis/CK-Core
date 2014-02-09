using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm.ConsoleDemo
{
    class Program
    {
        static void Main( string[] args )
        {
            PrepareDefaultGrandOutput(); // This one writes into ./HtmlGenerator/.

            // This is the default monitor.
            IActivityMonitor m = new ActivityMonitor();
            m.SetFilter( LogFilter.Debug );
            m.Output.RegisterClient( new ActivityMonitorConsoleClient() );

            // Analyses arguments.
            if( args.Length >= 1 )
            {
                using( MultiLogReader r = new MultiLogReader() )
                {
                    List<string> ckmonFiles = new List<string>();

                    using( m.OpenTrace().Send( "Loading from arguments:" ) )
                    {
                        foreach( string path in args )
                        {
                            if( Directory.Exists( path ) )
                            {
                                m.Trace().Send( "Directory: {0}", path );
                                ckmonFiles.AddRange( GetCkmonFilesFromDirectory( path ) );
                            }
                            else if( File.Exists( path ) && Path.GetExtension( path ) == @".ckmon" )
                            {
                                m.Trace().Send( "File: {0}", path );
                                ckmonFiles.Add( path );
                            }
                        }
                    }

                    using( m.OpenTrace().Send( "Log files loaded:" ) )
                    {
                        foreach( var f in ckmonFiles ) m.Trace().Send( f );
                    }

                    if( ckmonFiles.Count == 0 )
                    {
                        m.Fatal().Send( "No log files were found at the designated paths." );
                        PressAnyKeyToExit();
                    }
                    
                    string htmlDirectoryName = String.Format( "ckmon-{0}-html", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") );
                    string htmlDirectoryPath = Path.Combine(Environment.CurrentDirectory, htmlDirectoryName);

                    m.Info().Send("Creating HTML structure into: {0}", htmlDirectoryPath);

                    Directory.CreateDirectory( htmlDirectoryPath );

                    var rawLogFiles = r.Add( ckmonFiles );
                    foreach(var file in rawLogFiles)
                    {
                        if( file.Error != null )
                        {
                            m.Warn().Send( file.Error, "Exception encountered when reading file: {0}", file.FileName );
                        }
                    }

                    string indexHtmlPath = HtmlGenerator.CreateFromActivityMap( r.GetActivityMap(), m, 5000, htmlDirectoryPath );

                    if( indexHtmlPath != null && File.Exists( indexHtmlPath ) )
                    {
                        // Run result in browser
                        Process.Start( indexHtmlPath );
                    }
                    else
                    {
                        PressAnyKeyToExit();
                    }
                }

            }
            else
            {
                string rootPath = SystemActivityMonitor.RootLogPath;
                string directoryName = "DummyEntries";
                string directoryPath = Path.Combine( rootPath, directoryName );

                m.Warn().Send( "The program was called without a directory. What follows are dummy entries." );

                IActivityMonitor dummyMonitor = new ActivityMonitor();
                dummyMonitor.SetFilter( LogFilter.Debug );

                m.Trace().Send( "Writing entries..." );
                using( GrandOutput go = PrepareNewGrandOutputFolder( m, directoryName ) )
                {
                    go.Register( dummyMonitor );
                    SendDummyMonitorEvents( dummyMonitor );
                    m.Trace().Send( "Closing GrandOutput..." );
                }

                string indexHtmlPath = HtmlGenerator.CreateFromLogDirectory( directoryPath, m, 5000, true );

                if( indexHtmlPath != null )
                {
                    Process.Start( indexHtmlPath );
                }
                else
                {
                    PressAnyKeyToExit();
                }

            }
        }

        private static IEnumerable<string> GetCkmonFilesFromDirectory( string path )
        {
            string[] ckmonFiles = Directory.GetFiles( path, "*.ckmon", SearchOption.AllDirectories );
            return ckmonFiles;
        }

        private static void SendDummyMonitorEvents( IActivityMonitor m )
        {
            string dummyLongText = @"{
	""title"": ""Example Schema"",
	""type"": ""object"",
	""properties"": {
		""firstName"": {
			""type"": ""string""
		},
		""lastName"": {
			""type"": ""string""
		},
		""age"": {
			""description"": ""Age in years"",
			""type"": ""integer"",
			""minimum"": 0
		}
	},
	""required"": [""firstName"", ""lastName""]
}
";
            m.Trace().Send( dummyLongText );
            for( int i = 0; i < 30; i++ ) m.Trace().Send( "Trace Entry {0}", i );
            for( int i = 0; i < 30; i++ ) m.Info().Send( "Info Entry {0}", i );
            for( int i = 0; i < 30; i++ ) m.Warn().Send( "Warn Entry {0}", i );
            for( int i = 0; i < 30; i++ ) m.Error().Send( "Error Entry {0}", i );
            for( int i = 0; i < 30; i++ ) m.Fatal().Send( "Fatal Entry {0}", i );

            using( m.OpenFatal().Send( "Fatal group" ) )
            {
                for( int i = 0; i < 30; i++ ) m.Fatal().Send( "Fatal Entry {0}", i );
                using( m.OpenError().Send( "Error group" ) )
                {
                    for( int i = 0; i < 30; i++ ) m.Error().Send( "Error Entry {0}", i );
                    using( m.OpenWarn().Send( "Warn group" ) )
                    {
                        for( int i = 0; i < 30; i++ ) m.Warn().Send( "Warn Entry {0}", i );
                        using( m.OpenInfo().Send( "Info group" ) )
                        {
                            for( int i = 0; i < 30; i++ ) m.Info().Send( "Info Entry {0}", i );
                            using( m.OpenTrace().Send( "Trace group" ) )
                            {
                                for( int i = 0; i < 30; i++ ) m.Trace().Send( "Trace Entry {0}", i );
                            }
                        }
                    }
                    m.CloseGroup( "This is a conclusion..." );
                }
            }

            List<Exception> dummyExceptions = new List<Exception>();

            Exception e1 = new Exception( "Simple exception" );
            Exception e2 = new IOException( "Simple IO exception with Inner Exception", e1 );

            Exception e3 = new EndOfStreamException( "EOS exception with Inner", e1 );

            Exception e4 = new AggregateException( "Aggregate exception", new[] { e2, e3 } );

            dummyExceptions.Add( e1 );
            dummyExceptions.Add( e2 );
            dummyExceptions.Add( e3 );
            dummyExceptions.Add( e4 );

            foreach( var exception in dummyExceptions )
            {
                try
                {
                    throw exception;
                }
                catch( Exception e )
                {
                    m.Error().Send( e, "Exception log message ({0})", e.Message );
                }
            }
        }

        private static void PressAnyKeyToExit( int returnCode = 0 )
        {
            Console.WriteLine( "Press any key to exit." );
            Console.ReadKey();
            Environment.Exit( returnCode );
        }

        private static void PrepareDefaultGrandOutput()
        {
            GrandOutput go = GrandOutput.EnsureActiveDefaultWithDefaultSettings();
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            c.Load( XDocument.Parse( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""All"" Path=""./HtmlGenerator/"" />
    </Channel>
</GrandOutputConfiguration>" ).Root, new SystemActivityMonitor( false, null ) );


            bool result = go.SetConfiguration( c );
            Debug.Assert( result );
        }

        private static GrandOutput PrepareSingleChannelGrandOutput(string directoryName = "default", int maxEntriesPerFile = 20000)
        {
            GrandOutput go = new GrandOutput();
            GrandOutputConfiguration c = new GrandOutputConfiguration();
            c.Load( XDocument.Parse( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" Path=""./{0}"" MaxEntriesPerFile=""{1}"" />
    </Channel>
</GrandOutputConfiguration>" ).Root, new SystemActivityMonitor( false, null ) );


            bool result = go.SetConfiguration( c );
            Debug.Assert( result );

            return go;
        }

        public static GrandOutput PrepareNewGrandOutputFolder( IActivityMonitor monitor, string grandOutputFolderName = "Default", int entriesPerFile = 1000 )
        {
            GrandOutput go = new GrandOutput();
            GrandOutputConfiguration c = new GrandOutputConfiguration();

            bool result;

            result = c.Load( CreateGrandOutputConfiguration( grandOutputFolderName, entriesPerFile ), monitor );
            Debug.Assert( result, "Could load configuration XML" );

            result = go.SetConfiguration( c );
            Debug.Assert( result, "Could load configuration XML" );

            return go;
        }

        public static XElement CreateGrandOutputConfiguration( string grandOutputDirectoryName, int entriesPerFile )
        {
            string pathEntry = String.Format( @"Path=""./{0}/""", grandOutputDirectoryName );
            return XDocument.Parse(
                    String.Format( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" {0} MaxCountPerFile=""{1}"" />
    </Channel>
</GrandOutputConfiguration>",
                            pathEntry, entriesPerFile ) ).Root;
        }
    }
}
