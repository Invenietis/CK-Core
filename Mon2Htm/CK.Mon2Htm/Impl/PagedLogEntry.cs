using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    class PagedLogEntry : IPagedLogEntry
    {
        ILogEntry _entry;
        List<IPagedLogEntry> _children;

        public PagedLogEntry(ILogEntry rootEntry)
        {
            _entry = rootEntry;
            if( rootEntry.LogType == LogEntryType.OpenGroup ) _children = new List<IPagedLogEntry>();
        }

        internal void AddChild( IPagedLogEntry entry )
        {
            _children.Add( entry );
        }

        public IReadOnlyList<IPagedLogEntry> Children { get { return _children.AsReadOnly(); } }

        public int GroupStartsOnPage { get; internal set; }

        public int GroupEndsOnPage { get; internal set; }

        #region ILogEntry Members

        public LogEntryType LogType
        {
            get { return _entry.LogType; }
        }

        public Core.LogLevel LogLevel
        {
            get { return _entry.LogLevel; }
        }

        public string Text
        {
            get { return _entry.Text; }
        }

        public Core.CKTrait Tags
        {
            get { return _entry.Tags; }
        }

        public Core.DateTimeStamp LogTime
        {
            get { return _entry.LogTime; }
        }

        public Core.CKExceptionData Exception
        {
            get { return _entry.Exception; }
        }

        public string FileName
        {
            get { return _entry.FileName; }
        }

        public int LineNumber
        {
            get { return _entry.LineNumber; }
        }

        public IReadOnlyList<Core.ActivityLogGroupConclusion> Conclusions
        {
            get { return _entry.Conclusions; }
        }

        public void WriteLogEntry( System.IO.BinaryWriter w )
        {
            _entry.WriteLogEntry( w );
        }

        #endregion
    }
}
