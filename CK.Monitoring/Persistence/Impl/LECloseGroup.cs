using CK.Core;
using CK.Text;
using System.Collections.Generic;
using System.IO;

namespace CK.Monitoring.Impl
{
    class LECloseGroup: ILogEntry
    {
        readonly LogLevel _level;
        readonly IReadOnlyList<ActivityLogGroupConclusion> _conclusions;
        readonly DateTimeStamp _time;

        public LECloseGroup( DateTimeStamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c ) 
        {
            _time = t;
            _conclusions = c;
            _level = level;
        }

        public LECloseGroup( LEMCCloseGroup e ) 
        {
            _time = e.LogTime;
            _conclusions = e.Conclusions;
            _level = e.LogLevel;
        }

        public LogEntryType LogType => LogEntryType.CloseGroup;

        public string Text => null; 

        public LogLevel LogLevel => _level; 

        public DateTimeStamp LogTime => _time;

        public CKExceptionData Exception => null; 

        public string FileName => null;
        
        public int LineNumber => 0; 

        public CKTrait Tags => ActivityMonitor.Tags.Empty; 

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions => _conclusions;

        public virtual void WriteLogEntry( CKBinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _level, _time, _conclusions );
        }
        
    }
}
