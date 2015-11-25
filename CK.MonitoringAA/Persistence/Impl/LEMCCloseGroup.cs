#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\Impl\LEMCCloseGroup.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CK.Monitoring.Impl
{
    class LEMCCloseGroup : LECloseGroup, IMulticastLogEntry
    {
        readonly Guid _monitorId;
        readonly int _depth;
        readonly DateTimeStamp _previousLogTime;
        readonly LogEntryType _previousEntryType;

        public LEMCCloseGroup( Guid monitorId, int depth, DateTimeStamp previousLogTime, LogEntryType previousEntryType, DateTimeStamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
            : base( t, level, c )
        {
            _monitorId = monitorId;
            _depth = depth;
            _previousEntryType = previousEntryType;
            _previousLogTime = previousLogTime;
        }

        public Guid MonitorId { get { return _monitorId; } }

        public int GroupDepth { get { return _depth; } }

        public DateTimeStamp PreviousLogTime { get { return _previousLogTime; } }

        public LogEntryType PreviousEntryType { get { return _previousEntryType; } }

        public override void WriteLogEntry( BinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _monitorId, _previousEntryType, _previousLogTime, _depth, LogLevel, LogTime, Conclusions );
        }

        public ILogEntry CreateUnicastLogEntry()
        {
            return new LECloseGroup( this );
        }

    }
}
