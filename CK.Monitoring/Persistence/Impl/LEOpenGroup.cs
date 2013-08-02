using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Impl
{
    class LEOpenGroup : ILogEntry
    {
        readonly DateTime _time;
        readonly string _text;
        readonly LogLevel _level;

        public LEOpenGroup( string text, DateTime t, LogLevel l ) 
        {
            _text = text;
            _time = t;
            _level = l;
        }

        public LogEntryType LogType { get { return LogEntryType.OpenGroup; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return ActivityMonitor.EmptyTag; } }

        public DateTime LogTimeUtc { get { return _time; } }

        public Exception Exception { get { return null; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }


    }
}
