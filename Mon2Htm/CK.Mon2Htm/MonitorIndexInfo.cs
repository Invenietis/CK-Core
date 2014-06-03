using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public class MonitorIndexInfo
    {
        readonly Guid _monitorGuid;

        int _totalEntryCount, _totalTraceCount, _totalInfoCount, _totalWarnCount, _totalErrorCount, _totalFatalCount;
        int _pageLength;
        string _monitorTitle;

        List<MonitorPageReference> _pages;

        List<DateTimeStamp> _warnTimestamps;
        List<DateTimeStamp> _errorTimestamps;
        List<DateTimeStamp> _fatalTimestamps;
        CKSortedArrayKeyList<MonitorGroupReference,DateTimeStamp> _groups;
        Dictionary<DateTimeStamp,int> _logTimeToPage;

        private MonitorGroupReference GetCurrentGroupReference( List<ILogEntry> groupsPath )
        {
            if( groupsPath.Count > 0 )
            {
                return _groups.GetByKey( groupsPath[groupsPath.Count - 1].LogTime );
            }
            return null;
        }

        public MonitorGroupReference GetGroupReference( DateTimeStamp s )
        {
            return _groups.GetByKey( s );
        }

        public MonitorGroupReference GetGroupReference( ILogEntry e )
        {
            return GetGroupReference( e.LogTime );
        }

        private MonitorIndexInfo( Guid monitorGuid )
        {
            _monitorGuid = monitorGuid;

            _totalTraceCount = 0;
            _totalInfoCount = 0;
            _totalWarnCount = 0;
            _totalErrorCount = 0;
            _totalFatalCount = 0;

            _logTimeToPage = new Dictionary<DateTimeStamp, int>();
            _warnTimestamps = new List<DateTimeStamp>();
            _errorTimestamps = new List<DateTimeStamp>();
            _fatalTimestamps = new List<DateTimeStamp>();
            _groups = new CKSortedArrayKeyList<MonitorGroupReference, DateTimeStamp>( gr => gr.OpenGroupTimestamp, ( a, b ) => b.CompareTo( a ), false );
            _pages = new List<MonitorPageReference>();
        }

        public Guid MonitorGuid { get { return _monitorGuid; } }
        public int TotalTraceCount { get { return _totalTraceCount; } }
        public int TotalInfoCount { get { return _totalInfoCount; } }
        public int TotalWarnCount { get { return _totalWarnCount; } }
        public int TotalErrorCount { get { return _totalErrorCount; } }
        public int TotalFatalCount { get { return _totalFatalCount; } }
        public int TotalEntryCount { get { return _totalEntryCount; } }
        public int PageCount { get { return _pages.Count; } }
        public int PageLength { get { return _pageLength; } }
        public string MonitorTitle { get { if( String.IsNullOrEmpty( _monitorTitle ) ) { return _monitorGuid.ToString(); } else { return _monitorTitle; } } }

        public IReadOnlyList<MonitorPageReference> Pages { get { return _pages.ToReadOnlyList(); } }
        public ICKReadOnlyUniqueKeyedCollection<MonitorGroupReference, DateTimeStamp> Groups { get { return _groups; } }

        public static MonitorIndexInfo IndexMonitor( MultiLogReader.Monitor monitor, int itemsPerPage = 100 )
        {
            var index = new MonitorIndexInfo( monitor.MonitorId );

            index._pageLength = itemsPerPage;

            index.BuildIndex( monitor, itemsPerPage );

            return index;
        }

        public int GuessTimestampPage( DateTimeStamp t, int startPageIndex = 0 )
        {
            for( int i = startPageIndex; i < Pages.Count; i++ )
            {
                var pageRef = Pages[i];
                if( t >= pageRef.FirstEntryTimestamp && t <= pageRef.LastEntryTimestamp ) return i;
            }

            return -1;
        }

        public int GetPageIndexOf( DateTimeStamp t )
        {
            int pageNum;
            if( _logTimeToPage.TryGetValue( t, out pageNum ) )
            {
                return pageNum;
            }
            return -1;
        }

        private void BuildIndex( MultiLogReader.Monitor monitor, int itemsPerPage )
        {
            var page = monitor.ReadFirstPage( monitor.FirstEntryTime, itemsPerPage );
            IReadOnlyList<ILogEntry> pagePath = null;
            int i = 1;
            do
            {
                pagePath = AddPage( page, i++, pagePath );
            }
            while( page.ForwardPage() > 0 );
        }

        private void AddTimestampEntry( DateTimeStamp t, int pageIndex )
        {
            if( _logTimeToPage.ContainsKey( t ) ) return;

            _logTimeToPage.Add( t, pageIndex );
        }

        private IReadOnlyList<ILogEntry> AddPage( MultiLogReader.Monitor.LivePage page, int pageNumber, IReadOnlyList<ILogEntry> previousPageEndPath = null )
        {
            if( previousPageEndPath == null ) previousPageEndPath = new List<ILogEntry>().ToReadOnlyList();
            MonitorPageReference pageRef = new MonitorPageReference();
            pageRef.PageNumber = pageNumber;
            pageRef.PageLength = page.PageLength;
            pageRef.EntryCount = page.Entries.Count;
            _totalEntryCount += pageRef.EntryCount;
            List<ILogEntry> groupsPath = previousPageEndPath.ToList();
            List<MonitorGroupReference> groupRefsPath = previousPageEndPath.Select( x => GetGroupReference( x ) ).ToList();

            int i = 0;
            foreach( var parentedEntry in page.Entries )
            {
                if( i == 0 )
                {
                    pageRef.FirstEntryTimestamp = parentedEntry.Entry.LogTime;
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }
                if( i == page.Entries.Count - 1 )
                {
                    pageRef.LastEntryTimestamp = parentedEntry.Entry.LogTime;
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }

                if( parentedEntry.Entry.Tags.IsSupersetOf(ActivityMonitor.Tags.MonitorTopicChanged) && _monitorTitle == null)
                {
                    _monitorTitle = parentedEntry.Entry.Text;
                }

                if( parentedEntry.Entry.LogLevel.HasFlag( LogLevel.Trace ) ) _totalTraceCount++;
                if( parentedEntry.Entry.LogLevel.HasFlag( LogLevel.Info ) ) _totalInfoCount++;
                if( parentedEntry.Entry.LogLevel.HasFlag( LogLevel.Warn ) )
                {
                    _totalWarnCount++;
                    _warnTimestamps.Add( parentedEntry.Entry.LogTime );
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }
                if( parentedEntry.Entry.LogLevel.HasFlag( LogLevel.Error ) )
                {
                    _totalErrorCount++;
                    _errorTimestamps.Add( parentedEntry.Entry.LogTime );
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }
                if( parentedEntry.Entry.LogLevel.HasFlag( LogLevel.Fatal ) )
                {
                    _totalFatalCount++;
                    _fatalTimestamps.Add( parentedEntry.Entry.LogTime );
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }

                if( parentedEntry.Entry.LogType == LogEntryType.OpenGroup )
                {
                    MonitorGroupReference groupRef = new MonitorGroupReference()
                    {
                        OpenGroupEntry = parentedEntry.Entry,
                        OpenGroupTimestamp = parentedEntry.Entry.LogTime,
                        HighestLogLevel = parentedEntry.Entry.LogLevel
                    };
                    groupsPath.Add( parentedEntry.Entry );
                    groupRefsPath.Add( groupRef );
                    _groups.Add( groupRef );
                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }
                else if( parentedEntry.Entry.LogType == LogEntryType.CloseGroup )
                {
                    if( !parentedEntry.Parent.IsMissing )
                    {
                        var openGroupEntry = groupsPath[groupsPath.Count - 1];
                        var existingGroupRef = _groups.GetByKey( openGroupEntry.LogTime );

                        existingGroupRef.CloseGroupTimestamp = parentedEntry.Entry.LogTime;

                        groupsPath.RemoveAt( groupsPath.Count - 1 );
                        groupRefsPath.RemoveAt( groupRefsPath.Count - 1 );

                        AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                    }

                    AddTimestampEntry( parentedEntry.Entry.LogTime, _pages.Count );
                }

                // Update HighestLogLevel for all parent groups
                for( int j = groupRefsPath.Count - 1; j >= 0; j-- )
                {
                    MonitorGroupReference groupRef = groupRefsPath[j];

                    if( parentedEntry.Entry.LogLevel > groupRef.HighestLogLevel )
                    {
                        groupRef.HighestLogLevel = parentedEntry.Entry.LogLevel;
                    }
                    else
                    {
                        break;
                    }
                }

                i++;
            }
            _pages.Add( pageRef );

            return groupsPath.ToReadOnlyList();
        }
    }

    public class MonitorPageReference
    {
        public DateTimeStamp FirstEntryTimestamp { get; internal set; }
        public DateTimeStamp LastEntryTimestamp { get; internal set; }
        public int PageLength { get; internal set; }
        public int EntryCount { get; internal set; }
        public int PageNumber { get; internal set; }
    }

    public class MonitorGroupReference
    {
        public LogLevel HighestLogLevel { get; internal set; }
        public DateTimeStamp OpenGroupTimestamp { get; internal set; }
        public DateTimeStamp CloseGroupTimestamp { get; internal set; }
        public ILogEntry OpenGroupEntry { get; internal set; }
    }
}
