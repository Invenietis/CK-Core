#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerTextWriterSink.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Sinks the logs to a <see cref="TextWriter"/>.
    /// </summary>
    public class ActivityLoggerTextWriterSink : IActivityLoggerSink
    {
        Func<TextWriter> _writer;
        string _prefix;
        string _prefixLevel;

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerTextWriterSink"/> bound to a 
        /// function that must return the <see cref="TextWriter"/> to use when needed.
        /// </summary>
        public ActivityLoggerTextWriterSink( Func<TextWriter> writer )
        {
            _writer = writer;
            _prefixLevel = _prefix = String.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerTextWriterSink"/> bound to
        /// a <see cref="TextWriter"/>.
        /// </summary>
        public ActivityLoggerTextWriterSink( TextWriter writer )
        {
            _writer = () => writer;
            _prefixLevel = _prefix = String.Empty;
        }

        void IActivityLoggerSink.OnEnterLevel( LogLevel level, string text )
        {
            TextWriter w = _writer();
            _prefixLevel = _prefix + new String( '\u00A0', level.ToString().Length + 4 );
            w.WriteLine( _prefix + "-\u00A0" + level.ToString() + ":\u00A0" + text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnContinueOnSameLevel( LogLevel level, string text )
        {
            TextWriter w = _writer();
            w.WriteLine( _prefixLevel + text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnLeaveLevel( LogLevel level )
        {
            _prefixLevel = _prefix;
        }

        void IActivityLoggerSink.OnGroupOpen( IActivityLogGroup g )
        {
            TextWriter w = _writer();
            string start = String.Format( "{0}▪►-{1}:\u00A0", _prefix, g.GroupLevel.ToString() );
            _prefix += "▪\u00A0\u00A0";
            _prefixLevel = _prefix;
            w.WriteLine( start + g.GroupText.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnGroupClose( IActivityLogGroup g, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            TextWriter w = _writer();
            if( g.Exception != null )
            {
                DumpException( w, !g.IsGroupTextTheExceptionMessage, g.Exception );
            }
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );
            foreach( var c in conclusions )
            {
                string text = "◄▪-" + c.Text;
                w.WriteLine( _prefixLevel + text.Replace( _prefixLevel + Environment.NewLine, Environment.NewLine + _prefixLevel + "   " ) );
            }
        }

        void DumpException( TextWriter w, bool displayMessage, Exception ex )
        {
            string p;

            w.WriteLine( _prefix + "\u00A0┌──────────────────────────■ Exception ■──────────────────────────" );
            _prefix += "\u00A0|\u00A0";
            string start;
            if( displayMessage && ex.Message != null )
            {
                start = _prefix + "Message:\u00A0";
                p = _prefix + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0";
                w.WriteLine( start + ex.Message.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            if( ex.StackTrace != null )
            {
                start = _prefix + "Stack:\u00A0";
                p = _prefix + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0";
                w.WriteLine( start + ex.StackTrace.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            if( ex.InnerException != null )
            {
                w.WriteLine( _prefix + "\u00A0┌──────────────────────────▪ [Inner Exception] ▪──────────────────────────" );
                _prefix += "\u00A0|\u00A0";
                DumpException( w, true, ex.InnerException );
                _prefix = _prefix.Remove( _prefix.Length - 3 );
                w.WriteLine( _prefix + "\u00A0└─────────────────────────────────────────────────────────────────────────" );
            }
            _prefix = _prefix.Remove( _prefix.Length - 3 );
            w.WriteLine( _prefix + "\u00A0└─────────────────────────────────────────────────────────────────────────" );
        }

    }

}
