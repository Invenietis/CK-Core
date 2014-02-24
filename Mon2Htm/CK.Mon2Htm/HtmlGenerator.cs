using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    /// <summary>
    /// Utility class to generate an HTML structure from .ckmon binary log files.
    /// See: <see cref="CreateFromLogDirectory()"/> or <see cref="CreateFromActivityMap()"/>.
    /// </summary>
    public class HtmlGenerator
    {
        /// <summary>
        /// Resource prefix for content files.
        /// </summary>
        public static readonly string CONTENT_RESOURCE_PREFIX = @"CK.Mon2Htm.Content.";

        /// <summary>
        /// Time format used in index page.
        /// </summary>
        /// <remarks>
        /// This is parsed client-side by moment.js.
        /// </remarks>
        public static readonly string TIME_FORMAT = @"yyyy-MM-ddTHH:mm:ss.fff";

        readonly IActivityMonitor _monitor;
        readonly Dictionary<MultiLogReader.Monitor, MonitorIndexInfo> _indexInfos;
        readonly string _outputDirectoryPath;
        readonly MultiLogReader.ActivityMap _activityMap;
        readonly int _logEntryCountPerPage;
        readonly string _resourcesDirectoryPath;

        /// <summary>
        /// Creates an HTML view structure from a directory containing .ckmon files.
        /// </summary>
        /// <param name="directoryPath">Directory to scan for .ckmon files</param>
        /// <param name="activityMonitor">Activity monitor to use when logging events about generation.</param>
        /// <param name="recurse">Recurse into subdirectories when scanning for .ckmon files.</param>
        /// <param name="htmlOutputDirectory">Directory in which the HTML structure will be generated. Defaults to null, in which case an "html" folder will be created and used inside the logs' directoryPath.</param>
        /// <param name="logEntryCountPerPage">How many entries to write on every log page.</param>
        /// <returns>Full path of created index.html. Null when no valid file could be loaded from directoryPath.</returns>
        public static string CreateFromLogDirectory( string directoryPath, IActivityMonitor activityMonitor, int logEntryCountPerPage, bool recurse = true, string htmlOutputDirectory = null )
        {
            if( activityMonitor == null ) throw new ArgumentNullException( "activityMonitor" );
            if( !Directory.Exists( directoryPath ) ) throw new DirectoryNotFoundException( "The given path does not exist, or is not a directory." );

            SearchOption searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            // Get all .ckmon files from directory
            IEnumerable<string> logFilePaths = Directory.GetFiles( directoryPath, "*.ckmon", searchOption );

            // Output HTML files to directory/html/
            if( String.IsNullOrWhiteSpace( htmlOutputDirectory ) ) htmlOutputDirectory = Path.Combine( directoryPath, "html" );

            // Read directory files here.
            MultiLogReader r = new MultiLogReader();
            var logFiles = r.Add( logFilePaths );

            foreach( var logFile in logFiles )
            {
                if( logFile.Error != null )
                {
                    using( activityMonitor.OpenWarn().Send( "Error while adding file: {0}", logFile.FileName ) )
                    {
                        activityMonitor.Error().Send( logFile.Error );
                    }
                }
            }

            var activityMap = r.GetActivityMap();

            HtmlGenerator g = new HtmlGenerator( activityMap, htmlOutputDirectory, activityMonitor, logEntryCountPerPage, String.Empty );
            string indexPath = g.GenerateHtmlStructure();

            if( indexPath != null )
            {
                g.CopyResourcesToIndexDirectory();
            }

            return indexPath;
        }

        /// <summary>
        /// Creates an HTML view structure from a MultiLogReader.ActivityMap.
        /// </summary>
        /// <param name="_activityMap">ActivityMap to use.</param>
        /// <param name="activityMonitor">Activity monitor to use when logging events about generation.</param>
        /// <param name="htmlOutputDirectory">Directory in which the HTML structure will be generated.</param>
        /// <param name="logEntryCountPerPage">How many entries to write on every log page.</param>
        /// <returns>Full path of created index.html. Null when no valid file could be loaded from directoryPath.</returns>
        public static string CreateFromActivityMap( MultiLogReader.ActivityMap activityMap, IActivityMonitor activityMonitor, int logEntryCountPerPage, string outputDirectoryPath, string resourcesDirectoryPath = "" )
        {
            HtmlGenerator g = new HtmlGenerator( activityMap, outputDirectoryPath, activityMonitor, logEntryCountPerPage, resourcesDirectoryPath );

            string indexPath = g.GenerateHtmlStructure();

            if( indexPath != null && String.IsNullOrWhiteSpace( resourcesDirectoryPath ) )
            {
                g.CopyResourcesToIndexDirectory();
            }

            return indexPath;
        }
        private void CopyResourcesToIndexDirectory()
        {
            CopyResourcesToDirectory( _outputDirectoryPath );
        }

        /// <summary>
        /// Copies additional content (JS, CSS, images) into the target directory path.
        /// </summary>
        public static void CopyResourcesToDirectory(string resourceDirectoryRoot)
        {
            CopyContentResourceToFile( @"css\CkmonStyle.css", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\Reset.css", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\bootstrap.min.css", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\bootstrap-theme.min.css", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\ex-bg.png", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\warn.svg", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\error.svg", resourceDirectoryRoot );
            CopyContentResourceToFile( @"css\fatal.svg", resourceDirectoryRoot );
            CopyContentResourceToFile( @"js\bootstrap.min.js", resourceDirectoryRoot );
            CopyContentResourceToFile( @"js\jquery-2.0.3.min.js", resourceDirectoryRoot );
            CopyContentResourceToFile( @"js\moment.min.js", resourceDirectoryRoot );
            CopyContentResourceToFile( @"js\moment-with-langs.js", resourceDirectoryRoot );
            CopyContentResourceToFile( @"js\ckmon.js", resourceDirectoryRoot );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.eot", resourceDirectoryRoot );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.svg", resourceDirectoryRoot );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.ttf", resourceDirectoryRoot );
            CopyContentResourceToFile( @"fonts\glyphicons-halflings-regular.woff", resourceDirectoryRoot );
        }

        /// <summary>
        /// Initializes a new HtmlGenerator, using an existing ActivityMap.
        /// </summary>
        /// <param name="_activityMap">ActivityMap</param>
        /// <param name="htmlOutputDirectory">directory to output HTML into</param>
        /// <param name="activityMonitor">Monitor to use when reporting events about generation</param>
        private HtmlGenerator( MultiLogReader.ActivityMap activityMap, string htmlOutputDirectory, IActivityMonitor activityMonitor, int logEntryCountPerPage, string resourcesDirectoryPath )
        {
            Debug.Assert( activityMap != null );
            Debug.Assert( activityMonitor != null );
            Debug.Assert( !String.IsNullOrWhiteSpace( htmlOutputDirectory ) );

            _logEntryCountPerPage = logEntryCountPerPage;
            _monitor = activityMonitor;
            _outputDirectoryPath = htmlOutputDirectory;
            _indexInfos = new Dictionary<MultiLogReader.Monitor, MonitorIndexInfo>();
            _activityMap = activityMap;
            _resourcesDirectoryPath = resourcesDirectoryPath;

        }

        /// <summary>
        /// Creates the complete HTML structure of all Monitors contained in our ActivityMap,
        /// then generates an index.
        /// </summary>
        /// <returns>Complete index path. Null if no files could be loaded.</returns>
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
                    var monitorIndex = MonitorIndexInfo.IndexMonitor( monitor, _logEntryCountPerPage );
                    _indexInfos.Add( monitor, monitorIndex );

                    _monitor.Trace().Send( "Indexing monitor: {0}", monitor.MonitorId.ToString() );
                    _monitor.Trace().Send( "Writing monitor: {0}", monitor.MonitorId.ToString() );
                    var logPages = CreateMonitorHtmlStructure( monitor, monitorIndex );
                    if( logPages != null ) monitorPages.Add( monitor, logPages );
                }
            }

            string indexPath = CreateIndex( monitorPages );

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
            List<ParentedLogEntry> currentPageLogEntries = new List<ParentedLogEntry>();

            IReadOnlyList<ILogEntry> openGroupsOnStart = new List<ILogEntry>().ToReadOnlyList(); // To fix
            List<ILogEntry> openGroupsOnEnd = new List<ILogEntry>();

            int currentPageNumber = 1;

            int totalEntryCount = monitorIndex.TotalEntryCount;
            int totalPageCount = monitorIndex.PageCount;

            var page = monitor.ReadFirstPage( monitor.FirstEntryTime, _logEntryCountPerPage );

            do
            {
                foreach( var parentedLogEntry in page.Entries )
                {
                    var entry = parentedLogEntry.Entry;
                    currentPageLogEntries.Add( parentedLogEntry );

                    if( entry.LogType == LogEntryType.OpenGroup && !parentedLogEntry.IsMissing )
                    {
                        openGroupsOnEnd.Add( entry );
                    }
                    else if( entry.LogType == LogEntryType.CloseGroup && !parentedLogEntry.Parent.IsMissing )
                    {
                        openGroupsOnEnd.Remove( openGroupsOnEnd[openGroupsOnEnd.Count - 1] );
                    }

                    // Flush entries into HTML
                    if( currentPageLogEntries.Count >= _logEntryCountPerPage )
                    {
                        _monitor.Info().Send( "Generating page {0}", currentPageNumber );
                        string pageName = GenerateLogPage( currentPageLogEntries, monitor, currentPageNumber, openGroupsOnStart, openGroupsOnEnd.ToReadOnlyList() );
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
                string pageName = GenerateLogPage( currentPageLogEntries, monitor, currentPageNumber, openGroupsOnStart, openGroupsOnEnd.ToReadOnlyList() );
                currentPageLogEntries.Clear();

                pageFilenames.Add( pageName );
            }

            return pageFilenames;
        }

        /// <summary>
        /// Generates a single HTML log page from a list of entries corresponding to a monitor.
        /// </summary>
        /// <param name="currentPageLogEntries">Log entries that will be written on the page.</param>
        /// <param name="monitor">Monitor corresponding to the page.</param>
        /// <param name="currentPageNumber">Page number of the page that will be written.</param>
        /// <param name="openGroupsOnStart">Group path at the beginning of the page.</param>
        /// <param name="openGroupsOnEnd">Group path at the end of the page.</param>
        /// <returns></returns>
        private string GenerateLogPage( IEnumerable<ParentedLogEntry> currentPageLogEntries,
            MultiLogReader.Monitor monitor,
            int currentPageNumber,
            IReadOnlyList<ILogEntry> openGroupsOnStart,
            IReadOnlyList<ILogEntry> openGroupsOnEnd
            )
        {
            string filename = HtmlUtils.GetMonitorPageFilename( monitor, currentPageNumber );

            using( TextWriter tw = File.CreateText( Path.Combine( _outputDirectoryPath, filename ) ) )
            {
                WriteLogPageHeader( tw, monitor, currentPageNumber );

                HtmlEntryPageWriter.WriteEntries( tw, currentPageLogEntries, openGroupsOnStart, currentPageNumber, _indexInfos[monitor], monitor );

                WriteLogPageFooter( tw, monitor, currentPageNumber );
            }

            return filename;
        }

        private void WriteLogPageHeader( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage )
        {
            tw.Write( GetHtmlHeader( String.Format( "Log: {0} - Page {1}", monitor.MonitorId.ToString(), currentPage ), _resourcesDirectoryPath, true ) );
        }

        private void WriteLogPageFooter( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage )
        {
            WriteMonitorPaginator( tw, monitor, currentPage );
            tw.Write( HTML_ENTRYPAGE_CONTEXT_MENU );
            tw.Write( GetHtmlFooter( _resourcesDirectoryPath ) );
        }

        private string CreateIndex( Dictionary<MultiLogReader.Monitor, IEnumerable<string>> monitorPages )
        {
            string indexFilePath = Path.Combine( _outputDirectoryPath, @"index.html" );

            TextWriter tw = File.CreateText( indexFilePath );

            WriteIndex( monitorPages, tw );

            tw.Close();

            return indexFilePath;
        }

        private void WriteIndex( Dictionary<MultiLogReader.Monitor, IEnumerable<string>> monitorPages, TextWriter tw )
        {
            tw.Write( GetHtmlHeader( "Ckmon Index", _resourcesDirectoryPath ) );

            var dumpPaths = GetSystemActivityMonitorDumpPaths();
            if( dumpPaths.Count > 0 )
            {
                tw.Write( @"<div class=""alert alert-danger"">" );
                tw.Write( @"<h2>Found SystemActivityMonitor dumps</h2>" );
                tw.Write( @"<p>The logging system encountered the following errors:</p>" );
                foreach( var path in dumpPaths )
                {
                    tw.Write( @"<h3>{0} <small>{1}</small></h3>", Path.GetFileName( path ), Path.GetDirectoryName( path ) );
                    tw.Write( @"<pre>{0}</pre>", File.ReadAllText( path ) );
                }
                tw.Write( @"</div>" );
            }

            tw.Write( "<h1>ActivityMonitor log viewer</h1>" );
            tw.Write( String.Format( "<h3>Between {0} and {1}</h3>", _activityMap.FirstEntryDate, _activityMap.LastEntryDate ) );

            tw.Write( @"<h2>Monitors:</h2><table class=""monitorTable table table-striped table-bordered"">" );
            tw.Write( @"<thead><tr><th>Monitor</th><th>Started</th><th>Duration</th><th>Entries</th></tr></thead><tbody>" );

            var monitorList = _activityMap.Monitors.ToList();
            monitorList.Sort( ( a, b ) => b.FirstEntryTime.CompareTo( a.FirstEntryTime ) );

            foreach( MultiLogReader.Monitor monitor in monitorList )
            {
                tw.Write( @"<tr class=""monitorEntry"">" );
                IEnumerable<string> monitorPageList = null;
                string href = monitor.MonitorId.ToString();

                if( monitorPages.TryGetValue( monitor, out monitorPageList ) )
                {
                    href = String.Format( @"<a href=""{1}"">{0}</a>", _indexInfos[monitor].MonitorTitle,
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
            tw.Write( GetHtmlFooter(_resourcesDirectoryPath) );
        }

        private void WriteMonitorPaginator( TextWriter tw, MultiLogReader.Monitor monitor, int currentPage )
        {
            Debug.Assert( currentPage <= _indexInfos[monitor].PageCount );
            tw.Write( @"<div class=""center-container"">" );
            tw.Write( @"<ul class=""pagination center-content"">" );

            if( currentPage > 1 )
            {
                tw.Write( @"<li><a href=""{0}"">&laquo;</a></li>", HtmlUtils.GetMonitorPageFilename( monitor, 1 ) );
                tw.Write( @"<li><a href=""{0}"">Prev</a></li>", HtmlUtils.GetMonitorPageFilename( monitor, currentPage - 1 ) );
            }
            else
            {
                tw.Write( @"<li class=""disabled""><a>&laquo;</a></li>" );
                tw.Write( @"<li class=""disabled""><a>Prev</a></li>" );
            }

            foreach( var i in LogarithmicPagination( currentPage, _indexInfos[monitor].PageCount ) )
            {
                if( i == currentPage )
                {
                    tw.Write( @"<li class=""active""><a>{0}</a></li>", i );
                }
                else
                {
                    tw.Write( @"<li><a href=""{0}"">{1}</a></li>", HtmlUtils.GetMonitorPageFilename( monitor, i ), i );
                }

            }

            if( currentPage < _indexInfos[monitor].PageCount )
            {
                tw.Write( @"<li><a href=""{0}"">Next</a></li>", HtmlUtils.GetMonitorPageFilename( monitor, currentPage + 1 ) );
                tw.Write( @"<li><a href=""{0}"">&raquo;</a></li>", HtmlUtils.GetMonitorPageFilename( monitor, _indexInfos[monitor].PageCount ) );
            }
            else
            {
                tw.Write( @"<li class=""disabled""><a>Next</a></li>" );
                tw.Write( @"<li class=""disabled""><a>&raquo;</a></li>" );
            }

            tw.Write( @"</ul>" );
            tw.Write( @"</div>" );
        }

        private IList<string> GetSystemActivityMonitorDumpPaths()
        {
            List<string> dumpPaths = new List<string>();

            // Get all ckmon folders in the ActivityMap
            List<string> ckmonFolders = _activityMap.AllFiles.Select( x => Path.GetDirectoryName( x.FileName ) ).Distinct().ToList();

            foreach( var ckmonFolder in ckmonFolders )
            {
                // Find lowest SystemActivityMonitor folder in the path
                string currentFolder = Path.GetFullPath( ckmonFolder );
                string targetFolder = String.Empty;
                bool hasFolder = false;
                while( !hasFolder )
                {
                    targetFolder = Path.Combine( currentFolder, "SystemActivityMonitor" );
                    if( Directory.Exists( targetFolder ) )
                    {
                        hasFolder = true;
                        currentFolder = targetFolder;
                        break;
                    }
                    else
                    {
                        var currentFolderInfo = Directory.GetParent( currentFolder );
                        if( currentFolderInfo == null )
                        {
                            break; // At root!
                        }
                        else
                        {
                            currentFolder = currentFolderInfo.FullName;
                        }
                    }
                }

                if( hasFolder )
                {
                    foreach( var f in Directory.GetFiles( currentFolder, "*.txt" ) )
                    {
                        if( !dumpPaths.Contains( f ) ) dumpPaths.Add( f );
                    }
                }
                else
                {
                    // Broke at root, but didn't find a path
                    _monitor.Warn().Send( "Did not find a SystemActivityMonitor directory when browsing path {0}.", ckmonFolder );
                }

                if( dumpPaths.Count > 0 )
                {
                    using( _monitor.OpenWarn().Send( "Found the following SystemActivityMonitor dumps:" ) )
                    {
                        foreach( var p in dumpPaths )
                        {
                            _monitor.Warn().Send( "{0}", p );
                        }
                        _monitor.CloseGroup( String.Format( "{0} dumps.", dumpPaths.Count.ToString() ) );
                    }
                }
            }

            return dumpPaths;
        }

        /// <summary>
        /// Finds the common root directory from a list of paths.
        /// This one is from Rosetta Code. http://rosettacode.org/wiki/Find_common_directory_path#C.23
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string FindCommonPath( List<string> paths )
        {
            string separator = Path.DirectorySeparatorChar.ToString();
            string commonPath = String.Empty;
            List<string> separatedPath = paths
                .First( str => str.Length == paths.Max( st2 => st2.Length ) )
                .Split( new string[] { separator }, StringSplitOptions.RemoveEmptyEntries )
                .ToList();

            foreach( string pathSegment in separatedPath )
            {
                if( commonPath.Length == 0 && paths.All( str => str.StartsWith( pathSegment ) ) )
                {
                    commonPath = pathSegment;
                }
                else if( paths.All( str => str.StartsWith( commonPath + separator + pathSegment ) ) )
                {
                    commonPath += separator + pathSegment;
                }
                else
                {
                    break;
                }
            }

            return commonPath;
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

            using( Stream resource = typeof( HtmlGenerator ).Assembly.GetManifestResourceStream( resourcePath ) )
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

        private static string GetHtmlHeader( string title, string resourceDirectory, bool writeLogMenu = false )
        {
            title = HttpUtility.HtmlEncode( title );
            return String.Format( HTML_HEADER, title, writeLogMenu ? HTML_ENTRYPAGE_HEADER_MENU : String.Empty, resourceDirectory );
        }

        private static string GetHtmlFooter(string resourceDirectory)
        {
            return String.Format(HTML_FOOTER, resourceDirectory);
        }

        #region HTML header template
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
<link rel=""stylesheet"" type=""text/css"" href=""{2}css/Reset.css"">
<link rel=""stylesheet"" type=""text/css"" href=""{2}css/bootstrap.min.css"">
<link rel=""stylesheet"" type=""text/css"" href=""{2}css/bootstrap-theme.min.css"">
<link rel=""stylesheet"" type=""text/css"" href=""{2}css/CkmonStyle.css"">
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
                               {1}
                        </ul>
                    </div><!-- /.navbar-collapse -->
                </div>
            </nav>
        </header>

        <div class=""container"">
            <section>
";
        #endregion

        #region HTML footer template (scripts)
        static string HTML_FOOTER =
            @"
            </section>
        </div>
    </div>

<script src=""{0}js/jquery-2.0.3.min.js""></script>
<script src=""{0}js/bootstrap.min.js""></script>
<script src=""{0}js/moment-with-langs.js""></script>
<script src=""{0}js/ckmon.js""></script>
</body></html>";
        #endregion

        #region HTML entry page context menu
        static string HTML_ENTRYPAGE_CONTEXT_MENU =
            @"
<div id=""contextMenu"" class=""dropdown clearfix"">
    <ul class=""dropdown-menu"" role=""menu"" style=""display:block;position:static;margin-bottom:5px;"">
        <li><a id=""toggleGroupMenuEntry"" tabindex=""-1"" href=""#"">Toggle group</a></li>

        <li><a id=""expandStructureMenuEntry"" tabindex=""-1"" href=""#"">Open selected groups</a></li>
        <li><a id=""collapseStructureMenuEntry"" tabindex=""-1"" href=""#"">Close selected groups</a></li>

        <li><a id=""expandContentMenuEntry"" tabindex=""-1"" href=""#"">Expand content</a></li>
        <li><a id=""collapseContentMenuEntry"" tabindex=""-1"" href=""#"">Collapse content</a></li>

        <li><a id=""collapseParentMenuEntry"" tabindex=""-1"" href=""#"">Close parent</a></li>
    </ul>
</div>";
        #endregion

        #region HTML entry page header menu
        static string HTML_ENTRYPAGE_HEADER_MENU =
            @"
<li class=""dropdown"">
        <a href=""#"" class=""dropdown-toggle"" data-toggle=""dropdown"">Log page <b class=""caret""></b></a>
        <ul class=""dropdown-menu"">
        <li><a id=""openAllGroupsMenuEntry"" tabindex=""-1"" href=""#"">Open all groups</a></li>
        <li><a id=""closeAllGroupsMenuEntry"" tabindex=""-1"" href=""#"">Close all groups</a></li>

        <li class=""divider""></li>

        <li><a id=""openAllContentMenuEntry"" tabindex=""-1"" href=""#"">Expand all text</a></li>
        <li><a id=""closeAllContentMenuEntry"" tabindex=""-1"" href=""#"">Collapse all text</a></li>
    </ul>
</li>";
        #endregion

        private static readonly int LINKS_PER_STEP = 2;
        private static List<int> LogarithmicPagination( int page, int lastPage )
        {
            List<int> resultList = new List<int>();

            // Now calculate page links...
            int lastp1 = 1;
            int lastp2 = page;
            int p1 = 1;
            int p2 = page;
            int c1 = LINKS_PER_STEP + 1;
            int c2 = LINKS_PER_STEP + 1;
            int step = 1;

            while( true )
            {
                if( c1 >= c2 )
                {
                    resultList.Add( p1 );
                    lastp1 = p1;
                    p1 += step;
                    c1--;
                }
                else
                {
                    resultList.Add( p2 );
                    lastp2 = p2;
                    p2 -= step;
                    c2--;
                }
                if( c2 == 0 )
                {
                    step *= 10;
                    p1 += step - 1;         // Round UP to nearest multiple of $step
                    p1 -= (p1 % step);
                    p2 -= (p2 % step);   // Round DOWN to nearest multiple of $step
                    c1 = LINKS_PER_STEP;
                    c2 = LINKS_PER_STEP;
                }
                if( p1 > p2 )
                {
                    if( (lastp2 > page) || (page >= lastPage) )
                        return resultList.OrderBy( a => a ).ToList();
                    lastp1 = page;
                    lastp2 = lastPage;
                    p1 = page + 1;
                    p2 = lastPage;
                    c1 = LINKS_PER_STEP;
                    c2 = LINKS_PER_STEP + 1;
                    step = 1;
                }
            }
        }
    }
}
