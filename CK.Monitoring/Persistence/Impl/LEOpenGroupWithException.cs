using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Impl
{
    class LEOpenGroupWithException : ILogEntry
    {
        readonly DateTime _time;
        readonly string _text;
        readonly CKTrait _tags;
        readonly LogLevel _level;
        readonly Exception _ex;

        public LEOpenGroupWithException( string text, DateTime t, LogLevel l, CKTrait tags, Exception ex )
        {
            _text = text;
            _time = t;
            _level = l;
            _tags = tags;
            _ex = ex;
        }

        public LogEntryType LogType { get { return LogEntryType.OpenGroup; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return _tags; } }

        public DateTime LogTimeUtc { get { return _time; } }

        public Exception Exception { get { return _ex; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

    }
}
