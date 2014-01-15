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
