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

        public LEMCLog( Guid monitorId, int depth, string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
            : base( text, t, fileName, lineNumber, l, tags, ex )
        {
            _monitorId = monitorId;
            _depth = depth;
        }

        public Guid MonitorId { get { return _monitorId; } }

        public int GroupDepth { get { return _depth; } }

        public void WriteMultiCastLogEntry( BinaryWriter w )
        {
            LogEntry.WriteLog( w, _monitorId, _depth, false, LogLevel, LogTime, Text, Tags, Exception, FileName, LineNumber );
        }
        
        public ILogEntry CreateUnicastLogEntry()
        {
            return new LELog( this );
        }

    }
}
