using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CKMon2Htm
{
    public static class HtmUtils
    {
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

        /// <summary>
        /// Sanitizes a base64 string for use in HTML/CSS names and IDs.
        /// </summary>
        /// <param name="base64string">String to sanitize</param>
        /// <returns>Usable string</returns>
        public static string SanitizeBase64ForHtml( string base64string )
        {
            return base64string.Replace( '=', '.' ).Replace( '+', '-' ).Replace( '/', '_' );
        }

        /// <summary>
        /// Gets a page associated with a monitor and a page number.
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="currentPageNumber"></param>
        /// <returns></returns>
        public static string GetMonitorPageFilename( MultiLogReader.Monitor monitor, int currentPageNumber )
        {
            string filename = String.Format( "{0}_{1}.html", monitor.MonitorId.ToString(), currentPageNumber );
            return filename;
        }

        public static string GetReferenceHref( MultiLogReader.Monitor monitor, MonitorIndexInfo monitorIndex, DateTimeStamp timestamp )
        {
            return String.Format( "{0}#{1}", HtmUtils.GetMonitorPageFilename( monitor, monitorIndex.GetPageIndexOf( timestamp ) + 1 ), HttpUtility.UrlEncode( GetTimestampId( timestamp ) ) );
        }

        public static string GetTimestampId( DateTimeStamp t )
        {
            return HtmUtils.SanitizeBase64ForHtml( t.ToBase64String() );
        }

    }
}
