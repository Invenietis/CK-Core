#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\LogEventEntry.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Collections;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    class LogEventEntry : LogHostEventArgs, ILogEventEntry, IReadOnlyCollection<ILogEventError>
    {
        int _depth;
        EventInfo _event;
        internal object[] _parameters;
        int _errorCount;
        LogEventEntryError _firstError;
        LogEventEntryError _lastError;

        internal void InitOpen( int lsn, int depth, EventInfo e )
        {
            LSN = -lsn;
            _depth = depth;
            _event = e;
        }

        internal void InitClose( int lsn, int depth, EventInfo e )
        {
            LSN = lsn;
            _depth = depth;
            _event = e;
        }

        /// <summary>
        /// Initializes the entry as an hidden error head (the first time an error occcurs and no event entry is created
        /// for the event). This entry is not visible (it is not emitted as a log event), it is here to handle the 
        /// potential list of errors that the event will raise.
        /// </summary>
        /// <param name="lsn"></param>
        /// <param name="depth"></param>
        /// <param name="e"></param>
        /// <param name="firstOne"></param>
        internal void InitError( int lsn, int depth, EventInfo e, LogEventEntryError firstOne )
        {
            LSN = lsn;
            _depth = depth;
            _event = e;
            _errorCount = -1;
            _firstError = _lastError = firstOne;
        }

        public override LogEntryType EntryType
        {
            get { return LogEntryType.Event; }
        }

        public override int Depth
        {
            get { return _depth; }
        }

        public EventInfo Event
        {
            get { return _event; }
        }

        public object[] Parameters
        {
            get { return _parameters; }
        }

        internal void AddError( LogEventEntryError l )
        {
            if( _errorCount > 0 ) ++_errorCount;
            else --_errorCount;
            if( _lastError != null ) _lastError._nextError = l;
            else _firstError = l;
            _lastError = l;
        }

        public override MemberInfo Member
        {
            get { return _event; }
        }

        public IReadOnlyCollection<ILogEventError> Errors
        {
            get { return this; }
        }

        internal bool IsErrorHead
        {
            get { return _errorCount < 0; }
        }

        int IReadOnlyCollection<ILogEventError>.Count 
        { 
            get { return Math.Abs( _errorCount ); } 
        }
        
        bool IReadOnlyCollection<ILogEventError>.Contains( object o )
        {
            LogEventEntryError e = o as LogEventEntryError;
            return e != null && e.OtherErrors == this;
        }

        IEnumerator<ILogEventError> IEnumerable<ILogEventError>.GetEnumerator()
        {
            LogEventEntryError l = _firstError;
            while( l != null )
            {
                yield return l;
                l = l._nextError;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ILogEventError>)this).GetEnumerator();
        }

        internal void Close()
        {
            Debug.Assert( IsCreating );
            LSN = -LSN;
        }
    }
}
