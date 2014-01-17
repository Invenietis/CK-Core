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
            readonly DateTimeStamp _firstEntryTime;
            readonly int _firstDepth;
            readonly DateTimeStamp _lastEntryTime;
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

            public DateTimeStamp FirstEntryTime { get { return _firstEntryTime; } }

            public int FirstDepth { get { return _firstDepth; } }

            public DateTimeStamp LastEntryTime { get { return _lastEntryTime; } }

            public int LastDepth { get { return _lastDepth; } }

            internal class MultiFileReader : IDisposable
            {
                readonly DateTimeStamp _firstLogTime;
                readonly IReadOnlyList<RawLogFileMonitorOccurence> _files;
                CKSortedArrayList<OneLogReader> _readers;

                public MultiFileReader( DateTimeStamp firstLogTime, IReadOnlyList<RawLogFileMonitorOccurence> files )
                {
                    _firstLogTime = firstLogTime;
                    _files = files;
                }

                public IMulticastLogEntry Current { get { return _readers[0].Head.Entry; } }

                class OneLogReader : IDisposable
                {
                    public MulticastLogEntryWithOffset Head;
                    LogReader _reader;
                    public readonly RawLogFileMonitorOccurence File;
                    public readonly int FirstGroupDepth;

                    public OneLogReader( RawLogFileMonitorOccurence file, DateTimeStamp firstLogTime )
                    {
                        File = file;
                        _reader = file.CreateFilteredReaderAndMoveTo( firstLogTime );
                        FirstGroupDepth = _reader.CurrentMulticast.GroupDepth;
                        Head = _reader.CurrentMulticastWithOffset;
                    }

                    public OneLogReader( RawLogFileMonitorOccurence file, long offset )
                    {
                        _reader = file.CreateFilteredReader( offset );
                        _reader.MoveNext();
                        FirstGroupDepth = _reader.CurrentMulticast.GroupDepth;
                        Head = _reader.CurrentMulticastWithOffset;
                    }

                    public bool Forward()
                    {
                        Debug.Assert( _reader != null );
                        if( _reader.MoveNext() )
                        {
                            Head = _reader.CurrentMulticastWithOffset;
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

            /// <summary>
            /// A page gives access to <see cref="Entries"/> and <see cref="CurrentPath"/> by unifying
            /// all the raw log files and removing duplicates from them.
            /// Pages are sequentially accessed from a first page (obtained by <see cref="Monitor.ReadFirstPage"/>) and the by calling <see cref="ForwardPage"/>.
            /// </summary>
            public class LivePage : IDisposable
            {
                class WrappedList : IReadOnlyList<ParentedLogEntry>
                {
                    public readonly ParentedLogEntry[] Entries;

                    public WrappedList( ParentedLogEntry[] entries )
                    {
                        Entries = entries;
                    }

                    public ParentedLogEntry this[int index]
                    {
                        get
                        {
                            if( index >= Count ) throw new ArgumentOutOfRangeException();
                            return Entries[index];
                        }
                    }

                    public int Count { get; set; }

                    public IEnumerator<ParentedLogEntry> GetEnumerator()
                    {
                        return Entries.Take( Count ).GetEnumerator();
                    }

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                    {
                        return GetEnumerator();
                    }

                    internal void FillPage( MultiFileReader r, List<ParentedLogEntry> path )
                    {
                        ILogEntry lastPrevEntry = Count > 0 ? Entries[Count - 1].Entry : null;
                        Count = DoFillPage( r, path, lastPrevEntry );
                    }

                    int DoFillPage( MultiFileReader r, List<ParentedLogEntry> path, ILogEntry lastPrevEntry )
                    {
                        ParentedLogEntry parent = path.Count > 0 ? path[path.Count - 1] : null;
                        int i = 0;
                        do
                        {
                            var entry = r.Current;
                            if( entry.GroupDepth < path.Count )
                            {
                                // Adds a MissingCloseGroup with an unknown time for tail groups: handles the 
                                // last closing group specifically.
                                while( entry.GroupDepth < path.Count-1 )
                                {
                                    if( AppendEntry( path, ref parent, ref i, LogEntry.CreateMissingCloseGroup( DateTimeStamp.Unknown ) ) ) return i;
                                }
                                // Handles the last auto-close group: we may know its time thanks to our current entry (if its previous type is a CloseGroup).
                                Debug.Assert( entry.GroupDepth == path.Count-1, "We are on the last group to auto-close." );
                                DateTimeStamp prevTime = entry.PreviousEntryType == LogEntryType.CloseGroup ? entry.PreviousLogTime : DateTimeStamp.Unknown;
                                if( AppendEntry( path, ref parent, ref i, LogEntry.CreateMissingCloseGroup( prevTime ) ) ) return i;
                            }
                            else if( entry.GroupDepth > path.Count )
                            {
                                // Adds a MissingOpenGroup with an unknown time for head groups: handles the 
                                // last opening group specifically.
                                while( entry.GroupDepth > path.Count + 1 )
                                {
                                    if( AppendEntry( path, ref parent, ref i, LogEntry.CreateMissingOpenGroup( DateTimeStamp.Unknown ) ) ) return i;
                                }
                                // Handles the last auto-open group: we may know its time thanks to our current entry (if its previous type is a OpenGroup).
                                Debug.Assert( entry.GroupDepth == path.Count+1, "We are on the last group to auto-open." );
                                DateTimeStamp prevTime = entry.PreviousEntryType == LogEntryType.OpenGroup ? entry.PreviousLogTime : DateTimeStamp.Unknown;
                                if( AppendEntry( path, ref parent, ref i, LogEntry.CreateMissingOpenGroup( prevTime ) ) ) return i;
                            }
                            // If we know the the time and type of the previous entry and this does not correspond to 
                            // our predecessor, we inject a missing line.
                            // This is necessarily a line that we inject here thanks to the open/close adjustment above.
                            // If the log type of the known previous entry is Open or Close group, it means that there are incoherent group depths... and 
                            // we ignore this pathological case.
                            if( entry.PreviousEntryType != LogEntryType.None )
                            {
                                ILogEntry prevEntry = i > 0 ? Entries[i-1].Entry : lastPrevEntry;
                                if( prevEntry == null || prevEntry.LogTime != entry.PreviousLogTime )
                                {
                                    if( AppendEntry( path, ref parent, ref i, LogEntry.CreateMissingLine( entry.PreviousLogTime ) ) ) return i;
                                }
                            }
                            // Now that missing data has been handled, appends the line itself.
                            if( AppendEntry( path, ref parent, ref i, entry.CreateUnicastLogEntry() ) ) return i;
                        }
                        while( r.MoveNext() );
                        return i;
                    }

                    bool AppendEntry( List<ParentedLogEntry> path, ref ParentedLogEntry parent, ref int i, ILogEntry e )
                    {
                        Debug.Assert( e.LogType != LogEntryType.None );
                        if( e.LogType == LogEntryType.CloseGroup )
                        {
                            // Take no risk here: ignores the case where a close occurs while we are already 
                            // at the root. This SHOULD never happen unless there is a mismatch between FirstInitialGroupDepth
                            // and actual log file content.
                            if( path.Count > 0 )
                            {
                                Entries[i++] = new ParentedLogEntry( parent, e );
                                path.RemoveAt( path.Count - 1 );
                                if( i == Entries.Length ) return true;
                                parent = path.Count > 0 ? path[path.Count - 1] : null;
                            }
                            return false;
                        }
                        var pE = new ParentedLogEntry( parent, e );
                        Entries[i++] = pE;
                        if( e.LogType == LogEntryType.OpenGroup )
                        {
                            path.Add( pE );
                            parent = pE;
                        }
                        return i == Entries.Length;
                    }
                }

                readonly WrappedList _entries;
                readonly MultiFileReader _r;
                readonly int _pageLength;
                readonly List<ParentedLogEntry> _currentPath;

                internal LivePage( int initialGroupDepth, ParentedLogEntry[] entries, MultiFileReader r, int pageLength )
                {
                    Debug.Assert( pageLength == entries.Length || entries.Length == 0 );
                    _r = r;
                    _pageLength = pageLength;
                    _currentPath = new List<ParentedLogEntry>();
                    ParentedLogEntry e = null;
                    for( int i = 0; i < initialGroupDepth; ++i ) 
                    {
                        ParentedLogEntry g = new ParentedLogEntry( e, LogEntry.CreateMissingOpenGroup( DateTimeStamp.Unknown ) ); 
                        _currentPath.Add( g );
                        e = g;
                    }
                    _entries = new WrappedList( entries );
                    if( _r != null ) _entries.FillPage( _r, _currentPath );
                }

                /// <summary>
                /// Gets the log entries of the current page.
                /// </summary>
                public IReadOnlyList<ParentedLogEntry> Entries { get { return _entries; } }
                
                /// <summary>
                /// Gets the page length. 
                /// </summary>
                public int PageLength { get { return _pageLength; } }

                /// <summary>
                /// Loads the next page.
                /// </summary>
                /// <returns>The number of entries.</returns>
                public int ForwardPage()
                {
                    if( _r != null )
                    {
                        if( _r.MoveNext() )
                        {
                            _entries.FillPage( _r, _currentPath );
                        }
                        else
                        {
                            _entries.Count = 0;
                        }
                    }
                    return Entries.Count;
                }

                /// <summary>
                /// Closes all resources.
                /// </summary>
                public void Dispose()
                {
                    if( _r != null ) _r.Dispose();
                }
            }

            /// <summary>
            /// Loads the first available entries starting at a given time.
            /// </summary>
            /// <param name="firstLogTime">The first log time.</param>
            /// <param name="pageLength">The length of pages. Must be greater than 0.</param>
            /// <returns>The first <see cref="LivePage"/> from which next pages can be retrieved.</returns>
            public LivePage ReadFirstPage( DateTimeStamp firstLogTime, int pageLength )
            {
                if( pageLength < 1 ) throw new ArgumentOutOfRangeException( "pageLength" );
                MultiFileReader r = new MultiFileReader( firstLogTime, _files );
                if( r.MoveNext() )
                {
                    return new LivePage( _firstDepth, new ParentedLogEntry[pageLength], r, pageLength );
                }
                return new LivePage( _firstDepth, Util.EmptyArray<ParentedLogEntry>.Empty, null, pageLength );
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
