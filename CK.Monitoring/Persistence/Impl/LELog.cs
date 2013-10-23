using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.Monitoring.Impl
{
    class LELog : ILogEntry
    {
        readonly DateTime _time;
        readonly string _text;
        readonly string _fileName;
        readonly int _lineNumber;
        readonly LogLevel _level;

        public LELog( string text, DateTime t, string fileName, int lineNumber, LogLevel l ) 
        {
            _text = text;
            _time = t;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = l;
        }

        public LogEntryType LogType { get { return LogEntryType.Log; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return ActivityMonitor.EmptyTag; } }

        public DateTime LogTimeUtc { get { return _time; } }

        public CKExceptionData Exception { get { return null; } }

        public string FileName { get { return _fileName; } }

        public int LineNumber { get { return _lineNumber; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

        public void Write( System.IO.BinaryWriter w )
        {
            LogEntry.WriteLog( w, _level, _time, _text, null, _fileName, _lineNumber );
        }
    }
}
