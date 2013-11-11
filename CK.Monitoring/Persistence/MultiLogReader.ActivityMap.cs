using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    public partial class MultiLogReader : IDisposable
    {
        public class ActivityMap
        {
            readonly IReadOnlyCollection<LogFile> _allFiles;
            readonly IReadOnlyCollection<LogFile> _validFiles;
            readonly IReadOnlyCollection<Monitor> _monitorList;
            readonly Dictionary<Guid,Monitor> _monitors;
            readonly DateTime _firstEntryDate;
            readonly DateTime _lastEntryDate;

            internal ActivityMap( MultiLogReader reader )
            {
                // ConcurrentDictionary.Values is a snapshot (a ReadOnlyCollection).
                _allFiles = new CKReadOnlyCollectionOnICollection<LogFile>( reader._files.Values );
                _validFiles = _allFiles.Where( f => f.Error == null && f.TotalEntryCount > 0 ).ToReadOnlyList();
                _monitors = reader._monitors.ToDictionary( e => e.Key, e => new Monitor( e.Value ) );
                _monitorList = new CKReadOnlyCollectionOnICollection<Monitor>( _monitors.Values );
                _firstEntryDate = reader._globalFirstEntryTime;
                _lastEntryDate = reader._globalLastEntryTime;
            }

            public DateTime FirstEntryDate { get { return _firstEntryDate; } }

            public DateTime LastEntryDate { get { return _lastEntryDate; } }

            public IReadOnlyCollection<LogFile> ValidFiles { get { return _validFiles; } }

            public IReadOnlyCollection<LogFile> AllFiles { get { return _allFiles; } }

            public IReadOnlyCollection<Monitor> Monitors { get { return _monitorList; } }

            public Monitor FindMonitor( Guid monitorId )
            {
                return _monitors.GetValueWithDefault( monitorId, null );
            }
        }

        public class Monitor
        {
            readonly Guid _monitorId;
            readonly IReadOnlyList<LogFileMonitorOccurence> _files;
            DateTime _firstEntryTime;
            int _firstDepth;
            DateTime _lastEntryTime;
            int _lastDepth;

            internal Monitor( LiveIndexedMonitor m )
            {
                _monitorId = m.MonitorId;
                _files = m._files.OrderBy( f=> f.FirstEntryTime ).ToReadOnlyList();
                _firstEntryTime = m._firstEntryTime;
                _firstDepth = m._firstDepth;
                _lastEntryTime = m._lastEntryTime;
                _lastDepth = m._lastDepth;
            }

            public Guid MonitorId { get { return _monitorId; } }

            public IReadOnlyList<LogFileMonitorOccurence> Files { get { return _files; } }

            public DateTime FirstEntryTime { get { return _firstEntryTime; } }

            public int FirstDepth { get { return _firstDepth; } }

            public DateTime LastEntryTime { get { return _lastEntryTime; } }

            public int LastDepth { get { return _lastDepth; } }
        }

        public ActivityMap GetActivityMap()
        {
            _lockWriteRead.EnterWriteLock();
            try
            {
                return new ActivityMap( this );
            }
            finally
            {
                _lockWriteRead.ExitWriteLock();
            }
        }

    }

}
