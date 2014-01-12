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
        public class ActivityMap
        {
            readonly IReadOnlyCollection<RawLogFile> _allFiles;
            readonly IReadOnlyCollection<RawLogFile> _validFiles;
            readonly IReadOnlyCollection<Monitor> _monitorList;
            readonly Dictionary<Guid,Monitor> _monitors;
            readonly DateTime _firstEntryDate;
            readonly DateTime _lastEntryDate;

            internal ActivityMap( MultiLogReader reader )
            {
                // ConcurrentDictionary.Values is a snapshot (a ReadOnlyCollection), this is why 
                // it is safe to wrap it in a IReadOnlyCollection wrapper.
                _allFiles = new CKReadOnlyCollectionOnICollection<RawLogFile>( reader._files.Values );
                _validFiles = _allFiles.Where( f => f.Error == null && f.TotalEntryCount > 0 ).ToReadOnlyList();
                _monitors = reader._monitors.ToDictionary( e => e.Key, e => new Monitor( e.Value ) );
                _monitorList = new CKReadOnlyCollectionOnICollection<Monitor>( _monitors.Values );
                _firstEntryDate = reader._globalFirstEntryTime;
                _lastEntryDate = reader._globalLastEntryTime;
            }

            public DateTime FirstEntryDate { get { return _firstEntryDate; } }

            public DateTime LastEntryDate { get { return _lastEntryDate; } }

            public IReadOnlyCollection<RawLogFile> ValidFiles { get { return _validFiles; } }

            public IReadOnlyCollection<RawLogFile> AllFiles { get { return _allFiles; } }

            public IReadOnlyCollection<Monitor> Monitors { get { return _monitorList; } }

            public Monitor FindMonitor( Guid monitorId )
            {
                return _monitors.GetValueWithDefault( monitorId, null );
            }
        }

        public class Monitor
        {
            readonly Guid _monitorId;
            readonly IReadOnlyList<RawLogFileMonitorOccurence> _files;
            readonly LogTimestamp _firstEntryTime;
            readonly int _firstDepth;
            readonly LogTimestamp _lastEntryTime;
            readonly int _lastDepth;

            internal Monitor( LiveIndexedMonitor m )
            {
                _monitorId = m.MonitorId;
                _files = m._files.OrderBy( f => f.FirstEntryTime ).ToReadOnlyList();
                _firstEntryTime = m._firstEntryTime;
                _firstDepth = m._firstDepth;
                _lastEntryTime = m._lastEntryTime;
                _lastDepth = m._lastDepth;
            }

            public Guid MonitorId { get { return _monitorId; } }

            public IReadOnlyList<RawLogFileMonitorOccurence> Files { get { return _files; } }

            public LogTimestamp FirstEntryTime { get { return _firstEntryTime; } }

            public int FirstDepth { get { return _firstDepth; } }

            public LogTimestamp LastEntryTime { get { return _lastEntryTime; } }

            public int LastDepth { get { return _lastDepth; } }

            internal class MultiFileReader : IDisposable
            {
                readonly LogTimestamp _firstLogTime;
                readonly IReadOnlyList<RawLogFileMonitorOccurence> _files;
                CKSortedArrayList<OneLogReader> _readers;

                public MultiFileReader( LogTimestamp firstLogTime, IReadOnlyList<RawLogFileMonitorOccurence> files )
                {
                    _firstLogTime = firstLogTime;
                    _files = files;
                }

                public ILogEntry Current { get { return _readers[0].Head.Entry; } }

                class OneLogReader : IDisposable
                {
                    public LogEntryWithOffset Head;
                    LogReader _reader;
                    public readonly RawLogFileMonitorOccurence File;
                    public readonly int FirstGroupDepth;

                    public OneLogReader( RawLogFileMonitorOccurence file, LogTimestamp firstLogTime )
                    {
                        File = file;
                        _reader = file.CreateFilteredReaderAndMoveTo( firstLogTime );
                        FirstGroupDepth = _reader.MultiCastCurrentGroupDepth;
                        Head = _reader.CurrentWithOffset;
                    }

                    public OneLogReader( RawLogFileMonitorOccurence file, long offset )
                    {
                        _reader = file.CreateFilteredReader( offset );
                        _reader.MoveNext();
                        FirstGroupDepth = _reader.MultiCastCurrentGroupDepth;
                        Head = _reader.CurrentWithOffset;
                    }

                    public bool Forward()
                    {
                        Debug.Assert( _reader != null );
                        if( _reader.MoveNext() )
                        {
                            Head = _reader.CurrentWithOffset;
                            return true;
                        }
                        _reader.Dispose();
                        _reader = null;
                        return false;
                    }

                    /// <summary>
                    /// Compares Head.Entry.LogTime. 
                    /// </summary>
                    static public int CompareHeadTime( OneLogReader r1, OneLogReader r2 )
                    {
                        return r1.Head.Entry.LogTime.CompareTo( r2.Head.Entry.LogTime );
                    }

                    public void Dispose()
                    {
                        if( _reader != null )
                        {
                            _reader.Dispose();
                            _reader = null;
                        }
                    }
                }

                public bool MoveNext()
                {
                    if( _readers == null )
                    {
                        if( _files.Count == 0 ) return false;
                        _readers = new CKSortedArrayList<OneLogReader>( OneLogReader.CompareHeadTime, allowDuplicates: true );
                        foreach( var r in _files.Where( occ => occ.LastEntryTime >= _firstLogTime ).Select( occ => new OneLogReader( occ, _firstLogTime ) ) )
                        {
                            _readers.Add( r );
                        }
                        if( _readers.Count == 0 ) return false;
                        RemoveAllDuplicates();
                        Debug.Assert( _readers.Count > 0 );
                        return true;
                    }
                    if( _readers.Count == 0 ) return false;
                    if( !_readers[0].Forward() )
                    {
                        _readers.RemoveAt( 0 );
                        return _readers.Count > 0;
                    }
                    else
                    {
                        RemoveDuplicateAround( _readers.CheckPosition( 0 ) );
                        Debug.Assert( _readers.Count > 0 );
                        return true;
                    }
                }

                void RemoveDuplicateAround( int idx )
                {
                    // We can not be intelligent here:
                    // - The CKSortedArrayList does not guaranty a stable sort: the new reader position
                    //   should be checked around again.
                    // - Recursivity is a bad idea: imagine 2 identical files...
                    // ==> Lookups on the left and on the right and if a duplicate has been found, relies on RemoveAllDuplicates.
                    if( idx > 0 && RemoveDuplicate( idx - 1, idx ) ) RemoveAllDuplicates();
                    else if( idx < _readers.Count - 1 && RemoveDuplicate( idx, idx + 1 ) ) RemoveAllDuplicates();
                }

                void RemoveAllDuplicates()
                {
                    bool doItAgain = false;
                    do
                    {
                        for( int i = 0; i < _readers.Count - 1; ++i )
                        {
                            doItAgain |= RemoveDuplicate( i, i + 1 );
                        }
                    }
                    while( doItAgain );
                }

                bool RemoveDuplicate( int i1, int i2 )
                {
                    var first = _readers[i1];
                    var second = _readers[i2];
                    if( first.Head.Entry.LogTime == second.Head.Entry.LogTime )
                    {
                        if( !second.Forward() )
                        {
                            _readers.RemoveAt( i2 );
                        }
                        else
                        {
                            _readers.CheckPosition( i2 );
                        }
                        return true;
                    }
                    return false;
                }

                public void Dispose()
                {
                    if( _readers == null )
                        for( int i = 0; i < _readers.Count; ++i ) _readers[i].Dispose();
                }
            }

            public class LivePage : IDisposable
            {
                class WrappedList : IReadOnlyList<ILogEntry>
                {
                    public readonly ILogEntry[] Entries;

                    public WrappedList( ILogEntry[] entries )
                    {
                        Entries = entries;
                    }

                    public ILogEntry this[int index]
                    {
                        get
                        {
                            if( index >= Count ) throw new ArgumentOutOfRangeException();
                            return Entries[index];
                        }
                    }

                    public int Count { get; set; }

                    public IEnumerator<ILogEntry> GetEnumerator()
                    {
                        return Entries.Take( Count ).GetEnumerator();
                    }

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                    {
                        return GetEnumerator();
                    }

                    internal void FillPage( MultiFileReader r, List<ILogEntry> path )
                    {
                        int i;
                        for( i = 0; i < Entries.Length; ++i )
                        {
                            var e = r.Current;
                            Entries[i] = e;
                            if( e.LogType == LogEntryType.CloseGroup )
                            {
                                if( path.Count > 0 ) path.RemoveAt( path.Count - 1 );
                            }
                            else if( e.LogType == LogEntryType.OpenGroup ) path.Add( e );
                            if( !r.MoveNext() ) break;
                        }
                        Count = i + 1;
                    }
                }

                readonly WrappedList _entries;
                readonly MultiFileReader _r;
                readonly int _pageLength;
                readonly List<ILogEntry> _path;

                internal LivePage( int initialGroupDepth, ILogEntry[] entries, MultiFileReader r, int pageLength )
                {
                    Debug.Assert( pageLength == entries.Length || entries.Length == 0 );
                    _r = r;
                    _pageLength = pageLength;
                    _path = new List<ILogEntry>();
                    for( int i = 0; i < initialGroupDepth; ++i ) _path.Add( null );
                    _entries = new WrappedList( entries );
                    if( _r != null ) _entries.FillPage( _r, _path );
                }

                public IReadOnlyList<ILogEntry> Entries { get { return _entries; } }
                
                /// <summary>
                /// Gets the current path. First entries may be null: they correspond to <see cref="Monitor.FirstDepth"/>: we know
                /// that we are dealing with subordinate entries but we do not have in any of the available files the opening of these groups.
                /// </summary>
                public IReadOnlyList<ILogEntry> CurrentPath { get { return _path.AsReadOnlyList(); } }

                public int PageLength { get { return _pageLength; } }

                public int ForwardPage()
                {
                    if( _r != null )
                    {
                        _entries.Count = 0;
                        if( _r.MoveNext() ) _entries.FillPage( _r, _path );
                    }
                    return Entries.Count;
                }

                public void Dispose()
                {
                    if( _r != null ) _r.Dispose();
                }
            }

            public LivePage ReadFirstPage( LogTimestamp firstLogTime, int pageLength )
            {
                if( pageLength < 1 ) throw new ArgumentOutOfRangeException( "pageLength" );
                MultiFileReader r = new MultiFileReader( firstLogTime, _files );
                if( r.MoveNext() )
                {
                    return new LivePage( _firstDepth, new ILogEntry[pageLength], r, pageLength );
                }
                return new LivePage( _firstDepth, Util.EmptyArray<ILogEntry>.Empty, null, pageLength );
            }
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
