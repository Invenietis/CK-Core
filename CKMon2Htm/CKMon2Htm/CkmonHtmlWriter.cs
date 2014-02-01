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
    /// Class used to write raw HTML into TextWriters.
    /// </summary>
    /// <remarks>
    /// This class does not handle creation or global structure of HTML files.
    /// See the documentation of the various methods to determine what they write.
    /// </remarks>
    public class CkmonHtmlWriter
    {
        internal static void WriteEntries( TextWriter tw, IEnumerable<ILogEntry> currentPageLogEntries, IReadOnlyCollection<ILogEntry> initialOpenGroups )
        {
            int currentGroupDepth = initialOpenGroups.Count;
            Dictionary<CKExceptionData, string> exceptions = new Dictionary<CKExceptionData, string>();
            WriteLogListHeader( tw );

            // Open any outstanding depth div
            foreach( var group in initialOpenGroups )
            {
                OpenGroup( tw, group, false );
            }

            foreach( var entry in currentPageLogEntries )
            {
                if( entry.Exception != null ) FillExceptions( exceptions, entry.Exception );

                HandleEntry( tw, entry, exceptions );
                if( entry.LogType == LogEntryType.OpenGroup ) currentGroupDepth++;
                else if( entry.LogType == LogEntryType.CloseGroup ) currentGroupDepth--;
            }

            // Close any outstanding depth div
            for( int i = currentGroupDepth; i > 0; i-- )
            {
                CloseGroup( tw, null );
            }

            WriteLogListFooter( tw );
        }

        private static void FillExceptions( Dictionary<CKExceptionData, string> exceptions, CKExceptionData exception )
        {
            if( exceptions.ContainsKey( exception ) ) return;

            string exceptionModalId = String.Format( "exception-{0}", Guid.NewGuid().ToString() );

            exceptions.Add( exception, exceptionModalId );

            if( exception.AggregatedExceptions != null )
            {
                foreach( var ex in exception.AggregatedExceptions )
                {
                    FillExceptions( exceptions, ex );
                }
            }
            else if( exception.InnerException != null )
            {
                FillExceptions( exceptions, exception.InnerException );
            }
        }

        private static void HandleEntry( TextWriter tw, ILogEntry entry, Dictionary<CKExceptionData, string> exceptions )
        {
            if( entry.LogType == LogEntryType.OpenGroup )
            {
                OpenGroup( tw, entry );
            }
            else if( entry.LogType == LogEntryType.CloseGroup )
            {
                CloseGroup( tw, entry );
            }
            else if( entry.LogType == LogEntryType.Line )
            {
                WriteLine( tw, entry, exceptions );
            }
        }

        private static void WriteLine( TextWriter tw, ILogEntry entry, Dictionary<CKExceptionData, string> exceptions )
        {
            tw.Write( @"<li>" );

            string className = GetClassNameOfLogLevel( entry.LogLevel );
            if( entry.Exception == null )
            {
                tw.Write( String.Format( @"<pre class=""logLine {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) ) );

                tw.Write( String.Format(
                    @"{0}",
                    HttpUtility.HtmlEncode( entry.Text )
                    ) );

                tw.Write( @"</span></pre>" );
            }
            else
            {
                tw.Write( String.Format( @"<pre class=""logLine exceptionEntry {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) ) );

                tw.Write( String.Format(
                    @"{1} [{0}]",
                    HttpUtility.HtmlEncode( entry.Exception.ExceptionTypeName ),
                    HttpUtility.HtmlEncode( entry.Text )
                    ) );
                WriteExceptionCollapseButton( tw, exceptions[entry.Exception] );

                tw.Write( @"</span></pre>" );
                WriteExceptionCollapse( tw, entry.Exception, exceptions );
            }

            tw.Write( @"</li>" );
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

        private static void WriteLogListHeader( TextWriter tw )
        {
            tw.Write( @"<ul class=""logList"">" );
        }

        private static void WriteLogListFooter( TextWriter tw )
        {
            tw.Write( @"</ul>" );
        }

        private static void OpenGroup( TextWriter tw, ILogEntry entry = null, bool printMessage = true )
        {
            if( entry != null ) Debug.Assert( entry.LogType == LogEntryType.OpenGroup );

            WriteLogListFooter( tw );
            if( entry == null )
            {
                tw.Write( @"<div class=""logGroup"">" );
            }
            else
            {
                if( printMessage )
                {
                    string className = GetClassNameOfLogLevel( entry.LogLevel );
                    tw.Write( @"<pre class=""logLine logGroupMessage {0}""><span data-toggle=""tooltip"" title=""{1}"" rel=""tooltip"">", className, GetTooltipText( entry ) );

                    tw.Write(
                        @"<a class=""collapseTitle collapseToggle"" data-toggle=""collapse"" href=""#group-{1}"">Group start: {0}</a>",
                        HttpUtility.HtmlEncode( entry.Text ),
                        CKMon2Htm.GetTimestampId( entry.LogTime )
                         );

                    tw.Write( @"</span></pre>" );
                }
                tw.Write( @"<span class=""anchor"" id=""{1}""></span><div id=""group-{1}"" class=""collapse in logGroup {0}"">", GetClassNameOfLogLevel( entry.LogLevel ), CKMon2Htm.GetTimestampId( entry.LogTime ) );
            }

            WriteLogListHeader( tw );
        }

        private static void CloseGroup( TextWriter tw, ILogEntry entry = null )
        {
            if( entry != null ) Debug.Assert( entry.LogType == LogEntryType.CloseGroup );

            WriteLogListFooter( tw );

            if( entry != null )
            {
                tw.Write( @"<pre class=""logLine logGroupMessage {0}"">",
                    GetClassNameOfLogLevel( entry.LogLevel )
                    );

                tw.Write( "End of group." );
                if( entry.Conclusions.Count > 0)
                {
                    tw.Write( " Conclusions: {0}", String.Join( "; ", entry.Conclusions ) );
                }

                tw.Write( @"</pre>" );
                tw.Write( @"<span class=""anchor"" id=""{0}""></span>", CKMon2Htm.GetTimestampId( entry.LogTime ) );
            }

            tw.Write( @"</div>" );

            WriteLogListHeader( tw );

        }

        private static void WriteExceptionCollapseButton( TextWriter tw, string exceptionId )
        {
            tw.Write( @"<button class=""btn btn-xs btn-danger exceptionButton"" data-toggle=""collapse"" href=""#{0}"">
              View details
            </button>", exceptionId );
        }

        private static void WriteExceptionCollapse( TextWriter tw, CKExceptionData exception, Dictionary<CKExceptionData, string> exceptions )
        {
            Debug.Assert( exception != null );

            tw.Write( @"
            <div class=""exceptionContainer collapse"" id=""{0}"">
                  <div class=""exceptionHeader"">
                    <h3 class=""exceptionTitle"" id=""label-{0}"">{1}</h4>
                  </div>
                  <div class=""exceptionBody"">",
             exceptions[exception], // 0
             exception.ExceptionTypeName // 1
             );

            tw.Write( @"<h3>{0}</h3>", exception.Message );
            tw.Write( @"<h4>Stack trace:</h4>" );
            tw.Write( @"<pre class=""stackTrace"">{0}</pre>", exception.StackTrace );

            if( exception.AggregatedExceptions != null && exception.AggregatedExceptions.Count > 0 )
            {
                tw.Write( @"<h4>Aggregated exceptions:</h4>" );
                tw.Write( @"<ul class=""aggregateExceptions"">" );

                foreach( var ex in exception.AggregatedExceptions )
                {
                    tw.Write( @"<li>" );
                    tw.Write( @"<p class=""exceptionEntry"">[{0}] {1}", ex.ExceptionTypeName, ex.Message );
                    WriteExceptionCollapseButton( tw, exceptions[ex] );
                    tw.Write( @"</p>" );
                    WriteExceptionCollapse( tw, ex, exceptions );
                    tw.Write( @"</li>" );
                }

                tw.Write( @"</ul>" );
            }
            else if( exception.InnerException != null )
            {
                tw.Write( @"<h4>Caused by:</h4>" );
                tw.Write( @"<p class=""exceptionEntry"">[{0}] {1}", exception.InnerException.ExceptionTypeName, exception.InnerException.Message );
                WriteExceptionCollapseButton( tw, exceptions[exception.InnerException] );
                tw.Write( @"</p>" );
                WriteExceptionCollapse( tw, exception.InnerException, exceptions );
            }

            tw.Write( @"</div></div>" );
        }

        /// <summary>
        /// Gets the CSS class associated with a LogLevel.
        /// </summary>
        /// <param name="l">LogLevel</param>
        /// <returns>CSS class string</returns>
        public static string GetClassNameOfLogLevel( LogLevel l )
        {
            if( l.HasFlag( LogLevel.Trace ) ) return "trace";
            if( l.HasFlag( LogLevel.Info ) ) return "info";
            if( l.HasFlag( LogLevel.Warn ) ) return "warn";
            if( l.HasFlag( LogLevel.Error ) ) return "error";
            if( l.HasFlag( LogLevel.Fatal ) ) return "fatal";

            return String.Empty;
        }
    }
}
