using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.Monitoring.Impl
{
    class LELog : ILogEntry
    {
        readonly string _text;
        readonly CKTrait _tags;
        readonly string _fileName;
        readonly int _lineNumber;
        readonly LogLevel _level;
        readonly CKExceptionData _ex;
        readonly LogTimestamp _time;

        public LELog( string text, LogTimestamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
        {
            _text = text;
            _time = t;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = l;
            _tags = tags;
            _ex = ex;
        }

        public LELog( LEMCLog e )
        {
            _text = e.Text;
            _time = e.LogTime;
            _fileName = e.FileName;
            _lineNumber = e.LineNumber;
            _level = e.LogLevel;
            _tags = e.Tags;
            _ex = e.Exception;
        }

        public LogEntryType LogType { get { return LogEntryType.Line; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return _tags; } }

        public LogTimestamp LogTime { get { return _time; } }

        public string FileName { get { return _fileName; } }

        public int LineNumber { get { return _lineNumber; } }

        public CKExceptionData Exception { get { return _ex; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

        public void WriteLogEntry( System.IO.BinaryWriter w )
        {
            LogEntry.WriteLog( w, false, _level, _time, _text, _tags, _ex, _fileName, _lineNumber );
        }
    }
}
