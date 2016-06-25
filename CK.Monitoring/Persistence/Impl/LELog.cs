using System.Collections.Generic;
using System.IO;
using CK.Core;
using CK.Text;

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
        readonly DateTimeStamp _time;

        public LELog( string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
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

        public LogEntryType LogType => LogEntryType.Line;

        public LogLevel LogLevel => _level;

        public string Text => _text; 

        public CKTrait Tags => _tags; 

        public DateTimeStamp LogTime => _time;

        public string FileName => _fileName; 

        public int LineNumber => _lineNumber; 

        public CKExceptionData Exception => _ex; 

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions => null; 

        public virtual void WriteLogEntry( CKBinaryWriter w )
        {
            LogEntry.WriteLog( w, false, _level, _time, _text, _tags, _ex, _fileName, _lineNumber );
        }
    }
}
