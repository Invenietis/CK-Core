using System;
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
