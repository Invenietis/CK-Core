using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public LogEntryType LogType { get { return LogEntryType.CloseGroup; } }

        public string Text { get { return null; } }

        public LogLevel LogLevel { get { return _level; } }

        public DateTimeStamp LogTime { get { return _time; } }

        public CKExceptionData Exception { get { return null; } }

        public string FileName { get { return null; } }
        
        public int LineNumber { get { return 0; } }

        public CKTrait Tags { get { return ActivityMonitor.Tags.Empty; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return _conclusions; } }

        public virtual void WriteLogEntry( BinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _level, _time, _conclusions );
        }
        
    }
}
