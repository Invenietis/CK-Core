using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CK.Monitoring.Impl
{
    class LEMCOpenGroup : LEOpenGroup, IMulticastLogEntry
    {
        readonly Guid _monitorId;
        readonly int _depth;

        public LEMCOpenGroup( Guid monitorId, int depth, string text, LogTimestamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
            : base( text, t, fileName, lineNumber, l, tags, ex )
        {
            _monitorId = monitorId;
            _depth = depth;
        }

        public Guid MonitorId { get { return _monitorId; } }

        public int GroupDepth { get { return _depth; } }

        public void WriteMultiCastLogEntry( BinaryWriter w )
        {
            LogEntry.WriteLog( w, _monitorId, _depth, true, LogLevel, LogTime, Text, Tags, Exception, FileName, LineNumber );
        }

        public ILogEntry CreateUnicastLogEntry()
        {
            return new LEOpenGroup( this );
        }

    }
}
