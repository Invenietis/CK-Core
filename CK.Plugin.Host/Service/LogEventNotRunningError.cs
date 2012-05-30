#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\LogEventNotRunningError.cs) is part of CiviKey. 
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
    class LogEventNotRunningError : LogHostEventArgs, ILogEventNotRunningError
    {
        int _depth;
        EventInfo _event;
        bool _serviceIsDisabled;

        internal LogEventNotRunningError( int lsn, int depth, EventInfo e, bool serviceIsDisabled )
        {
            Debug.Assert( e != null );
            LSN = lsn;
            _event = e;
            _depth = depth;
            _serviceIsDisabled = serviceIsDisabled;
        }

        public override LogEntryType EntryType
        {
            get { return LogEntryType.EventNotRunningError; }
        }

        public bool ServiceIsDisabled
        {
            get { return _serviceIsDisabled; }
        }

        public override int Depth
        {
            get { return _depth; }
        }

        public MemberInfo Culprit
        {
            get { return _event; }
        }

        public EventInfo Event
        {
            get { return _event; }
        }

        public override MemberInfo Member
        {
            get { return _event; }
        }

    }
}
