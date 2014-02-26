using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    class LogPage : ILogPage
    {
        readonly IReadOnlyList<ILogEntry> _entries;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtStart;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtEnd;
        readonly int _pageNumber;

        internal LogPage(IReadOnlyList<ILogEntry> entries, IReadOnlyList<ILogEntry> openGroupsAtStart, IReadOnlyList<ILogEntry> openGroupsAtEnd, int pageNumber)
        {
            _entries = entries;
            _openGroupsAtStart = openGroupsAtStart;
            _openGroupsAtEnd = openGroupsAtEnd;
            _pageNumber = pageNumber;
        }

        #region ILogPage Members

        public IReadOnlyList<ILogEntry> Entries
        {
            get { return _entries; }
        }

        public IReadOnlyList<ILogEntry> OpenGroupsAtStart
        {
            get { return _openGroupsAtStart; }
        }

        public IReadOnlyList<ILogEntry> OpenGroupsAtEnd
        {
            get { return _openGroupsAtEnd; }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
        }

        #endregion
    }
}
