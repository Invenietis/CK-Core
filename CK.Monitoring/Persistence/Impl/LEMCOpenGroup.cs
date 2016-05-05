using CK.Core;
using CK.Text;
using System;
using System.IO;

namespace CK.Monitoring.Impl
{
    class LEMCOpenGroup : LEOpenGroup, IMulticastLogEntry
    {
        readonly Guid _monitorId;
        readonly int _depth;
        readonly LogEntryType _previousEntryType;
        readonly DateTimeStamp _previousLogTime;

        public LEMCOpenGroup( Guid monitorId, int depth, DateTimeStamp previousLogTime, LogEntryType previousEntryType, string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
            : base( text, t, fileName, lineNumber, l, tags, ex )
        {
            _monitorId = monitorId;
            _depth = depth;
            _previousEntryType = previousEntryType;
            _previousLogTime = previousLogTime;
        }

        public Guid MonitorId => _monitorId; 

        public int GroupDepth => _depth;

        public LogEntryType PreviousEntryType => _previousEntryType;

        public DateTimeStamp PreviousLogTime => _previousLogTime;

        public override void WriteLogEntry( CKBinaryWriter w )
        {
            LogEntry.WriteLog( w, _monitorId, _previousEntryType, _previousLogTime, _depth, true, LogLevel, LogTime, Text, Tags, Exception, FileName, LineNumber );
        }

        public ILogEntry CreateUnicastLogEntry()
        {
            return new LEOpenGroup( this );
        }

    }
}
