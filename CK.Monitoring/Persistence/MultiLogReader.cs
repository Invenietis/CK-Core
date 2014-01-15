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
        readonly ConcurrentDictionary<string,RawLogFile> _files;
        readonly ReaderWriterLockSlim _lockWriteRead;

        readonly object _globalInfoLock;
        DateTime _globalFirstEntryTime;
        DateTime _globalLastEntryTime;

        internal class LiveIndexedMonitor
        {
            readonly MultiLogReader _reader;
            internal readonly Guid MonitorId;
            internal readonly List<RawLogFileMonitorOccurence> _files;
            internal DateTimeStamp _firstEntryTime;
            internal int _firstDepth;
            internal DateTimeStamp _lastEntryTime;
            internal int _lastDepth;

            internal LiveIndexedMonitor( Guid monitorId, MultiLogReader reader )
            {
                MonitorId = monitorId;
                _reader = reader;
                _files = new List<RawLogFileMonitorOccurence>();
                _firstEntryTime = DateTimeStamp.MaxValue;
                _lastEntryTime = DateTimeStamp.MinValue;
            }

            internal void Register( RawLogFileMonitorOccurence fileOccurence, bool newOccurence, long streamOffset, IMulticastLogEntry log )
            {
                lock( _files )
                {
                    Debug.Assert( newOccurence == !_files.Contains( fileOccurence ) ); 
                    if( newOccurence ) _files.Add( fileOccurence );
                    if( _firstEntryTime > log.LogTime )
                    {
                        _firstEntryTime = log.LogTime;
                        _firstDepth = log.GroupDepth;
                    }
                    if( _lastEntryTime < log.LogTime )
                    {
                        _lastEntryTime = log.LogTime;
                        _lastDepth = log.GroupDepth;
                    }
                }
            }
        }

        /// <summary>
        /// Immutable object that describes the occurrence of a Monitor in a <see cref="RawLogFile"/>.
        /// </summary>
        public class RawLogFileMonitorOccurence
        {
            public readonly RawLogFile LogFile;
            public readonly Guid MonitorId;
            public readonly long FirstOffset;
            public long LastOffset { get; internal set; }
            public DateTimeStamp FirstEntryTime { get; internal set; }
            public DateTimeStamp LastEntryTime { get; internal set; }

            /// <summary>
            /// Creates and opens a <see cref="LogReader"/> that reads unicast entries only from this monitor.
            /// The reader is initially positioned before the entry (i.e. <see cref="MoveNext"/> must be called).
            /// </summary>
            /// <param name="streamOffset">Initial stream position.</param>
            /// <returns>A log reader that will read only entries from this monitor.</returns>
            public LogReader CreateFilteredReader( long streamOffset )
            {
                return LogReader.Open( LogFile.FileName, streamOffset != -1 ? streamOffset : FirstOffset, LogFile.FileVersion, new LogReader.MulticastFilter( MonitorId, LastOffset ) );
            }

            /// <summary>
            /// Opens a <see cref="LogReader"/> that reads unicast entries only from this monitor and positions it on the first entry
            /// with the given time (i.e. <see cref="MoveNext"/> has been called).
            /// </summary>
            /// <param name="logTime">Log time. Must exist in the stream otherwise an exception is thrown.</param>
            /// <returns>A log reader that will read only entries from this monitor.</returns>
            public LogReader CreateFilteredReaderAndMoveTo( DateTimeStamp logTime )
            {
                var r = LogReader.Open( LogFile.FileName, FirstOffset, LogFile.FileVersion, new LogReader.MulticastFilter( MonitorId, LastOffset ) );
                while( r.MoveNext() && r.Current.LogTime < logTime ) ;
                return r;
            }

            internal RawLogFileMonitorOccurence( RawLogFile f, Guid monitorId, long streamOffset )
            {
                LogFile = f;
                MonitorId = monitorId;
                FirstOffset = streamOffset;
                FirstEntryTime = DateTimeStamp.MaxValue;
                LastEntryTime = DateTimeStamp.MinValue;
            }
        }

        /// <summary>
        /// Immutable object that contains a description of the content of a raw log file.
        /// </summary>
        public class RawLogFile
        {
            readonly string _fileName;
            int _fileVersion;
            DateTimeStamp _firstEntryTime;
            DateTimeStamp _lastEntryTime;
            int _totalEntryCount;
            int _unfilteredEntryCount;
            int _fatalCount;
            int _errorCount;
            int _warnCount;
            int _infoCount;
            int _traceCount;
            IReadOnlyList<RawLogFileMonitorOccurence> _monitors;
            Exception _error;

            public string FileName { get { return _fileName; } }
            public DateTimeStamp FirstEntryTime { get { return _firstEntryTime; } }
            public DateTimeStamp LastEntryTime { get { return _lastEntryTime; } }
            public int FileVersion { get { return _fileVersion; } }
            public int TotalEntryCount { get { return _totalEntryCount; } }
            public int UnfilteredEntryCount { get { return _unfilteredEntryCount; } }
            public int FatalCount { get { return _fatalCount; } }
            public int ErrorCount { get { return _errorCount; } }
            public int WarnCount { get { return _warnCount; } }
            public int InfoCount { get { return _infoCount; } }
            public int TraceCount { get { return _traceCount; } }
            public IReadOnlyList<RawLogFileMonitorOccurence> Monitors { get { return _monitors; } }

            public Exception Error { get { return _error; } }

            internal object InitializerLock;

            internal RawLogFile( string fileName )
            {
                _fileName = fileName;
                InitializerLock = new object();
                _firstEntryTime = DateTimeStamp.MaxValue;
                _lastEntryTime = DateTimeStamp.MinValue;
            }

            internal void Initialize( MultiLogReader reader )
            {
                try
                {
                    var monitorOccurences = new Dictionary<Guid, RawLogFileMonitorOccurence>();
                    var monitorOccurenceList = new List<RawLogFileMonitorOccurence>();
                    using( var r = LogReader.Open( FileName ) )
                    {
                        if( r.MoveNext() )
                        {
                            _fileVersion = r.StreamVersion;
                            do
                            {
                                var log = r.Current as IMulticastLogEntry;
                                if( log != null )
                                {
                                    UpdateLogFileStatistics( log );
                                    UpdateMonitor( reader, r.StreamOffset, monitorOccurences, monitorOccurenceList, log );
                                }
                            }
                            while( r.MoveNext() );
                        }
                    }
                    _monitors = monitorOccurenceList.ToReadOnlyList();
                }
                catch( Exception ex )
                {
                    _error = ex;
                }
            }

            void UpdateMonitor( MultiLogReader reader, long streamOffset, Dictionary<Guid, RawLogFileMonitorOccurence> monitorOccurence, List<RawLogFileMonitorOccurence> monitorOccurenceList, IMulticastLogEntry log )
            {
                bool newOccurence = false;
                RawLogFileMonitorOccurence occ;
                if( !monitorOccurence.TryGetValue( log.MonitorId, out occ ) )
                {
                    occ = new RawLogFileMonitorOccurence( this, log.MonitorId, streamOffset );
                    monitorOccurence.Add( log.MonitorId, occ );
                    monitorOccurenceList.Add( occ );
                    newOccurence = true;
                }
                if( occ.FirstEntryTime > log.LogTime ) occ.FirstEntryTime = log.LogTime;
                if( occ.LastEntryTime < log.LogTime ) occ.LastEntryTime = log.LogTime;
                occ.LastOffset = streamOffset;
                reader.RegisterOneLog( occ, newOccurence, streamOffset, log );
            }

            void UpdateLogFileStatistics( IMulticastLogEntry log )
            {
                ++_totalEntryCount;
                if( _firstEntryTime > log.LogTime ) _firstEntryTime = log.LogTime;
                if( _lastEntryTime < log.LogTime ) _lastEntryTime = log.LogTime;
                if( (log.LogLevel & LogLevel.IsFiltered) != 0 ) ++_unfilteredEntryCount;
                switch( log.LogLevel & LogLevel.Mask )
                {
                    case LogLevel.Trace: ++_traceCount; break;
                    case LogLevel.Info: ++_infoCount; break;
                    case LogLevel.Warn: ++_warnCount; break;
                    case LogLevel.Error: ++_errorCount; break;
                    case LogLevel.Fatal: ++_fatalCount; break;
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="MultiLogReader"/>.
        /// </summary>
        public MultiLogReader()
        {
            _monitors = new ConcurrentDictionary<Guid, LiveIndexedMonitor>();
            _files = new ConcurrentDictionary<string, RawLogFile>( StringComparer.InvariantCultureIgnoreCase );
            _lockWriteRead = new ReaderWriterLockSlim();
            _globalInfoLock = new object();
            _globalFirstEntryTime = DateTime.MaxValue;
            _globalLastEntryTime = DateTime.MinValue;
        }

        /// <summary>
        /// Adds a bunch of log files.
        /// </summary>
        /// <param name="files">Set of files to add.</param>
        /// <returns>List of newly added files (already existing files are skipped).</returns>
        public List<RawLogFile> Add( IEnumerable<string> files )
        {
            List<RawLogFile> result = new List<RawLogFile>();
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

        /// <summary>
        /// Adds a file to this reader. This is thread safe (can be called from any thread at any time). 
        /// </summary>
        /// <param name="filePath">The path of the file to add.</param>
        /// <param name="newFileIndex">True if the file has actually been added, false it it was already added.</param>
        /// <returns>The RawLogFile object (newly created or already existing).</returns>
        public RawLogFile Add( string filePath, out bool newFileIndex )
        {
            newFileIndex = false;
            filePath = FileUtil.NormalizePathSeparator( filePath, false );
            _lockWriteRead.EnterReadLock();
            RawLogFile f = _files.GetOrAdd( filePath, fileName => new RawLogFile( fileName ) );
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
            if( newFileIndex )
            {
                lock( _globalInfoLock )
                {
                    if( _globalFirstEntryTime > f.FirstEntryTime.TimeUtc ) _globalFirstEntryTime = f.FirstEntryTime.TimeUtc;
                    if( _globalLastEntryTime < f.LastEntryTime.TimeUtc ) _globalLastEntryTime = f.LastEntryTime.TimeUtc;
                }
            }
            _lockWriteRead.ExitReadLock();
            return f;
        }

        LiveIndexedMonitor RegisterOneLog( RawLogFileMonitorOccurence fileOccurence, bool newOccurence, long streamOffset, IMulticastLogEntry log )
        {
            Debug.Assert( fileOccurence.MonitorId == log.MonitorId );
            Debug.Assert( !newOccurence || (fileOccurence.FirstEntryTime == log.LogTime && fileOccurence.LastEntryTime == log.LogTime ) );
            LiveIndexedMonitor m = _monitors.GetOrAdd( log.MonitorId, id => new LiveIndexedMonitor( id, this ) );
            m.Register( fileOccurence, newOccurence, streamOffset, log );
            return m;
        }

        public void Dispose()
        {
            _lockWriteRead.Dispose();
        }

    }

}
