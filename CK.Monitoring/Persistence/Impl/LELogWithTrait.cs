using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.Monitoring.Impl
{
    class LELogWithTrait : ILogEntry
    {
        readonly DateTime _time;
        readonly string _text;
        readonly CKTrait _tags;
        readonly string _fileName;
        readonly int _lineNumber;
        readonly LogLevel _level;

        public LELogWithTrait( string text, DateTime t, string fileName, int lineNumber, LogLevel l, CKTrait tags )
        {
            _text = text;
            _time = t;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = l;
            _tags = tags;
        }

        public LogEntryType LogType { get { return LogEntryType.Log; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return _tags; } }

        public DateTime LogTimeUtc { get { return _time; } }

        public string FileName { get { return _fileName; } }

        public int LineNumber { get { return _lineNumber; } }

        public CKExceptionData Exception { get { return null; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

        public void Write( System.IO.BinaryWriter w )
        {
            LogEntry.WriteLog( w, _level, _time, _text, _tags, _fileName, _lineNumber );
        }
    }
}
