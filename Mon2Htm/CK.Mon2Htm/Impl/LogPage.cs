using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;
using CK.Core;

namespace CK.Mon2Htm
{
    class LogPage : IStructuredLogPage
    {
        readonly IReadOnlyList<IPagedLogEntry> _entries;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtStart;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtEnd;
        readonly int _pageNumber;

        internal LogPage(IReadOnlyList<ILogEntry> entries, IReadOnlyList<ILogEntry> openGroupsAtStart, IReadOnlyList<ILogEntry> openGroupsAtEnd, int pageNumber, MonitorIndexInfo indexInfo)
        {
            _entries = BuildStructuredLogEntries( entries, openGroupsAtStart, openGroupsAtEnd, indexInfo, pageNumber );

            _openGroupsAtStart = openGroupsAtStart;
            _openGroupsAtEnd = openGroupsAtEnd;
            _pageNumber = pageNumber;
        }

        private static IReadOnlyList<IPagedLogEntry> BuildStructuredLogEntries( IEnumerable<ILogEntry> entries, IReadOnlyList<ILogEntry> openGroupsAtStart, IReadOnlyList<ILogEntry> openGroupsAtEnd, MonitorIndexInfo indexInfo, int pageNumber )
        {
            List<PagedLogEntry> logEntries = new List<PagedLogEntry>();

            List<PagedLogEntry> currentPath = new List<PagedLogEntry>();

            // Process already-opened group

            foreach( var entry in openGroupsAtStart )
            {
                bool exists;

                var pagedEntry = new PagedLogEntry( entry );

                int groupStartPage = indexInfo.GetPageIndexOf( entry.LogTime ) + 1;
                int groupEndPage = indexInfo.GetPageIndexOf( indexInfo.Groups.GetByKey( entry.LogTime, out exists ).CloseGroupTimestamp ) + 1;

                pagedEntry.GroupStartsOnPage = groupStartPage;
                if(pageNumber != groupEndPage) pagedEntry.GroupEndsOnPage = groupEndPage;

                if( currentPath.Count > 0 )
                {
                    // Add current path as child
                    currentPath[currentPath.Count - 1].AddChild( pagedEntry );
                }
                else
                {
                    // Add to root
                    logEntries.Add( pagedEntry );
                }

                currentPath.Add( pagedEntry );
            }

            foreach( var entry in entries )
            {
                var pagedEntry = new PagedLogEntry( entry );

                if( currentPath.Count > 0  )
                {
                    // Add current path as child
                    currentPath[currentPath.Count - 1].AddChild( pagedEntry );
                }
                else
                {
                    // Add to root
                    logEntries.Add( pagedEntry );
                }

                if( pagedEntry.LogType == LogEntryType.OpenGroup )
                {
                    bool exists;

                    int groupEndPage = indexInfo.GetPageIndexOf( indexInfo.Groups.GetByKey( entry.LogTime, out exists ).CloseGroupTimestamp ) + 1;

                    pagedEntry.GroupStartsOnPage = 0;
                    if( pageNumber != groupEndPage ) pagedEntry.GroupEndsOnPage = groupEndPage;

                    currentPath.Add( pagedEntry );
                }
                else if( pagedEntry.LogType == LogEntryType.CloseGroup )
                {
                    var openGroupTimestamp = indexInfo.Groups.First( x => x.CloseGroupTimestamp == pagedEntry.LogTime ).OpenGroupTimestamp;
                    var groupStartPage = indexInfo.GetPageIndexOf( openGroupTimestamp );

                    if( pageNumber != groupStartPage ) pagedEntry.GroupStartsOnPage = groupStartPage;
                    pagedEntry.GroupEndsOnPage = 0;

                    currentPath.RemoveAt( currentPath.Count - 1 );
                }
                
            }

            return logEntries.AsReadOnlyList();
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

        public IReadOnlyList<IPagedLogEntry> Entries
        {
            get { return _entries; }
        }
    }
}
