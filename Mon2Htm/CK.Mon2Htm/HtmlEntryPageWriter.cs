﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
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
    public class HtmlEntryPageWriter
    {
        readonly TextWriter _tw;
        readonly IReadOnlyList<ILogEntry> _initialPath;
        readonly List<ILogEntry> _currentPath;
        readonly MonitorIndexInfo _indexInfo;
        readonly MultiLogReader.Monitor _monitor;
        readonly int _pageNumber;


        internal static void WriteEntries( TextWriter tw, IEnumerable<ParentedLogEntry> logEntries, IReadOnlyList<ILogEntry> initialOpenGroups, int pageNumber, MonitorIndexInfo indexInfo, MultiLogReader.Monitor monitor )
        {
            var writer = new HtmlEntryPageWriter( tw, initialOpenGroups, pageNumber, indexInfo, monitor );

            writer.DoWriteEntries( logEntries );
        }

        private HtmlEntryPageWriter( TextWriter tw, IReadOnlyList<ILogEntry> initialPath, int pageNumber, MonitorIndexInfo indexInfo, MultiLogReader.Monitor monitor )
        {
            _tw = tw;
            _pageNumber = pageNumber;
            _monitor = monitor;
            _indexInfo = indexInfo;
            _initialPath = initialPath;
            _currentPath = _initialPath.ToList();
        }

        private void DoWriteEntries( IEnumerable<ParentedLogEntry> logEntries )
        {
            Dictionary<CKExceptionData, string> exceptions = new Dictionary<CKExceptionData, string>();

            WriteLogGroupBreadcrumb( _initialPath );

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

        private void HandleEntry( ParentedLogEntry entry )
        {
            if( entry.Entry.LogType == LogEntryType.OpenGroup )
            {
                WriteOpenGroup( entry.Entry );
                _currentPath.Add( entry.Entry );
            }
            else if( entry.Entry.LogType == LogEntryType.CloseGroup )
            {
                if( entry.Parent.IsMissing )
                {
                    WriteCloseGroup( entry );
                }
                else
                {
                    WriteCloseGroup( entry );
                    _currentPath.RemoveAt( _currentPath.Count - 1 );
                }
            }
            else if( entry.Entry.LogType == LogEntryType.Line )
            {
                WriteLine( entry.Entry );
            }
        }

        #region Entry writing

        private void WriteLine( ILogEntry entry )
        {
            _tw.Write( @"<li>" );

            string className = HtmlUtils.GetClassNameOfLogLevel( entry.LogLevel );
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

                _tw.WriteLine( @"<span class=""anchor"" id=""{0}""></span>",
                    HtmlUtils.GetTimestampId( entry.LogTime ) );

                if( printMessage )
                {
                    string className = HtmlUtils.GetClassNameOfLogLevel( entry.LogLevel );
                    _tw.Write( @"<p class=""logLine logGroupMessage {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) );

                    _tw.Write(
                        @"<a class=""collapseTitle collapseToggle"" data-toggle=""collapse"" href=""#group-{1}"">Group start: '{0}'</a>",
                        HttpUtility.HtmlEncode( entry.Text ),
                        HtmlUtils.GetTimestampId( entry.LogTime )
                         );

                    var indexGroupEntry = _indexInfo.Groups.GetByKey( entry.LogTime );
                    if( indexGroupEntry.CloseGroupTimestamp > DateTimeStamp.MinValue )
                    {
                        _tw.Write( @" <a href=""{0}""><span class=""glyphicon glyphicon-fast-forward""></span></a> ",
                            HtmlUtils.GetReferenceHref( _monitor, _indexInfo, indexGroupEntry.CloseGroupTimestamp ) );
                    }

                    _tw.WriteLine( @"</span></p>" );
                }

                _tw.WriteLine( @"<div id=""group-{1}"" class=""collapse in logGroup {0}"">",
                    HtmlUtils.GetClassNameOfLogLevel( entry.LogLevel ),
                    HtmlUtils.GetTimestampId( entry.LogTime ) );
            }

            WriteLogListHeader();
        }

        private void WriteCloseGroup( ParentedLogEntry parentedEntry = null )
        {
            bool closeDiv = true;

            if( parentedEntry != null ) Debug.Assert( parentedEntry.Entry.LogType == LogEntryType.CloseGroup );

            WriteLogListFooter();

            if( parentedEntry != null )
            {
                var entry = parentedEntry.Entry;
                _tw.Write( @"<span class=""anchor"" id=""{0}""></span>", HtmlUtils.GetTimestampId( entry.LogTime ) );

                _tw.Write( @"<p class=""logLine logGroupMessage {0}"">",
                    HtmlUtils.GetClassNameOfLogLevel( entry.LogLevel )
                    );

                if( parentedEntry.Parent.IsMissing )
                {

                    _tw.Write( HttpUtility.HtmlEncode( "End of group: <Open entry missing>" ) );
                    closeDiv = false;
                }
                else
                {
                    _tw.Write( @"<a href=""{0}""><span class=""glyphicon glyphicon-fast-backward""></span></a> ",
                        HtmlUtils.GetReferenceHref( _monitor, _indexInfo, parentedEntry.Parent.Entry.LogTime ) );

                    if( parentedEntry.IsMissing )
                    {
                        _tw.Write(
                            HttpUtility.HtmlEncode(
                                String.Format( "<Missing end of group>: '{0}'.", parentedEntry.Parent.Entry.Text )
                            )
                        );
                    }
                    else
                    {
                        _tw.Write( "End of group: '{0}'.", parentedEntry.Parent.Entry.Text );
                    }
                }

                if( entry.Conclusions.Count > 0 )
                {
                    _tw.Write( " Conclusions: {0}", String.Join( "; ", entry.Conclusions ) );
                }

                _tw.Write( @"</p>" );
            }

            if( closeDiv ) _tw.Write( @"</div>" );

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
            _tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Next page</a>", HtmlUtils.GetMonitorPageFilename( _monitor, _pageNumber + 1 ) );
        }

        private void WritePrevPageButton()
        {
            _tw.Write( @"<a href=""{0}"" class=""btn btn-lg btn-warning largePageButton"" role=""button"">Previous page</a>", HtmlUtils.GetMonitorPageFilename( _monitor, _pageNumber - 1 ) );
        }

        private void WriteLogGroupBreadcrumb( IReadOnlyList<ILogEntry> groupsToWrite, bool reverse = false )
        {
            if( groupsToWrite == null ) return;
            _tw.Write( @"<div class=""groupBreadcrumb"">" );
            if( !reverse )
            {
                foreach( var group in groupsToWrite )
                {
                    _tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", HtmlUtils.GetClassNameOfLogLevel( group.LogLevel ) );
                    _tw.Write( @"<p>{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-backward""></span></a></p>",
                        group.Text,
                        HtmlUtils.GetReferenceHref( _monitor, _indexInfo, _indexInfo.Groups.GetByKey( group.LogTime ).OpenGroupTimestamp ) );
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
                    _tw.Write( @"<div class=""groupBreadcrumbItem {0}"">", HtmlUtils.GetClassNameOfLogLevel( group.LogLevel ) );
                }
                foreach( var group in groupsToWrite.Reverse() )
                {
                    _tw.Write( "<p>" );
                    var groupInfo = _indexInfo.Groups.GetByKey( group.LogTime );
                    if( groupInfo.CloseGroupTimestamp > DateTimeStamp.MinValue )
                    {
                        _tw.Write( @"{0} <a href=""{1}""><span class=""glyphicon glyphicon-fast-forward""></span></a>",
                            group.Text,
                            HtmlUtils.GetReferenceHref( _monitor, _indexInfo, _indexInfo.Groups.GetByKey( group.LogTime ).CloseGroupTimestamp ) );
                    }
                    else
                    {
                        _tw.Write(
                            HttpUtility.HtmlEncode(
                                String.Format( @"{0} <Missing group end>",
                                    group.Text,
                                    HtmlUtils.GetReferenceHref( _monitor, _indexInfo, _indexInfo.Groups.GetByKey( group.LogTime ).CloseGroupTimestamp )
                                    )
                                )
                            );
                    }
                    _tw.Write( @"</p></div>" );
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
