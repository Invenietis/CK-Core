#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Client\ActivityMonitorTextWriterClient.cs) is part of CiviKey. 
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
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Formats the activity and pushes piece of texts to an <see cref="Action{T}"/> where T is a string.
    /// </summary>
    public class ActivityMonitorTextWriterClient : ActivityMonitorTextHelperClient
    {
        readonly Action<string> _writer;
        readonly StringBuilder _buffer;
        string _prefix;
        string _prefixLevel;
        CKTrait _currentTags;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to a 
        /// function that must write a string, with a filter.
        /// </summary>
        /// <param name="writer">Function that writes the content.</param>
        /// <param name="filter">Filter to apply</param>
        public ActivityMonitorTextWriterClient( Action<string> writer, LogFilter filter )
            : base( filter )
        {
            if( writer == null ) throw new ArgumentNullException( "writer" );
            _writer = writer;
            _buffer = new StringBuilder();
            _prefixLevel = _prefix = String.Empty;
            _currentTags = ActivityMonitor.Tags.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to a 
        /// function that must write a string.
        /// </summary>
        /// <param name="writer">Function that writes the content.</param>
        public ActivityMonitorTextWriterClient( Action<string> writer )
            : this( writer, LogFilter.Undefined )
        {
        }

        /// <summary>
        /// Writes all the information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnEnterLevel( ActivityMonitorLogData data )
        {
            var w = _buffer.Clear();
            _prefixLevel = _prefix + new String( ' ', data.MaskedLevel.ToString().Length + 4 );
            string text = data.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != data.Tags )
            {
                w.AppendLine( string.Format( "{0}- {1}: {2} -[{3}]", _prefix, data.MaskedLevel.ToString(), text, data.Tags ) );
                _currentTags = data.Tags;
            }
            else
            {
                w.AppendLine( string.Format( "{0}- {1}: {2}", _prefix, data.MaskedLevel.ToString(), text ) );
            }
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }
            _writer( w.ToString() );
        }

        /// <summary>
        /// Writes all information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            var w = _buffer.Clear();
            string text = data.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != data.Tags )
            {
                w.AppendLine( string.Format( "{0}{1} -[{2}]", _prefixLevel, text, data.Tags ) );
                _currentTags = data.Tags;
            }
            else w.AppendLine( _prefixLevel + text );
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }

            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Updates the internally maintained prefix for lines.
        /// </summary>
        /// <param name="level">Previous level.</param>
        protected override void OnLeaveLevel( LogLevel level )
        {
            Debug.Assert( (level & LogLevel.IsFiltered) == 0 );
            _prefixLevel = _prefix;
        }

        /// <summary>
        /// Writes a group opening.
        /// </summary>
        /// <param name="g">Group information.</param>
        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            var w = _buffer.Clear();
            string start = String.Format( "{0}> {1}: ", _prefix, g.MaskedGroupLevel.ToString() );
            _prefix += "|  ";
            _prefixLevel = _prefix;
            string text = g.GroupText.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != g.GroupTags )
            {
                _currentTags = g.GroupTags;
                w.AppendLine( string.Format( "{0}{1} -[{2}]", start, text, _currentTags ) );
            }
            else
            {
                w.AppendLine( start + text );
            }
            if( g.Exception != null )
            {
                DumpException( w, _prefix, !g.IsGroupTextTheExceptionMessage, g.Exception );
            }

            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Writes group conclusion and updates internally managed line prefix.
        /// </summary>
        /// <param name="g">Group that must be closed.</param>
        /// <param name="conclusions">Conclusions for the group.</param>
        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            var w = _buffer.Clear();
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );

            w.AppendLine( string.Format( "{0}< {1}", _prefixLevel, conclusions.Where( c => !c.Text.Contains( Environment.NewLine ) ).ToStringGroupConclusion() ) );

            foreach( var c in conclusions.Where( c => c.Text.Contains( Environment.NewLine ) ) )
            {
                string text = "< " + c.Text;
                w.AppendLine( _prefixLevel + "  " + c.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel + "   " ) );
            }

            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Recursively dumps an <see cref="Exception"/> as readable text.
        /// </summary>
        /// <param name="w">The TextWriter to write to.</param>
        /// <param name="prefix">Prefix that will start all lines.</param>
        /// <param name="displayMessage">Whether the exception message must be displayed or skip.</param>
        /// <param name="ex">The exception to display.</param>
        static public void DumpException( StringBuilder w, string prefix, bool displayMessage, Exception ex )
        {
            CKException ckEx = ex as CKException;
            if( ckEx != null && ckEx.ExceptionData != null )
            {
                ckEx.ExceptionData.ToStringBuilder( w, prefix );
                return;
            }

            string header = String.Format( " ┌──────────────────────────■ Exception : {0} ■──────────────────────────", ex.GetType().Name );

            string p;
            w.AppendLine( prefix + header );
            string localPrefix = prefix + " | ";
            string start;
            if( displayMessage && ex.Message != null )
            {
                start = localPrefix + "Message: ";
                p = Environment.NewLine + localPrefix + "         ";
                w.AppendLine( start + ex.Message.Replace( Environment.NewLine, p ) );
            }
            if( ex.StackTrace != null )
            {
                start = localPrefix + "Stack: ";
                p = Environment.NewLine + localPrefix + "       ";
                w.AppendLine( start + ex.StackTrace.Replace( Environment.NewLine, p ) );
            }
            var fileNFEx = ex as System.IO.FileNotFoundException;
            if( fileNFEx != null )
            {
                if( !String.IsNullOrEmpty( fileNFEx.FileName ) ) w.AppendLine( localPrefix + "FileName: " + fileNFEx.FileName );
                if( fileNFEx.FusionLog != null )
                {
                    start = localPrefix + "FusionLog: ";
                    p = Environment.NewLine + localPrefix + "         ";
                    w.AppendLine( start + fileNFEx.FusionLog.Replace( Environment.NewLine, p ) );
                }
            }
            else
            {
                var loadFileEx = ex as System.IO.FileLoadException;
                if( loadFileEx != null )
                {
                    if( !String.IsNullOrEmpty( loadFileEx.FileName ) ) w.AppendLine( localPrefix + "FileName: " + loadFileEx.FileName );
                    if( loadFileEx.FusionLog != null )
                    {
                        start = localPrefix + "FusionLog: ";
                        p = Environment.NewLine + localPrefix + "         ";
                        w.AppendLine( start + loadFileEx.FusionLog.Replace( Environment.NewLine, p ) );
                    }
                    else
                    {
                        var typeLoadEx = ex as ReflectionTypeLoadException;
                        if( typeLoadEx != null )
                        {
                            w.AppendLine( localPrefix + " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" );
                            p = localPrefix + " | ";
                            foreach( var item in typeLoadEx.LoaderExceptions )
                            {
                                DumpException( w, p, true, item );
                            }
                            w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
                        }
                        else
                        {
                            var configEx = ex as System.Configuration.ConfigurationException;
                            if( configEx != null )
                            {
                                if( !String.IsNullOrEmpty( configEx.Filename ) ) w.AppendLine( localPrefix + "FileName: " + configEx.Filename );
                            }
                        }
                    }
                }
            }
            // The InnerException of an aggregated exception is the same as the first of it InnerExceptionS.
            // (The InnerExceptionS are the contained/aggregated exceptions of the AggregatedException object.)
            // This is why, if we are on an AggregatedException we do not follow its InnerException.
            var aggrex = ex as AggregateException;
            if( aggrex != null && aggrex.InnerExceptions.Count > 0 )
            {
                w.AppendLine( localPrefix + " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" );
                p = localPrefix + " | ";
                foreach( var item in aggrex.InnerExceptions )
                {
                    DumpException( w, p, true, item );
                }
                w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            else if( ex.InnerException != null )
            {
                w.AppendLine( localPrefix + " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" );
                p = localPrefix + " | ";
                DumpException( w, p, true, ex.InnerException );
                w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            w.AppendLine( prefix + " └" + new String( '─', header.Length - 2 ) );
        }

    }

}
