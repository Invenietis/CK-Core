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

        public LEMCCloseGroup( Guid monitorId, int depth, LogTimestamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
            : base( t, level, c )
        {
            _monitorId = monitorId;
            _depth = depth;
        }

        public Guid MonitorId { get { return _monitorId; } }

        public int GroupDepth { get { return _depth; } }

        public void WriteMultiCastLogEntry( BinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _monitorId, _depth, LogLevel, LogTime, Conclusions );
        }

        public ILogEntry CreateUnicastLogEntry()
        {
            return new LECloseGroup( this );
        }

    }
}
