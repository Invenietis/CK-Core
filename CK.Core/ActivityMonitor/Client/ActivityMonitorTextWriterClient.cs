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
* Copyright © 2007-2012, 
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
    /// Sinks the logs to a <see cref="TextWriter"/>.
    /// </summary>
    public class ActivityMonitorTextWriterClient : ActivityMonitorTextHelperClient
    {
        Func<TextWriter> _writer;
        string _prefix;
        string _prefixLevel;
        CKTrait _currentTags;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to a 
        /// function that must return the <see cref="TextWriter"/> to use when needed.
        /// </summary>
        public ActivityMonitorTextWriterClient( Func<TextWriter> writer )
        {
            _writer = writer;
            _prefixLevel = _prefix = String.Empty;
            _currentTags = ActivityMonitor.Tags.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to
        /// a <see cref="TextWriter"/>.
        /// </summary>
        public ActivityMonitorTextWriterClient( TextWriter writer )
        {
            _writer = () => writer;
            _prefixLevel = _prefix = String.Empty;
            _currentTags = ActivityMonitor.Tags.Empty;
        }

        /// <summary>
        /// Writes all the information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnEnterLevel( ActivityMonitorLogData data )
        {
            TextWriter w = _writer();
            _prefixLevel = _prefix + new String( ' ', data.MaskedLevel.ToString().Length + 4 );
            string text = data.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != data.Tags )
            {
                w.WriteLine( "{0}- {1}: {2} -[{3}]", _prefix, data.MaskedLevel.ToString(), text, data.Tags );
                _currentTags = data.Tags;
            }
            else
            {
                w.WriteLine( "{0}- {1}: {2}", _prefix, data.MaskedLevel.ToString(), text );
            }
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }
        }

        /// <summary>
        /// Writes all information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            TextWriter w = _writer();
            string text = data.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != data.Tags )
            {
                w.WriteLine( "{0}{1} -[{2}]", _prefixLevel, text, data.Tags );
                _currentTags = data.Tags;
            }
            else w.WriteLine( _prefixLevel + text );
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }
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
            TextWriter w = _writer();
            string start = String.Format( "{0}> {1}: ", _prefix, g.MaskedGroupLevel.ToString() );
            _prefix += "|  ";
            _prefixLevel = _prefix;
            string text = g.GroupText.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != g.GroupTags )
            {
                _currentTags = g.GroupTags;
                w.WriteLine( "{0}{1} -[{2}]", start, text, _currentTags );
            }
            else
            {
                w.WriteLine( start + text );
            }
            if( g.Exception != null )
            {
                DumpException( w, _prefix, !g.IsGroupTextTheExceptionMessage, g.Exception );
            }
        }

        /// <summary>
        /// Wtites group conclusion and updates internally managed line prefix.
        /// </summary>
        /// <param name="g">Group that must be closed.</param>
        /// <param name="conclusions">Conclusions for the group.</param>
        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            TextWriter w = _writer();
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );

            w.WriteLine( "{0}< {1}", _prefixLevel, conclusions.Where( c => !c.Text.Contains( Environment.NewLine ) ).ToStringGroupConclusion() );

            foreach( var c in conclusions.Where( c => c.Text.Contains( Environment.NewLine ) ) )
            {
                string text = "< " + c.Text;
                w.WriteLine( _prefixLevel + "  " + c.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel + "   " ) );
            }
        }

        /// <summary>
        /// Recursively dumps an <see cref="Exception"/> as readable text.
        /// </summary>
        /// <param name="w">The TextWriter to write to.</param>
        /// <param name="prefix">Prefix that will start all lines.</param>
        /// <param name="displayMessage">Whether the exception message must be displayed or skip.</param>
        /// <param name="ex">The exception to display.</param>
        static public void DumpException( TextWriter w, string prefix, bool displayMessage, Exception ex )
        {
            CKException ckEx = ex as CKException;
            if( ckEx != null && ckEx.ExceptionData != null )
            {
                ckEx.ExceptionData.ToTextWriter( w, prefix );
                return;
            }

            string header = String.Format( " ┌──────────────────────────■ Exception : {0} ■──────────────────────────", ex.GetType().Name );

            string p;
            w.WriteLine( prefix + header );
            string localPrefix = prefix + " | ";
            string start;
            if( displayMessage && ex.Message != null )
            {
                start = localPrefix + "Message: ";
                p = Environment.NewLine + localPrefix + "         ";
                w.WriteLine( start + ex.Message.Replace( Environment.NewLine, p ) );
            }
            if( ex.StackTrace != null )
            {
                start = localPrefix + "Stack: ";
                p = Environment.NewLine + localPrefix + "       ";
                w.WriteLine( start + ex.StackTrace.Replace( Environment.NewLine, p ) );
            }
            var fileNFEx = ex as System.IO.FileNotFoundException;
            if( fileNFEx != null )
            {
                if( !String.IsNullOrEmpty( fileNFEx.FileName ) ) w.WriteLine( localPrefix + "FileName: " + fileNFEx.FileName );
                if( fileNFEx.FusionLog != null )
                {
                    start = localPrefix + "FusionLog: ";
                    p = Environment.NewLine + localPrefix + "         ";
                    w.WriteLine( start + fileNFEx.FusionLog.Replace( Environment.NewLine, p ) );
                }
            }
            else
            {
                var loadFileEx = ex as System.IO.FileLoadException;
                if( loadFileEx != null )
                {
                    if( !String.IsNullOrEmpty( loadFileEx.FileName ) ) w.WriteLine( localPrefix + "FileName: " + loadFileEx.FileName );
                    if( loadFileEx.FusionLog != null )
                    {
                        start = localPrefix + "FusionLog: ";
                        p = Environment.NewLine + localPrefix + "         ";
                        w.WriteLine( start + loadFileEx.FusionLog.Replace( Environment.NewLine, p ) );
                    }
                    else
                    {
                        var typeLoadEx = ex as ReflectionTypeLoadException;
                        if( typeLoadEx != null )
                        {
                            w.WriteLine( localPrefix + " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" );
                            p = localPrefix + " | ";
                            foreach( var item in typeLoadEx.LoaderExceptions )
                            {
                                DumpException( w, p, true, item );
                            }
                            w.WriteLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
                        }
                        else
                        {
                            var configEx = ex as System.Configuration.ConfigurationException;
                            if( configEx != null )
                            {
                                if( !String.IsNullOrEmpty( configEx.Filename ) ) w.WriteLine( localPrefix + "FileName: " + configEx.Filename );
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
                w.WriteLine( localPrefix + " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" );
                p = localPrefix + " | ";
                foreach( var item in aggrex.InnerExceptions )
                {
                    DumpException( w, p, true, item );
                }
                w.WriteLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            else if( ex.InnerException != null )
            {
                w.WriteLine( localPrefix + " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" );
                p = localPrefix + " | ";
                DumpException( w, p, true, ex.InnerException );
                w.WriteLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            w.WriteLine( prefix + " └" + new String( '─', header.Length - 2 ) );
        }

    }

}
