#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CK.Mon2Htm\HtmlUtils.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public static class HtmlUtils
    {
        /// <summary>
        /// Gets the CSS class associated with a LogLevel.
        /// </summary>
        /// <param name="l">LogLevel</param>
        /// <returns>CSS class string</returns>
        public static string GetClassNameOfLogLevel( LogLevel l )
        {
            if( (l & LogLevel.Trace) != 0 ) return "trace";
            if( (l & LogLevel.Info) != 0 ) return "info";
            if( (l & LogLevel.Warn) != 0 ) return "warn";
            if( (l & LogLevel.Error) != 0 ) return "error";
            if( (l & LogLevel.Fatal) != 0 ) return "fatal";
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
            return String.Format( "{0}#{1}", HtmlUtils.GetMonitorPageFilename( monitor, monitorIndex.GetPageIndexOf( timestamp ) + 1 ), HtmlUtils.UrlEncode( GetTimestampId( timestamp ) ) );
        }

        public static string HtmlEncode( string s )
        {
            return System.Net.WebUtility.HtmlEncode( s );
        }
        public static string UrlEncode( string s )
        {
            return System.Uri.EscapeDataString( s );
        }
        public static string HtmlAttributeEncode( string s )
        {
            return System.Security.SecurityElement.Escape( s );
        }

        public static string GetTimestampId( DateTimeStamp t )
        {
            return HtmlUtils.SanitizeBase64ForHtml( t.ToBase64String() );
        }

    }
}
