#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\LogEventEntryError.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin.Hosting
{
    class LogEventEntryError : LogHostEventArgs, ILogEventError
    {
        LogEventEntry _entry;
        MethodInfo _target;
        Exception _exception;
        internal LogEventEntryError _nextError;

        internal LogEventEntryError( int lsn, LogEventEntry e, MethodInfo target, Exception ex )
        {
            Debug.Assert( e != null && target != null && ex != null );
            LSN = lsn;
            _entry = e;
            _target = target;
            _exception = ex;
        }

        public override LogEntryType EntryType
        {
            get { return LogEntryType.EventError; }
        }

        public override int Depth
        {
            get { return _entry.Depth; }
        }

        public EventInfo Event
        {
            get { return _entry.Event; }
        }

        public MethodInfo Target
        {
            get { return _target; }
        }

        public MemberInfo Culprit
        {
            get { return _target; }
        }

        public ILogEventEntry EventEntry
        {
            get { return _entry.IsErrorHead ? null : _entry; }
        }

        public IReadOnlyCollection<ILogEventError> OtherErrors
        {
            get { return _entry; }
        }

        public Exception Error
        {
            get { return _exception; }
        }

        public override MemberInfo Member
        {
            get { return _entry.Event; }
        }

    }
}
