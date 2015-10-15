#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\ActivityMonitorGroupData.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Data required by <see cref="IActivityMonitor.UnfilteredOpenGroup"/>.
    /// </summary>
    public class ActivityMonitorGroupData : ActivityMonitorLogData
    {
        Func<string> _getConclusion;

        internal Func<string> GetConclusionText
        {
            get { return _getConclusion; }
            set { _getConclusion = value; }
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorGroupData"/>.
        /// </summary>
        /// <param name="level">Log level. Can not be <see cref="LogLevel.None"/>.</param>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.Tags"/>) to associate to the log. It will be union-ed with the current <see cref="IActivityMonitor.AutoTags"/>.</param>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="Exception.Message"/> is the text.</param>
        /// <param name="logTime">
        /// Time of the log.
        /// You may use <see cref="DateTimeStamp.UtcNow"/> or <see cref="ActivityMonitorExtension.NextLogTime">IActivityMonitor.NextLogTime()</see> extension method.
        /// </param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="getConclusionText">Optional function that provides delayed obtention of the group conclusion: will be called on group closing.</param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source file that emitted the log. Can be null.</param>
        public ActivityMonitorGroupData( LogLevel level, CKTrait tags, string text, DateTimeStamp logTime, Exception exception, Func<string> getConclusionText, string fileName, int lineNumber )
            : base( level, exception, tags, text, logTime, fileName, lineNumber )
        {
            _getConclusion = getConclusionText;
        }

        internal ActivityMonitorGroupData( LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
        }

        /// <summary>
        /// Used only to initialize a ActivityMonitorGroupSender for rejected opened group.
        /// </summary>
        internal ActivityMonitorGroupData()
        {
        }

        internal void Initialize( string text, Exception exception, CKTrait tags, DateTimeStamp logTime, Func<string> getConclusionText )
        {
            base.Initialize( text, exception, tags, logTime );
            _getConclusion = getConclusionText;
        }

        /// <summary>
        /// Calls <see cref="GetConclusionText"/> and sets it to null.
        /// </summary>
        internal string ConsumeConclusionText()
        {
            string autoText = null;
            if( _getConclusion != null )
            {
                try
                {
                    autoText = _getConclusion();
                }
                catch( Exception ex )
                {
                    autoText = String.Format( R.ActivityMonitorErrorWhileGetConclusionText, ex.Message );
                }
                _getConclusion = null;
            }
            return autoText;
        }

    }
}
