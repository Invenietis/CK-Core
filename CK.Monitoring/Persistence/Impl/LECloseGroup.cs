using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Impl
{
    class LECloseGroup: ILogEntry
    {
        readonly DateTime _time;
        readonly LogLevel _level;
        readonly IReadOnlyList<ActivityLogGroupConclusion> _conclusions;

        public LECloseGroup( DateTime t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c ) 
        {
            _time = t;
            _conclusions = c;
            _level = level;
        }

        public LogEntryType LogType { get { return LogEntryType.CloseGroup; } }

        public string Text { get { return null; } }

        public LogLevel LogLevel { get { return _level; } }

        public DateTime LogTimeUtc { get { return _time; } }

        public Exception Exception { get { return null; } }

        public CKTrait Tags { get { return ActivityMonitor.EmptyTag; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return _conclusions; } }
    }
}
