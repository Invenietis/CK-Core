#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\Impl\LEMCLog.cs) is part of CiviKey. 
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
using System.IO;
using CK.Core;

namespace CK.Monitoring.Impl
{
    class LEMCLog : LELog, IMulticastLogEntry
    {
        readonly Guid _monitorId;
        readonly int _depth;
        readonly DateTimeStamp _previousLogTime;
        readonly LogEntryType _previousEntryType;

        public LEMCLog( Guid monitorId, int depth, DateTimeStamp previousLogTime, LogEntryType previousEntryType, string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
            : base( text, t, fileName, lineNumber, l, tags, ex )
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
            LogEntry.WriteLog( w, _monitorId, _previousEntryType, _previousLogTime, _depth, false, LogLevel, LogTime, Text, Tags, Exception, FileName, LineNumber );
        }
        
        public ILogEntry CreateUnicastLogEntry()
        {
            return new LELog( this );
        }
    }
}
