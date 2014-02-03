using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CKMon2Htm
{
    /// <summary>
    /// Utility class to write log entry page contents into a TextWriter. This shouldn't be instanciated directly; Use <see cref="WriteEntries()"/>.
    /// </summary>
    /// <remarks>
    /// This class does not handle creation or global structure of HTML files. It writes the following:
    /// - Open groups header
    /// - Large "Previous page" button
    /// - Log entries, groups, exceptions with their controls
    /// - Large "Next page" button
    /// - Open groups footer
    /// </remarks>
    public class CKMonEntryPageWriter
    {
        readonly TextWriter _tw;
        readonly IReadOnlyList<ILogEntry> _initialPath;
        readonly List<ILogEntry> _currentPath;
        readonly MonitorIndexInfo _indexInfo;
        readonly MultiLogReader.Monitor _monitor;
        readonly int _pageNumber;

        internal static void WriteEntries( TextWriter tw, IEnumerable<ILogEntry> logEntries, IReadOnlyList<ILogEntry> initialOpenGroups, int pageNumber, MonitorIndexInfo indexInfo, MultiLogReader.Monitor monitor )
        {
            var writer = new CKMonEntryPageWriter( tw, initialOpenGroups, pageNumber, indexInfo, monitor );

            writer.DoWriteEntries( logEntries );
        }

        private CKMonEntryPageWriter( TextWriter tw, IReadOnlyList<ILogEntry> initialPath, int pageNumber, MonitorIndexInfo indexInfo, MultiLogReader.Monitor monitor )
        {
            _tw = tw;
            _pageNumber = pageNumber;
            _monitor = monitor;
            _indexInfo = indexInfo;
            _initialPath = initialPath;
            _currentPath = _initialPath.ToList();
        }

        private void DoWriteEntries( IEnumerable<ILogEntry> logEntries )
        {
            Dictionary<CKExceptionData, string> exceptions = new Dictionary<CKExceptionData, string>();

            WriteLogGroupBreadcrumb(_initialPath);

            if( _pageNumber > 1 ) WritePrevPageButton();

            WriteLogListHeader();

            // Open any outstanding depth div before writing entries
            foreach( var group in _initialPath )
            {
                WriteOpenGroup( group, false );
            }

            // Write entries
            foreach( var entry in logEntries )
            {
                HandleEntry( entry );

                // TODO: path change
            }

            // Close any outstanding depth div
            for( int i = _currentPath.Count; i > 0; i-- )
            {
                WriteCloseGroup( null ); // TODO: path
            }

            WriteLogListFooter();

            if( _pageNumber < _indexInfo.PageCount ) WriteNextPageButton();

            WriteLogGroupBreadcrumb( _currentPath.ToReadOnlyList(), true );
        }

        private void HandleEntry( ILogEntry entry )
        {
            if( entry.LogType == LogEntryType.OpenGroup )
            {
                WriteOpenGroup( entry );
                _currentPath.Add( entry );
            }
            else if( entry.LogType == LogEntryType.CloseGroup )
            {
                WriteCloseGroup( entry, _currentPath[_currentPath.Count - 1] );
                _currentPath.RemoveAt( _currentPath.Count - 1 );
            }
            else if( entry.LogType == LogEntryType.Line )
            {
                WriteLine( entry );
            }
        }

        #region Entry writing

        private void WriteLine( ILogEntry entry )
        {
            _tw.Write( @"<li>" );

            string className = HtmUtils.GetClassNameOfLogLevel( entry.LogLevel );
            if( entry.Exception == null )
            {
                _tw.Write( String.Format( @"<pre class=""logLine {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) ) );

                _tw.Write( String.Format(
                    @"{0}",
                    HttpUtility.HtmlEncode( entry.Text )
                    ) );

                _tw.WriteLine( @"</span></pre>" );
            }
            else
            {
                _tw.Write( String.Format( @"<pre class=""logLine exceptionEntry {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) ) );

                _tw.Write( String.Format(
                    @"{1} [{0}]",
                    HttpUtility.HtmlEncode( entry.Exception.ExceptionTypeName ),
                    HttpUtility.HtmlEncode( entry.Text )
                    ) );

                string exceptionId = GenerateExceptionId();

                WriteExceptionCollapseButton( exceptionId );

                _tw.WriteLine( @"</span></pre>" );
                WriteExceptionCollapse( entry.Exception, exceptionId );
            }

            _tw.WriteLine( @"</li>" );
        }

        private void WriteOpenGroup( ILogEntry entry = null, bool printMessage = true )
        {
            if( entry != null ) Debug.Assert( entry.LogType == LogEntryType.OpenGroup );

            WriteLogListFooter();
            if( entry == null )
            {
                _tw.WriteLine( @"<div class=""logGroup"">" );
            }
            else
            {
                if( printMessage )
                {
                    string className = HtmUtils.GetClassNameOfLogLevel( entry.LogLevel );
                    _tw.Write( @"<pre class=""logLine logGroupMessage {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) );

                    _tw.Write(
                        @"<a class=""collapseTitle collapseToggle"" data-toggle=""collapse"" href=""#group-{1}"">Group start: {0}</a>",
                        HttpUtility.HtmlEncode( entry.Text ),
                        HtmUtils.GetTimestampId( entry.LogTime )
                         );

                    _tw.WriteLine( @"</span></pre>" );
                }
                _tw.WriteLine( @"<span class=""anchor"" id=""{1}""></span><div id=""group-{1}"" class=""collapse in logGroup {0}"">",
                    HtmUtils.GetClassNameOfLogLevel( entry.LogLevel ),
                    HtmUtils.GetTimestampId( entry.LogTime ) );
            }

            WriteLogListHeader();
        }

        private void WriteCloseGroup( ILogEntry entry = null, ILogEntry openGroupEntry = null )
        {
            if( entry != null ) Debug.Assert( entry.LogType == LogEntryType.CloseGroup );

            WriteLogListFooter();

            if( entry != null )
            {
                _tw.Write( @"<pre class=""logLine logGroupMessage {0}"">",
                    HtmUtils.GetClassNameOfLogLevel( entry.LogLevel )
                    );

                _tw.Write( "End of group." );
                if( entry.Conclusions.Count > 0 )
                {
                    _tw.Write( " Conclusions: {0}", String.Join( "; ", entry.Conclusions ) );
                }

                _tw.Write( @"</pre>" );
                _tw.Write( @"<span class=""anchor"" id=""{0}""></span>", HtmUtils.GetTimestampId( entry.LogTime ) );
            }

            _tw.Write( @"</div>" );

            WriteLogListHeader();

        }

        private void WriteLogListHeader()
        {
            _tw.Write( @"<ul class=""logList"">" );
        }

        private void WriteLogListFooter()
        {
            _tw.Write( @"</ul>" );
        }

        private void WriteExceptionCollapseButton( string exceptionId )
        {
            _tw.Write( @"<button class=""btn btn-xs btn-danger exceptionButton"" data-toggle=""collapse"" href=""#{0}"">
              View details
            </button>", exceptionId );
        }

        private void WriteExceptionCollapse( CKExceptionData exception, string exceptionId )
        {
            Debug.Assert( exception != null );

            _tw.Write( @"
            <div class=""exceptionContainer collapse"" id=""{0}"">
                  <div class=""exceptionHeader"">
                    <h3 class=""exceptionTitle"" id=""label-{0}"">{1}</h4>
                  </div>
                  <div class=""exceptionBody"">",
             exceptionId, // 0
             exception.ExceptionTypeName // 1
             );

            _tw.Write( @"<h3>{0}</h3>", exception.Message );
            _tw.Write( @"<h4>Stack trace:</h4>" );
            _tw.Write( @"<pre class=""stackTrace"">{0}</pre>", exception.StackTrace );

            if( exception.AggregatedExceptions != null && exception.AggregatedExceptions.Count > 0 )
            {
                _tw.Write( @"<h4>Aggregated exceptions:</h4>" );
                _tw.Write( @"<ul class=""aggregateExceptions"">" );

                foreach( var ex in exception.AggregatedExceptions )
                {
                    string aggExceptionId = GenerateExceptionId();

                    _tw.Write( @"<li>" );
                    _tw.Write( @"<p class=""exceptionEntry"">[{0}] {1}", ex.ExceptionTypeName, ex.Message );
                    WriteExceptionCollapseButton( aggExceptionId );
                    _tw.Write( @"</p>" );
                    WriteExceptionCollapse( ex, aggExceptionId );
                    _tw.Write( @"</li>" );
                }

                _tw.Write( @"</ul>" );
            }
            else if( exception.InnerException != null )
            {
                string innerExceptionId = GenerateExceptionId();

                _tw.Write( @"<h4>Caused by:</h4>" );
                _tw.Write( @"<p class=""exceptionEntry"">[{0}] {1}", exception.InnerException.ExceptionTypeName, exception.InnerException.Message );
                WriteExceptionCollapseButton( innerExceptionId );
                _tw.Write( @"</p>" );
                WriteExceptionCollapse( exception.InnerException, innerExceptionId );
            }

            _tw.Write( @"</div></div>" );
        }

        private void WriteNextPageButton()
        {
            _tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Next page</a>", HtmUtils.GetMonitorPageFilename( _monitor, _pageNumber + 1 ) );
        }

        private void WritePrevPageButton()
        {
            _tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Previous page</a>", HtmUtils.GetMonitorPageFilename( _monitor, _pageNumber - 1 ) );
        }

        private void WriteLogGroupBreadcrumb( IReadOnlyList<ILogEntry> groupsToWrite, bool reverse = false )
        {
            if( groupsToWrite == null ) return;
            _tw.Write( @"<div class=""groupBreadcrumb"">" );
            if( !reverse )
            {
                foreach( var group in groupsToWrite )
                {
                    _tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", HtmUtils.GetClassNameOfLogLevel( group.LogLevel ) );
                    _tw.Write( @"<p>{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-backward""></span></a></p>",
                        group.Text,
                        HtmUtils.GetReferenceHref( _monitor, _indexInfo, _indexInfo.Groups.GetByKey( group.LogTime ).OpenGroupTimestamp ) );
                }
                for( int i = 0; i < groupsToWrite.Count; i++ )
                {
                    _tw.Write( @"</div>" );
                }
                _tw.Write( @"<div class=""groupBreadcrumbSeparator""></div>" );
            }
            else
            {
                _tw.Write( @"<div class=""groupBreadcrumbSeparator""></div>" );
                foreach( var group in groupsToWrite )
                {
                    _tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", HtmUtils.GetClassNameOfLogLevel( group.LogLevel ) );
                }
                foreach( var group in groupsToWrite.Reverse() )
                {
                    _tw.Write( @"<p>{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-forward""></span></a></p>",
                        group.Text,
                        HtmUtils.GetReferenceHref( _monitor, _indexInfo, _indexInfo.Groups.GetByKey( group.LogTime ).CloseGroupTimestamp ) );
                    _tw.Write( @"</div>" );
                }
            }
            _tw.Write( @"</div>" );
        }

        #endregion

        #region Static utilities

        private static string GenerateExceptionId()
        {
            return String.Format( "exception-{0}", Guid.NewGuid().ToString() );
        }

        private static string GetTooltipText( ILogEntry entry )
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( @"Logged at:<br>{0}", entry.LogTime.TimeUtc.ToString() );

            if( !String.IsNullOrWhiteSpace( entry.FileName ) && entry.LineNumber > 0 )
            {
                sb.AppendFormat( @"<br>Sent from:<br>{0}:{1}", entry.FileName, entry.LineNumber );
            }
            return HttpUtility.HtmlAttributeEncode( sb.ToString() );
        }

        #endregion
    }
}
