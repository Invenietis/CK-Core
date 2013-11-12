using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    public sealed partial class MultiLogReader : IDisposable
    {
        readonly ConcurrentDictionary<Guid,LiveIndexedMonitor> _monitors;
        readonly ConcurrentDictionary<string,LogFile> _files;
        readonly ReaderWriterLockSlim _lockWriteRead;

        readonly object _globalInfoLock;
        DateTime _globalFirstEntryTime;
        DateTime _globalLastEntryTime;

        internal class LiveIndexedMonitor
        {
            readonly MultiLogReader _reader;
            internal readonly Guid MonitorId;
            internal readonly List<LogFileMonitorOccurence> _files;
            internal DateTime _firstEntryTime;
            internal int _firstDepth;
            internal DateTime _lastEntryTime;
            internal int _lastDepth;

            internal LiveIndexedMonitor( Guid monitorId, MultiLogReader reader )
            {
                MonitorId = monitorId;
                _reader = reader;
                _files = new List<LogFileMonitorOccurence>();
                _firstEntryTime = DateTime.MaxValue;
                _lastEntryTime = DateTime.MinValue;
            }

            internal void Register( LogFileMonitorOccurence fileOccurence, bool newOccurence, IMulticastLogEntry log )
            {
                lock( _files )
                {
                    Debug.Assert( newOccurence == !_files.Contains( fileOccurence ) ); 
                    if( newOccurence ) _files.Add( fileOccurence );
                    if( _firstEntryTime > log.LogTimeUtc )
                    {
                        _firstEntryTime = log.LogTimeUtc;
                        _firstDepth = log.GroupDepth;
                    }
                    if( _lastEntryTime < log.LogTimeUtc )
                    {
                        _lastEntryTime = log.LogTimeUtc;
                        _lastDepth = log.GroupDepth;
                    }
                }
            }
        }

        public class LogFileMonitorOccurence
        {
            public readonly LogFile LogFile;
            public readonly Guid MonitorId;
            public DateTime FirstEntryTime { get; internal set; }
            public DateTime LastEntryTime { get; internal set; }

            internal LogFileMonitorOccurence( LogFile f, Guid monitorId, DateTime firstEntryTime )
            {
                LogFile = f;
                MonitorId = monitorId;
                FirstEntryTime = firstEntryTime;
            }
        }

        public struct LogFileIndex
        {
            public readonly DateTime LogTime;
            public readonly long FileOffset;

            internal LogFileIndex( DateTime t, long p )
            {
                LogTime = t;
                FileOffset = p;
            }
        }

        public class LogFile
        {
            readonly string _fileName;
            DateTime _firstEntryTime;
            DateTime _lastEntryTime;
            int _totalEntryCount;
            int _unfilteredEntryCount;
            int _fatalCount;
            int _errorCount;
            int _warnCount;
            int _infoCount;
            int _traceCount;
            IReadOnlyList<LogFileMonitorOccurence> _monitors;
            IReadOnlyList<LogFileIndex> _indicies;
            Exception _error;

            public string FileName { get { return _fileName; } }
            public DateTime FirstEntryTime { get { return _firstEntryTime; } }
            public DateTime LastEntryTime { get { return _lastEntryTime; } }
            public int TotalEntryCount { get { return _totalEntryCount; } }
            public int UnfilteredEntryCount { get { return _unfilteredEntryCount; } }
            public int FatalCount { get { return _fatalCount; } }
            public int ErrorCount { get { return _errorCount; } }
            public int WarnCount { get { return _warnCount; } }
            public int InfoCount { get { return _infoCount; } }
            public int TraceCount { get { return _traceCount; } }
            public IReadOnlyList<LogFileMonitorOccurence> Monitors { get { return _monitors; } }
            public IReadOnlyList<LogFileIndex> Indicies { get { return _indicies; } }

            public Exception Error { get { return _error; } }

            internal object InitializerLock;

            internal LogFile( string fileName )
            {
                _fileName = fileName;
                InitializerLock = new object();
                _firstEntryTime = DateTime.MaxValue;
            }

            internal void Initialize( MultiLogReader reader )
            {
                try
                {
                    var monitorOccurences = new Dictionary<Guid, LogFileMonitorOccurence>();
                    var monitorOccurenceList = new List<LogFileMonitorOccurence>();
                    var indicies = new List<LogFileIndex>();
                    int indexRemainder = reader.BucketSize;
                    using( var r = LogReader.Open( FileName ) )
                    {
                        while( r.MoveNext() )
                        {
                            var log = r.Current as IMulticastLogEntry;
                            if( log != null )
                            {
                                UpdateStatistics( log );
                                UpdateMonitor( reader, monitorOccurences, monitorOccurenceList, log );
                                if( --indexRemainder == 0 )
                                {
                                    indicies.Add( new LogFileIndex( log.LogTimeUtc, r.StreamOffset ) );
                                    indexRemainder = reader.BucketSize;
                                }
                            }
                        }
                    }
                    _monitors = monitorOccurenceList.ToReadOnlyList();
                    _indicies = indicies.ToReadOnlyList();
                }
                catch( Exception ex )
                {
                    _error = ex;
                }
            }

            private void UpdateMonitor( MultiLogReader reader, Dictionary<Guid, LogFileMonitorOccurence> monitorOccurence, List<LogFileMonitorOccurence> monitorOccurenceList, IMulticastLogEntry log )
            {
                bool newOccurence = false;
                LogFileMonitorOccurence occ;
                if( !monitorOccurence.TryGetValue( log.MonitorId, out occ ) )
                {
                    occ = new LogFileMonitorOccurence( this, log.MonitorId, log.LogTimeUtc );
                    monitorOccurence.Add( log.MonitorId, occ );
                    monitorOccurenceList.Add( occ );
                    newOccurence = true;
                }
                occ.LastEntryTime = log.LogTimeUtc;
                reader.RegisterOneLog( occ, newOccurence, log );
            }

            private void UpdateStatistics( IMulticastLogEntry log )
            {
                if( ++_totalEntryCount == 1 )
                {
                    _firstEntryTime = log.LogTimeUtc;
                }
                if( (log.LogLevel & LogLevel.IsFiltered) != 0 ) ++_unfilteredEntryCount;
                switch( log.LogLevel & LogLevel.Mask )
                {
                    case LogLevel.Trace: ++_traceCount; break;
                    case LogLevel.Info: ++_infoCount; break;
                    case LogLevel.Warn: ++_warnCount; break;
                    case LogLevel.Error: ++_errorCount; break;
                    case LogLevel.Fatal: ++_fatalCount; break;
                }
                _lastEntryTime = log.LogTimeUtc;
            }
        }

        public MultiLogReader()
        {
            _monitors = new ConcurrentDictionary<Guid, LiveIndexedMonitor>();
            _files = new ConcurrentDictionary<string, LogFile>( StringComparer.InvariantCultureIgnoreCase );
            _lockWriteRead = new ReaderWriterLockSlim();
            _globalInfoLock = new object();
            _globalFirstEntryTime = DateTime.MaxValue;
            _globalLastEntryTime = DateTime.MinValue;
            BucketSize = 200;
        }

        /// <summary>
        /// Gets or set the size of the indexing buckets.
        /// Defaults to 200.
        /// </summary>
        public int BucketSize { get; set; }

        public List<LogFile> Add( IEnumerable<string> files )
        {
            List<LogFile> result = new List<LogFile>();
            Parallel.ForEach( files, s => 
            { 
                bool newOne;
                var f = Add( s, out newOne );
                lock( result )
                {
                    if( !result.Contains( f ) ) result.Add( f );
                }
            } );
            return result;
        }

        public LogFile Add( string filePath, out bool newFileIndex )
        {
            newFileIndex = false;
            filePath = FileUtil.NormalizePathSeparator( filePath, false );
            _lockWriteRead.EnterReadLock();
            LogFile f = _files.GetOrAdd( filePath, fileName => new LogFile( fileName ) );
            var l = f.InitializerLock;
            if( l != null )
            {
                lock( l )
                {
                    if( f.InitializerLock != null )
                    {
                        newFileIndex = true;
                        f.Initialize( this );
                        f.InitializerLock = null;
                    }
                }
            }
            lock( _globalInfoLock )
            {
                if( _globalFirstEntryTime > f.FirstEntryTime ) _globalFirstEntryTime = f.FirstEntryTime;
                if( _globalLastEntryTime > f.LastEntryTime ) _globalLastEntryTime = f.LastEntryTime;
            }
            _lockWriteRead.ExitReadLock();
            return f;
        }

        LiveIndexedMonitor RegisterOneLog( LogFileMonitorOccurence fileOccurence, bool newOccurence, IMulticastLogEntry log )
        {
            Debug.Assert( fileOccurence.MonitorId == log.MonitorId );
            Debug.Assert( !newOccurence || (fileOccurence.FirstEntryTime == log.LogTimeUtc && fileOccurence.LastEntryTime == log.LogTimeUtc ) );
            LiveIndexedMonitor m = _monitors.GetOrAdd( log.MonitorId, id => new LiveIndexedMonitor( id, this ) );
            m.Register( fileOccurence, newOccurence, log );
            return m;
        }

        public void Dispose()
        {
            _lockWriteRead.Dispose();
        }

    }

}
