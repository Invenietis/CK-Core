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
            _currentTags = ActivityMonitor.EmptyTag;
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to
        /// a <see cref="TextWriter"/>.
        /// </summary>
        public ActivityMonitorTextWriterClient( TextWriter writer )
        {
            _writer = () => writer;
            _prefixLevel = _prefix = String.Empty;
            _currentTags = ActivityMonitor.EmptyTag;
        }

        protected override void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            TextWriter w = _writer();
            _prefixLevel = _prefix + new String( ' ', level.ToString().Length + 4 );
            text = text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != tags )
            {
                w.WriteLine( "{0}- {1}: {2} -[{3}]", _prefix, level.ToString(), text, tags );
                _currentTags = tags;
            }
            else
            {
                w.WriteLine( "{0}- {1}: {2}", _prefix, level.ToString(), text );
            }
        }

        protected override void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            TextWriter w = _writer();
            text = text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel );
            if( _currentTags != tags )
            {
                w.WriteLine( "{0}{1} -[{2}]", _prefixLevel, text, tags );
                _currentTags = tags;
            }
            else w.WriteLine( _prefixLevel + text );
        }

        protected override void OnLeaveLevel( LogLevel level )
        {
            _prefixLevel = _prefix;
        }

        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            TextWriter w = _writer();
            string start = String.Format( "{0}> {1}: ", _prefix, g.GroupLevel.ToString() );
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
        }

        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            TextWriter w = _writer();
            if( g.Exception != null )
            {
                DumpException( w, !g.IsGroupTextTheExceptionMessage, g.Exception );
            }
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );

            w.WriteLine( "{0}< {1}", _prefixLevel, conclusions.Where( c => !c.Text.Contains( Environment.NewLine ) ).ToStringGroupConclusion() );

            foreach( var c in conclusions.Where( c => c.Text.Contains( Environment.NewLine ) ) )
            {
                string text = "< " + c.Text;
                w.WriteLine( _prefixLevel + "  " + c.Text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel + "   " ) );
            }
        }

        void DumpException( TextWriter w, bool displayMessage, Exception ex )
        {
            CKException ckEx = ex as CKException;
            if( ckEx != null && ckEx.ExceptionData != null )
            {
                ckEx.ExceptionData.ToTextWriter( w, _prefix );
                return;
            }

            string header = String.Format( " ┌──────────────────────────■ Exception : {0} ■──────────────────────────", ex.GetType().Name );

            string p;
            w.WriteLine( _prefix + header );
            _prefix += " | ";
            string start;
            if( displayMessage && ex.Message != null )
            {
                start = _prefix + "Message: ";
                p = _prefix + "         ";
                w.WriteLine( start + ex.Message.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            if( ex.StackTrace != null )
            {
                start = _prefix + "Stack: ";
                p = _prefix + "       ";
                w.WriteLine( start + ex.StackTrace.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            var fileNFEx = ex as System.IO.FileNotFoundException;
            if( fileNFEx != null )
            {
                if( !String.IsNullOrEmpty( fileNFEx.FileName ) ) w.WriteLine( _prefix + "FileName: " + fileNFEx.FileName );
                if( fileNFEx.FusionLog != null )
                {
                    start = _prefix + "FusionLog: ";
                    p = _prefix + "         ";
                    w.WriteLine( start + fileNFEx.FusionLog.Replace( Environment.NewLine, Environment.NewLine + p ) );
                }
            }
            else
            {
                var loadFileEx = ex as System.IO.FileLoadException;
                if( loadFileEx != null )
                {
                    if( !String.IsNullOrEmpty( loadFileEx.FileName ) ) w.WriteLine( _prefix + "FileName: " + loadFileEx.FileName );
                    if( loadFileEx.FusionLog != null )
                    {
                        start = _prefix + "FusionLog: ";
                        p = _prefix + "         ";
                        w.WriteLine( start + loadFileEx.FusionLog.Replace( Environment.NewLine, Environment.NewLine + p ) );
                    }
                    else
                    {
                        var typeLoadEx = ex as ReflectionTypeLoadException;
                        if( typeLoadEx != null )
                        {
                            w.WriteLine( _prefix + " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" );
                            _prefix += " | ";
                            foreach( var item in typeLoadEx.LoaderExceptions )
                            {
                                DumpException( w, true, item );
                            }
                            _prefix = _prefix.Remove( _prefix.Length - 3 );
                            w.WriteLine( _prefix + " └─────────────────────────────────────────────────────────────────────────" );
                        }
                        else
                        {
                            var configEx = ex as System.Configuration.ConfigurationException;
                            if( configEx != null )
                            {
                                if( !String.IsNullOrEmpty( configEx.Filename ) ) w.WriteLine( _prefix + "FileName: " + configEx.Filename );
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
                w.WriteLine( _prefix + " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" );
                _prefix += " | ";
                foreach( var item in aggrex.InnerExceptions )
                {
                    DumpException( w, true, item );
                }
                _prefix = _prefix.Remove( _prefix.Length - 3 );
                w.WriteLine( _prefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            else if( ex.InnerException != null )
            {
                w.WriteLine( _prefix + " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" );
                _prefix += " | ";
                DumpException( w, true, ex.InnerException );
                _prefix = _prefix.Remove( _prefix.Length - 3 );
                w.WriteLine( _prefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            _prefix = _prefix.Remove( _prefix.Length - 3 );
            w.WriteLine( _prefix + " └" + new String( '─', header.Length - 2 ) );
        }

    }

}
