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
    /// <summary>
    /// This reader process multiples .ckmon files (possibly in different steps: it internally aggregates the result) and can 
    /// create <see cref="ActivityMap"/> objects on demand.
    /// It is a thread safe object (the ActivityMap is immutable).
    /// </summary>
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
            internal Dictionary<CKTrait,int> _tags; 

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
                    if( !log.Tags.IsEmpty )
                    {
                        if( _tags == null )
                        {
                            _tags = new Dictionary<CKTrait, int>();
                            foreach( var t in log.Tags.AtomicTraits ) _tags.Add( t, 1 );
                        }
                        else
                        {
                            foreach( var t in log.Tags.AtomicTraits )
                            {
                                int count;
                                _tags.TryGetValue( t, out count );
                                _tags[t] = count + 1;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Immutable object that describes the occurrence of a Monitor in a <see cref="RawLogFile"/>.
        /// </summary>
        public sealed class RawLogFileMonitorOccurence
        {
            /// <summary>
            /// The <see cref="RawLogFile"/>.
            /// </summary>
            public readonly RawLogFile LogFile;
            /// <summary>
            /// The monitor's identifier.
            /// </summary>
            public readonly Guid MonitorId;
            /// <summary>
            /// First offset for this <see cref="MonitorId"/> in this <see cref="LogFile"/>.
            /// </summary>
            public readonly long FirstOffset;
            /// <summary>
            /// Last offset for this <see cref="MonitorId"/> in this <see cref="LogFile"/>.
            /// </summary>
            public long LastOffset { get; internal set; }
            /// <summary>
            /// First entry time for this <see cref="MonitorId"/> in this <see cref="LogFile"/>.
            /// </summary>
            public DateTimeStamp FirstEntryTime { get; internal set; }
            /// <summary>
            /// Last entry time for this <see cref="MonitorId"/> in this <see cref="LogFile"/>.
            /// </summary>
            public DateTimeStamp LastEntryTime { get; internal set; }

            /// <summary>
            /// Creates and opens a <see cref="LogReader"/> that reads unicast entries only from this monitor.
            /// The reader is initially positioned before the entry (i.e. <see cref="LogReader.MoveNext"/> must be called).
            /// </summary>
            /// <param name="streamOffset">Initial stream position.</param>
            /// <returns>A log reader that will read only entries from this monitor.</returns>
            public LogReader CreateFilteredReader( long streamOffset )
            {
                return LogReader.Open( LogFile.FileName, streamOffset != -1 ? streamOffset : FirstOffset, LogFile.FileVersion, new LogReader.MulticastFilter( MonitorId, LastOffset ) );
            }

            /// <summary>
            /// Opens a <see cref="LogReader"/> that reads unicast entries only from this monitor and positions it on the first entry
            /// with the given time (i.e. <see cref="LogReader.MoveNext"/> has been called).
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
        public sealed class RawLogFile
        {
            readonly string _fileName;
            int _fileVersion;
            DateTimeStamp _firstEntryTime;
            DateTimeStamp _lastEntryTime;
            int _totalEntryCount;
            IReadOnlyList<RawLogFileMonitorOccurence> _monitors;
            Exception _error;
            bool _badEndOfFile;

            /// <summary>
            /// Gets the file name.
            /// </summary>
            public string FileName { get { return _fileName; } }

            /// <summary>
            /// Gets the first entry time.
            /// </summary>
            public DateTimeStamp FirstEntryTime { get { return _firstEntryTime; } }
            
            /// <summary>
            /// Gets the last entry time.
            /// </summary>
            public DateTimeStamp LastEntryTime { get { return _lastEntryTime; } }

            /// <summary>
            /// Gets the file version.
            /// </summary>
            public int FileVersion { get { return _fileVersion; } }
            
            /// <summary>
            /// Gets the total number of entries.
            /// </summary>
            public int TotalEntryCount { get { return _totalEntryCount; } }
            
            /// <summary>
            /// Gets whether this file does not end with the end of stream marker (a zero byte).
            /// </summary>
            public bool BadEndOfFile { get { return _badEndOfFile; } }

            /// <summary>
            /// Gets whether no <see cref="Error"/> occurred and there is no <see cref="BadEndOfFile"/>.
            /// </summary>
            public bool IsValidFile { get { return !_badEndOfFile && _error == null; } }

            /// <summary>
            /// Gets the <see cref="Exception"/> that occurred while reading file.
            /// </summary>
            public Exception Error { get { return _error; } }
            
            /// <summary>
            /// Gets the different monitors that appear in this file.
            /// </summary>
            public IReadOnlyList<RawLogFileMonitorOccurence> Monitors { get { return _monitors; } }

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
                    using( var r = LogReader.Open( _fileName ) )
                    {
                        if( r.MoveNext() )
                        {
                            _fileVersion = r.StreamVersion;
                            do
                            {
                                var log = r.Current as IMulticastLogEntry;
                                if( log != null )
                                {
                                    ++_totalEntryCount;
                                    if( _firstEntryTime > log.LogTime ) _firstEntryTime = log.LogTime;
                                    if( _lastEntryTime < log.LogTime ) _lastEntryTime = log.LogTime;
                                    UpdateMonitor( reader, r.StreamOffset, monitorOccurences, monitorOccurenceList, log );
                                }
                            }
                            while( r.MoveNext() );
                        }
                        _badEndOfFile = r.BadEndOfFileMarker;
                        _error = r.ReadException;
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

            /// <summary>
            /// Overridden to return details about its content.
            /// </summary>
            /// <returns>Detailed string.</returns>
            public override string ToString()
            {
                return String.Format( "File: '{0}' ({1}), from {2} for {3}, Error={4}", FileName, TotalEntryCount, _firstEntryTime, _lastEntryTime.TimeUtc-_firstEntryTime.TimeUtc, _error != null ? _error.ToString() : "None" );
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
        /// <returns>List of newly added files (already known files are skipped).</returns>
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

        /// <summary>
        /// Releases this reader.
        /// </summary>
        public void Dispose()
        {
            _lockWriteRead.Dispose();
        }

    }

}
