using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CKMon2Htm
{
    public class CKMon2Htm
    {
        public static readonly string CONTENT_RESOURCE_PREFIX = @"CKMon2Htm.Content.";
        public static readonly int LOG_ENTRIES_PER_PAGE = 300;
        public static readonly string TIME_FORMAT = @"yyyy-MM-ddTHH:mm:ss.fff";

        readonly IActivityMonitor _monitor;
        readonly Dictionary<MultiLogReader.Monitor, MonitorIndexInfo> _indexInfos;
        readonly string _outputDirectoryPath;
        readonly MultiLogReader.ActivityMap _activityMap;

        /// <summary>
        /// Creates an HTML view structure for a directory containing .ckmon files.
        /// </summary>
        /// <param name="directoryPath">Directory to use</param>
        /// <param name="activityMonitor">Activity monitor to use when logging events about generation.</param>
        /// <param name="recurse">Recurse into subdirectories</param>
        /// <param name="htmlOutputDirectory">Directory in which the HTML structure will be generated. Defaults to null, in which case an "html" folder will be created and used inside the logs' directoryPath.</param>
        /// <returns>Path of the HTML index. Null when no valid file could be loaded from directoryPath.</returns>
        public static string CreateFromLogDirectory( string directoryPath, IActivityMonitor activityMonitor, bool recurse = true, string htmlOutputDirectory = null )
        {
            if( activityMonitor == null ) throw new ArgumentNullException( "activityMonitor" );
            if( !Directory.Exists( directoryPath ) ) throw new DirectoryNotFoundException( "The given path does not exist, or is not a directory." );

            SearchOption searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            // Get all .ckmon files from directory
            IEnumerable<string> logFilePaths = Directory.GetFiles( directoryPath, "*.ckmon", searchOption );

            // Output HTML files to directory/html/
            if( String.IsNullOrWhiteSpace( htmlOutputDirectory ) ) htmlOutputDirectory = Path.Combine( directoryPath, "html" );

            CKMon2Htm g = new CKMon2Htm( logFilePaths, htmlOutputDirectory, activityMonitor );


            string indexPath = g.GenerateHtmlStructure();

            if( indexPath != null )
            {
                g.CopyContent();
            }

            return indexPath;
        }

        /// <summary>
        /// Copies additional content (JS, CSS) into the target directory path.
        /// </summary>
        private void CopyContent()
        {
            CopyContentResourceToFile( @"css\CkmonStyle.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\Reset.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\bootstrap-modal-bs3patch.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\bootstrap.min.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\bootstrap-modal.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\bootstrap-theme.min.css", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\ex-bg.png", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\info.svg", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\warn.svg", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\error.svg", _outputDirectoryPath );
            CopyContentResourceToFile( @"css\fatal.svg", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\bootstrap.min.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\jquery-2.0.3.min.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\bootstrap-modal.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\bootstrap-modalmanager.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\readmore.min.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\moment.min.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\moment-with-langs.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"js\ckmon.js", _outputDirectoryPath );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.eot", _outputDirectoryPath );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.svg", _outputDirectoryPath );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.ttf", _outputDirectoryPath );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.woff", _outputDirectoryPath );
        }

        /// <summary>
        /// Copies a named resource contained in the Content folder of this project, having type EmbeddedResource, into a target path.
        /// </summary>
        /// <param name="fileName">Named resource contained in the Content folder of this project, having type EmbeddedResource.</param>
        /// <param name="targetDirectory">Full filename to create.</param>
        private static void CopyContentResourceToFile( string fileName, string targetDirectory )
        {
            string resourceName = fileName.Replace( '/', '.' ).Replace( '\\', '.' );

            string resourcePath = CONTENT_RESOURCE_PREFIX + resourceName;

            string filename = Path.Combine( targetDirectory, fileName );

            using( Stream resource = typeof( CKMon2Htm ).Assembly.GetManifestResourceStream( resourcePath ) )
            {
                if( resource == null )
                {
                    throw new ArgumentException( "No such resource", "fileName" );
                }

                if( !Directory.Exists( Path.GetDirectoryName( filename ) ) ) Directory.CreateDirectory( Path.GetDirectoryName( filename ) );
                if( File.Exists( filename ) ) File.Delete( filename );
                using( Stream output = File.OpenWrite( filename ) )
                {
                    resource.CopyTo( output );
                }
            }
        }

        /// <summary>
        /// Initializes a new CKMon2Htm, adding monitor files into a MultiLogReader.
        /// </summary>
        /// <param name="logFilePaths">Directory containing ckmon files</param>
        /// <param name="htmlOutputDirectory">directory to output HTML into</param>
        /// <param name="activityMonitor">Monitor to use when reporting events about generation</param>
        private CKMon2Htm( IEnumerable<string> logFilePaths, string htmlOutputDirectory, IActivityMonitor activityMonitor )
        {
            Debug.Assert( logFilePaths != null );
            Debug.Assert( activityMonitor != null );
            Debug.Assert( !String.IsNullOrWhiteSpace( htmlOutputDirectory ) );

            MultiLogReader r = new MultiLogReader();

            _monitor = activityMonitor;
            _outputDirectoryPath = htmlOutputDirectory;
            _indexInfos = new Dictionary<MultiLogReader.Monitor, MonitorIndexInfo>();

            var logFiles = r.Add( logFilePaths );

            foreach( var logFile in logFiles )
            {
                if( logFile.Error != null )
                {
                    using( _monitor.OpenWarn().Send( "Error while adding file: {0}", logFile.FileName ) )
                    {
                        _monitor.Error().Send( logFile.Error );
                    }
                }
            }

            _activityMap = r.GetActivityMap();
        }

        /// <summary>
        /// Initializes a new CKMon2Htm, using an existing ActivityMap.
        /// </summary>
        /// <param name="_activityMap">ActivityMap</param>
        /// <param name="htmlOutputDirectory">directory to output HTML into</param>
        /// <param name="activityMonitor">Monitor to use when reporting events about generation</param>
        private CKMon2Htm( MultiLogReader.ActivityMap activityMap, string htmlOutputDirectory, IActivityMonitor activityMonitor )
        {
            Debug.Assert( activityMap != null );
            Debug.Assert( activityMonitor != null );
            Debug.Assert( !String.IsNullOrWhiteSpace( htmlOutputDirectory ) );

            _monitor = activityMonitor;
            _outputDirectoryPath = htmlOutputDirectory;
            _indexInfos = new Dictionary<MultiLogReader.Monitor, MonitorIndexInfo>();
            _activityMap = activityMap;

        }

        /// <summary>
        /// Creates the complete HTML structure of all Monitors contained in our MultiLogReader's ActivityMap,
        /// then generates an index.
        /// </summary>
        /// <returns>Null if no files could be loaded.</returns>
        private string GenerateHtmlStructure()
        {
            if( _activityMap.ValidFiles.Count == 0 )
            {
                _monitor.Warn().Send( "No valid log files could be loaded. Nothing will be done." );
                return null;
            }

            _monitor.Info().Send( "Generating HTML files in directory: '{0}'", _outputDirectoryPath );

            if( !Directory.Exists( _outputDirectoryPath ) ) Directory.CreateDirectory( _outputDirectoryPath );

            Dictionary<MultiLogReader.Monitor, IEnumerable<string>> monitorPages = new Dictionary<MultiLogReader.Monitor, IEnumerable<string>>();
            using( _monitor.OpenTrace().Send( "Writing monitors' HTML files" ) )
            {
                foreach( var monitor in _activityMap.Monitors )
                {
                    var monitorIndex = MonitorIndexInfo.IndexMonitor( monitor, LOG_ENTRIES_PER_PAGE );
                    _indexInfos.Add( monitor, monitorIndex );

                    _monitor.Trace().Send( "Indexing monitor: {0}", monitor.MonitorId.ToString() );
                    _monitor.Trace().Send( "Writing monitor: {0}", monitor.MonitorId.ToString() );
                    var logPages = CreateMonitorHtmlStructure( monitor, monitorIndex );
                    if( logPages != null ) monitorPages.Add( monitor, logPages );
                }
            }

            string indexPath = CreateIndex( _activityMap, monitorPages );

            return indexPath;
        }

        /// <summary>
        /// Creates the HTML structure of a single monitor into a paginated list of files.
        /// </summary>
        /// <param name="monitor">Monitor to use</param>
        /// <returns>List of HTML files created for this monitor.</returns>
        /// <remarks>
        /// This loops and stores the log entries of every page, then writes them to a file when changing pages.
        /// </remarks>
        private IEnumerable<string> CreateMonitorHtmlStructure( MultiLogReader.Monitor monitor, MonitorIndexInfo monitorIndex )
        {
            _monitor.Info().Send( "Generating HTML for monitor: {0}", monitor.ToString() );

            List<string> pageFilenames = new List<string>();
            List<ILogEntry> currentPageLogEntries = new List<ILogEntry>();

            IReadOnlyList<ILogEntry> openGroupsOnStart = new List<ILogEntry>().ToReadOnlyList(); // To fix
            List<ILogEntry> openGroupsOnEnd = new List<ILogEntry>();

            int currentPageNumber = 1;

            int totalEntryCount = monitorIndex.TotalEntryCount;
            int totalPageCount = monitorIndex.PageCount;

            var page = monitor.ReadFirstPage( monitor.FirstEntryTime, LOG_ENTRIES_PER_PAGE );

            do
            {
                foreach( var parentedLogEntry in page.Entries )
                {
                    var entry = parentedLogEntry.Entry;
                    currentPageLogEntries.Add( entry );

                    if( entry.LogType == LogEntryType.OpenGroup && !parentedLogEntry.IsMissing )
                    {
                        openGroupsOnEnd.Add( entry );
                    }
                    else if( entry.LogType == LogEntryType.CloseGroup && !parentedLogEntry.IsMissing )
                    {
                        openGroupsOnEnd.Remove( openGroupsOnEnd[openGroupsOnEnd.Count - 1] );
                    }

                    // Flush entries into HTML
                    if( currentPageLogEntries.Count >= LOG_ENTRIES_PER_PAGE )
                    {
                        _monitor.Info().Send( "Generating page {0}", currentPageNumber );
                        string pageName = GenerateLogPage( currentPageLogEntries, monitor, monitorIndex, currentPageNumber, openGroupsOnStart, openGroupsOnEnd.ToReadOnlyList(), totalPageCount );
                        currentPageNumber++;
                        currentPageLogEntries.Clear();

                        pageFilenames.Add( pageName );
                        openGroupsOnStart = openGroupsOnEnd.ToReadOnlyList();
                    }
                }

            } while( page.ForwardPage() > 0 );

            // Flush outstanding entries into HTML
            if( currentPageLogEntries.Count > 0 )
            {
                _monitor.Info().Send( "Generating outstanding page {0}", currentPageNumber );
                string pageName = GenerateLogPage( currentPageLogEntries, monitor, monitorIndex, currentPageNumber, openGroupsOnStart, openGroupsOnEnd.ToReadOnlyList(), totalPageCount );
                currentPageLogEntries.Clear();

                pageFilenames.Add( pageName );
            }

            return pageFilenames;
        }

        private static string GetMonitorPageFilename( MultiLogReader.Monitor monitor, int currentPageNumber )
        {
            string filename = String.Format( "{0}_{1}.html", monitor.MonitorId.ToString(), currentPageNumber );
            return filename;
        }

        private string GenerateLogPage( IEnumerable<ILogEntry> currentPageLogEntries,
            MultiLogReader.Monitor monitor,
            MonitorIndexInfo monitorIndex,
            int currentPageNumber,
            IReadOnlyList<ILogEntry> openGroupsOnStart,
            IReadOnlyList<ILogEntry> openGroupsOnEnd,
            int totalPages )
        {
            string filename = GetMonitorPageFilename( monitor, currentPageNumber );

            using( TextWriter tw = File.CreateText( Path.Combine( _outputDirectoryPath, filename ) ) )
            {
                WriteLogPageHeader( tw, monitor, currentPageNumber, totalPages );

                WriteLogGroupBreadcrumb( tw, monitor, openGroupsOnStart, monitorIndex );

                if( currentPageNumber > 1 ) WritePrevPageButton( tw, monitor, currentPageNumber );

                CkmonHtmlWriter.WriteEntries( tw, currentPageLogEntries, openGroupsOnStart );

                if( currentPageNumber < totalPages ) WriteNextPageButton( tw, monitor, currentPageNumber );

                WriteLogGroupBreadcrumb( tw, monitor, openGroupsOnEnd, monitorIndex, true );

                WriteLogPageFooter( tw, monitor, currentPageNumber, totalPages );
            }

            return filename;
        }

        private void WriteNextPageButton( TextWriter tw, MultiLogReader.Monitor monitor, int currentPageNumber )
        {
            tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Next page</a>", GetMonitorPageFilename( monitor, currentPageNumber + 1 ) );
        }

        private void WritePrevPageButton( TextWriter tw, MultiLogReader.Monitor monitor, int currentPageNumber )
        {
            tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Previous page</a>", GetMonitorPageFilename( monitor, currentPageNumber - 1 ) );
        }

        private void WriteLogGroupBreadcrumb( TextWriter tw, MultiLogReader.Monitor monitor, IReadOnlyList<ILogEntry> openGroupsOnStart, MonitorIndexInfo monitorIndex, bool reverse = false )
        {
            if( openGroupsOnStart == null ) return;
            tw.Write( @"<div class=""groupBreadcrumb"">" );
            if( !reverse )
            {
                foreach( var group in openGroupsOnStart )
                {
                    tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", CkmonHtmlWriter.GetClassNameOfLogLevel( group.LogLevel ) );
                    tw.Write( @"<p>{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-backward""></span></a></p>", group.Text, GetReferenceHref( monitor, monitorIndex, monitorIndex.Groups.GetByKey( group.LogTime ).OpenGroupTimestamp ) );
                }
                for( int i = 0; i < openGroupsOnStart.Count; i++ )
                {
                    tw.Write( @"</div>" );
                }
                tw.Write( @"<div class=""groupBreadcrumbSeparator""></div>" );
            }
            else
            {
                tw.Write( @"<div class=""groupBreadcrumbSeparator""></div>" );
                foreach( var group in openGroupsOnStart )
                {
                    tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", CkmonHtmlWriter.GetClassNameOfLogLevel( group.LogLevel ) );
                }
                foreach( var group in openGroupsOnStart.Reverse() )
                {
                    tw.Write( @"<p>{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-forward""></span></a></p>", group.Text, GetReferenceHref( monitor, monitorIndex, monitorIndex.Groups.GetByKey( group.LogTime ).CloseGroupTimestamp ) );
                    tw.Write( @"</div>" );
                }
            }
            tw.Write( @"</div>" );
        }

        public static string GetReferenceHref( MultiLogReader.Monitor monitor, MonitorIndexInfo monitorIndex, DateTimeStamp timestamp )
        {
            return String.Format( "{0}#{1}", GetMonitorPageFilename( monitor, monitorIndex.GetPageIndexOf( timestamp ) + 1 ), HttpUtility.UrlEncode( GetTimestampId( timestamp ) ) );
        }

        public static string GetTimestampId( DateTimeStamp t )
        {
            return t.ToBase64String().Replace( '=', '.' ).Replace( '+', '-' ).Replace( '/', '_' );
        }

        private void WriteLogPageHeader( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage, int pagecount )
        {
            tw.Write( HtmlHeader( String.Format( "Log: {0} - Page {1}", monitor.MonitorId.ToString(), currentPage ) ) );
        }

        private void WriteLogPageFooter( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage, int pagecount )
        {
            WriteMonitorPaginator( tw, monitor, currentPage, pagecount );
            tw.Write( HtmlFooter() );
        }

        private string CreateIndex( MultiLogReader.ActivityMap activityMap, Dictionary<MultiLogReader.Monitor, IEnumerable<string>> monitorPages )
        {
            string indexFilePath = Path.Combine( _outputDirectoryPath, @"index.html" );

            TextWriter tw = File.CreateText( indexFilePath );

            WriteIndex( activityMap, monitorPages, tw );

            tw.Close();

            return indexFilePath;
        }

        private void WriteIndex( MultiLogReader.ActivityMap activityMap, Dictionary<MultiLogReader.Monitor, IEnumerable<string>> monitorPages, TextWriter tw )
        {
            tw.Write( HtmlHeader( "Ckmon Index" ) );

            tw.Write( "<h1>ActivityMonitor log viewer</h1>" );
            tw.Write( String.Format( "<h3>Between {0} and {1}</h3>", activityMap.FirstEntryDate, activityMap.LastEntryDate ) );

            tw.Write( @"<h2>Monitors:</h2><table class=""monitorTable table table-striped table-bordered"">" );
            tw.Write( @"<thead><tr><th>Monitor ID</th><th>Started</th><th>Duration</th><th>Entries</th></tr></thead><tbody>" );

            var monitorList = activityMap.Monitors.ToList();
            monitorList.Sort( ( a, b ) => b.FirstEntryTime.CompareTo( a.FirstEntryTime ) );

            foreach( MultiLogReader.Monitor monitor in monitorList )
            {
                tw.Write( @"<tr class=""monitorEntry"">" );
                IEnumerable<string> monitorPageList = null;
                string href = monitor.MonitorId.ToString();

                if( monitorPages.TryGetValue( monitor, out monitorPageList ) )
                {
                    href = String.Format( @"<a href=""{1}"">{0}</a>", monitor.MonitorId.ToString(),
                        HttpUtility.UrlEncode( monitorPageList.First() ) );
                }

                tw.Write( String.Format( @"
<td class=""monitorId"">{0}</td>
<td class=""monitorTime""><span data-toggle=""tooltip"" title=""{6}"" rel=""tooltip""><span class=""startTime"">{1}</span></span></td>
<td class=""monitorTime""><span data-toggle=""tooltip"" title=""{7}"" rel=""tooltip""><span class=""endTime"">{2}</span></span></td>
<td>
    <div class=""warnCount entryCount"">{3}</div>
    <div class=""errorCount entryCount"">{4}</div>
    <div class=""fatalCount entryCount"">{5}</div>
    <div class=""totalCount entryCount"">Total: {8}</div>
</td>",
                    href,
                    monitor.FirstEntryTime.TimeUtc.ToString( TIME_FORMAT ),
                    monitor.LastEntryTime.TimeUtc.ToString( TIME_FORMAT ),
                    _indexInfos[monitor].TotalWarnCount,
                    _indexInfos[monitor].TotalErrorCount,
                    _indexInfos[monitor].TotalFatalCount,
                    String.Format( "First entry: {0}<br>Last entry: {0}", monitor.FirstEntryTime.TimeUtc.ToString( TIME_FORMAT ), monitor.LastEntryTime.TimeUtc.ToString( TIME_FORMAT ) ),
                    String.Format( "Monitor duration: {0}", (monitor.LastEntryTime.TimeUtc - monitor.FirstEntryTime.TimeUtc).ToString( "c" ) ),
                    _indexInfos[monitor].TotalEntryCount
                    ) );

                tw.Write( "</tr>" );
            }
            tw.Write( "</tbody></table>" );
            tw.Write( HtmlFooter() );
        }

        private static string HtmlHeader( string title )
        {
            title = HttpUtility.HtmlEncode( title );
            return String.Format( HTML_HEADER, title );
        }

        private static string HtmlFooter()
        {
            return HTML_FOOTER;
        }

        private static void WriteMonitorPaginator( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage, int totalPages )
        {
            Debug.Assert( currentPage <= totalPages );
            tw.Write( @"<ul class=""pagination"">" );

            if( currentPage > 1 )
            {
                tw.Write( @"<li><a href=""{0}"">&laquo;</a></li>", GetMonitorPageFilename( monitor, 1 ) );
                tw.Write( @"<li><a href=""{0}"">Prev</a></li>", GetMonitorPageFilename( monitor, currentPage - 1 ) );
            }
            else
            {
                tw.Write( @"<li class=""disabled""><a>&laquo;</a></li>" );
                tw.Write( @"<li class=""disabled""><a>Prev</a></li>" );
            }

            for( int i = 1; i <= totalPages; i++ )
            {
                if( i == currentPage )
                {
                    tw.Write( @"<li class=""active""><a>{0}</a></li>", i );
                }
                else
                {
                    tw.Write( @"<li><a href=""{0}"">{1}</a></li>", GetMonitorPageFilename( monitor, i ), i );
                }

            }

            if( currentPage < totalPages )
            {
                tw.Write( @"<li><a href=""{0}"">Next</a></li>", GetMonitorPageFilename( monitor, currentPage + 1 ) );
                tw.Write( @"<li><a href=""{0}"">&raquo;</a></li>", GetMonitorPageFilename( monitor, totalPages ) );
            }
            else
            {
                tw.Write( @"<li class=""disabled""><a>Next</a></li>" );
                tw.Write( @"<li class=""disabled""><a>&raquo;</a></li>" );
            }

            tw.Write( @"</ul>" );
        }

        static string HTML_HEADER =
            @"<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<title>{0}</title>

<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<!--[if lt IE 9]>
<script src=""https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js""></script>
<script src=""https://oss.maxcdn.com/libs/respond.js/1.3.0/respond.min.js""></script>
<![endif]-->
<link rel=""stylesheet"" type=""text/css"" href=""css/Reset.css"">
<link rel=""stylesheet"" type=""text/css"" href=""css/bootstrap.min.css"">
<link rel=""stylesheet"" type=""text/css"" href=""css/bootstrap-theme.min.css"">
<link rel=""stylesheet"" type=""text/css"" href=""css/bootstrap-modal-bs3patch.css"">
<link rel=""stylesheet"" type=""text/css"" href=""css/bootstrap-modal.css"">
<link rel=""stylesheet"" type=""text/css"" href=""css/CkmonStyle.css"">
</head>
<body>

    <div id=""wrap"">
        <header>
            <nav class=""navbar navbar-inverse navbar-fixed-top"" role=""navigation"">
                <div class=""container"">
                    <!-- Brand and toggle get grouped for better mobile display -->
                    <div class=""navbar-header"">
                        <button type=""button"" class=""navbar-toggle"" data-toggle=""collapse"" data-target=""#bs-top-navbar"">
                            <span class=""sr-only"">Menu</span>
                            <span class=""icon-bar""></span>
                            <span class=""icon-bar""></span>
                            <span class=""icon-bar""></span>
                        </button>
                        <a class=""navbar-brand"" href=""index.html"">Activity Monitor logs</a>
                    </div>

                    <!-- Collect the nav links, forms, and other content for toggling -->
                    <div class=""collapse navbar-collapse"" id=""bs-top-navbar"">
                        <ul class=""nav navbar-nav"">
                            <li><a href=""index.html"">Index</a></li>
                        </ul>
                    </div><!-- /.navbar-collapse -->
                </div>
            </nav>
        </header>

        <div class=""container"">
            <section>
";

        static string HTML_FOOTER =
            @"
            </section>
        </div>
    </div>

<script src=""js/jquery-2.0.3.min.js""></script>
<script src=""js/readmore.min.js""></script>
<script src=""js/bootstrap.min.js""></script>
<script src=""js/bootstrap-modalmanager.js""></script>
<script src=""js/bootstrap-modal.js""></script>
<script src=""js/moment-with-langs.js""></script>
<script src=""js/ckmon.js""></script>
<script type=""text/javascript"">
</script>
</body></html>";

        public static string CreateFromActivityMap( MultiLogReader.ActivityMap activityMap, IActivityMonitor m, string outputDirectoryPath )
        {
            CKMon2Htm g = new CKMon2Htm( activityMap, outputDirectoryPath, m );

            string indexPath = g.GenerateHtmlStructure();

            if( indexPath != null )
            {
                g.CopyContent();
            }

            return indexPath;
        }
    }
}
