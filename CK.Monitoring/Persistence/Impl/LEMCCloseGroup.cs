using CK.Core;
using CK.Text;
using System;
using System.Collections.Generic;
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

        public Guid MonitorId => _monitorId; 

        public int GroupDepth => _depth;

        public DateTimeStamp PreviousLogTime => _previousLogTime; 

        public LogEntryType PreviousEntryType => _previousEntryType; 

        public override void WriteLogEntry( CKBinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _monitorId, _previousEntryType, _previousLogTime, _depth, LogLevel, LogTime, Conclusions );
        }

        public ILogEntry CreateUnicastLogEntry()
        {
            return new LECloseGroup( this );
        }

    }
}
