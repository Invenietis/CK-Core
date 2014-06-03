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
        readonly string _text;
        readonly CKTrait _tags;
        readonly string _fileName;
        readonly int _lineNumber;
        readonly LogLevel _level;
        readonly CKExceptionData _ex;
        readonly DateTimeStamp _time;

        public LEOpenGroup( string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
        {
            _text = text;
            _time = t;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = l;
            _tags = tags;
            _ex = ex;
        }

        public LEOpenGroup( LEMCOpenGroup e )
        {
            _text = e.Text;
            _time = e.LogTime;
            _fileName = e.FileName;
            _lineNumber = e.LineNumber;
            _level = e.LogLevel;
            _tags = e.Tags;
            _ex = e.Exception;
        }

        public LogEntryType LogType { get { return LogEntryType.OpenGroup; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return _tags; } }

        public DateTimeStamp LogTime { get { return _time; } }

        public CKExceptionData Exception { get { return _ex; } }

        public string FileName { get { return _fileName; } }

        public int LineNumber { get { return _lineNumber; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

        public virtual void WriteLogEntry( System.IO.BinaryWriter w )
        {
            LogEntry.WriteLog( w, true, _level, _time, _text, _tags, _ex, _fileName, _lineNumber );
        }
    }
}
