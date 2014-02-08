using CK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    /// <summary>
    /// A log reader acts as an enumerator of <see cref="ILogEntry"/> that are stored in a <see cref="Stream"/>.
    /// </summary>
    public sealed class LogReader : IEnumerator<ILogEntry>
    {
        Stream _stream;
        BinaryReader _binaryReader;
        ILogEntry _current;
        IMulticastLogEntry _currentMulticast;
        int _streamVersion;
        long _currentPosition;
        Exception _readException;
        bool _badEndOfFille;

        /// <summary>
        /// Current version stamp. Writes are done with this version, but reads MUST handle it.
        /// The first released version is 5.
        /// </summary>
        public const int CurrentStreamVersion = 5;

        #if net40

        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on a stream that must start with the version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        public LogReader( Stream stream )
            : this( stream, -1 )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on a stream with an explicit version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="streamVersion">Version of the log stream. Use -1 to read the version if the stream starts with it.</param>
        public LogReader( Stream stream, int streamVersion )
        {
            if( streamVersion < 4 && streamVersion != -1 ) 
                throw new ArgumentException( "Must be -1 or greater or equal to 4 (the first version).", "streamVersion" );
            _stream = stream;
            _binaryReader = new BinaryReader( stream, Encoding.UTF8 );
            _streamVersion = streamVersion;
        }

        #else

        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on a stream that must start with the version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="mustClose">
        /// Defaults to true (the stream will be automatically closed).
        /// False to let the stream opened once this reader is disposed, the end of the log data is reached or an error is encountered.
        /// </param>
        public LogReader( Stream stream, bool mustClose = true )
            : this( stream, -1, mustClose )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on a stream with an explicit version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="streamVersion">Version of the log stream. Use -1 to read the version if the stream starts with it.</param>
        /// <param name="mustClose">
        /// Defaults to true (the stream will be automatically closed).
        /// False to let the stream opened once this reader is disposed, the end of the log data is reached or an error is encountered.
        /// </param>
        public LogReader( Stream stream, int streamVersion, bool mustClose = true )
        {
            if( streamVersion < 5 && streamVersion != -1 ) 
                throw new ArgumentException( "Must be -1 or greater or equal to 5 (the first version).", "streamVersion" );
            _stream = stream;
            _binaryReader = new BinaryReader( stream, Encoding.UTF8, !mustClose );
            _streamVersion = streamVersion;
        }
        #endif
        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a file.
        /// The file will be closed when <see cref="LogReader.Dispose"/> will be called.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        public static LogReader Open( string path )
        {
            return Open( path, -1 );
        }

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a file for which the version is known.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <param name="version">Version of the log data: the file must not start with the version.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        public static LogReader Open( string path, int version )
        {
            return new LogReader( new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.SequentialScan ), version );
        }

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a file starting at the specified offset. 
        /// The file version must known since the header of the file will not be read.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <param name="offset">Offset where the stream position must be initially set.</param>
        /// <param name="version">Version of the log data.</param>
        /// <param name="filter">An optional <see cref="MulticastFilter"/>.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        public static LogReader Open( string path, long offset, int version, MulticastFilter filter = null )
        {
            FileStream s = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.SequentialScan );
            try
            {
                s.Position = offset;
            }
            catch
            {
                s.Dispose();
                throw;
            }
            var r = new LogReader( s, version );
            r.CurrentFilter = filter;
            return r;
        }

        /// <summary>
        /// Enables filtering of a multi-cast stream: only entries from the specified monitor will be read. 
        /// </summary>
        public class MulticastFilter
        {
            /// <summary>
            /// The filtered monitor identifier.
            /// </summary>
            public readonly Guid MonitorId;

            /// <summary>
            /// The offset of the last entry in the stream (see <see cref="LogReader.StreamOffset"/>).
            /// <see cref="Int64.MaxValue"/> when unknown.
            /// </summary>
            public readonly long KnownLastMonitorEntryOffset;

            /// <summary>
            /// Initializes a new <see cref="MulticastFilter"/>.
            /// </summary>
            /// <param name="monitorId">Monitor identifier to filter.</param>
            /// <param name="knownLastMonitorEntryOffset">Offset of the last entry in the stream (when known this enables to stop processing as soon as possible).</param>
            public MulticastFilter( Guid monitorId, long knownLastMonitorEntryOffset = Int64.MaxValue )
            {
                MonitorId = monitorId;
                KnownLastMonitorEntryOffset = knownLastMonitorEntryOffset;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="MulticastFilter"/> that will be taken into account during the next <see cref="MoveNext"/>.
        /// Only entries from this monitor will be extracted when reading a <see cref="IMulticastLogEntry"/> (pure unicast <see cref="ILogEntry"/> will be ignored).
        /// </summary>
        /// <remarks>
        /// Note that the <see cref="Current"/> will be <see cref="ILogEntry"/> objects: multi-cast entry properties (<see cref="IMulticastLogEntry.MonitorId"/> 
        /// and <see cref="IMulticastLogEntry.GroupDepth"/>) are no more available when a filter is set.
        /// </remarks>
        public MulticastFilter CurrentFilter { get; set; }

        /// <summary>
        /// Gets the stream version. It is available only after the first call to <see cref="MoveNext"/>.
        /// </summary>
        public int StreamVersion { get { return _streamVersion; } }

        /// <summary>
        /// Current <see cref="ILogEntry"/> that can be a <see cref="IMulticastLogEntry"/>.
        /// As usual, <see cref="MoveNext"/> must be called before getting the first entry.
        /// </summary>
        public ILogEntry Current
        {
            get
            {
                if( _current == null ) throw new InvalidOperationException();
                return _current;
            }
        }

        /// <summary>
        /// Gets the <see cref="Current"/> entry if the underlying entry is a <see cref="IMulticastLogEntry"/>, null otherwise.
        /// This captures the actual entry when a <see cref="CurrentFilter"/> is set (Current is then a mere Unicast entry).
        /// </summary>
        public IMulticastLogEntry CurrentMulticast
        {
            get
            {
                if( _current == null ) throw new InvalidOperationException();
                return _currentMulticast;
            }
        }

        /// <summary>
        /// Gets the exception that may have been thrown when reading the file.
        /// </summary>
        public Exception ReadException
        {
            get { return _readException; }
        }

        /// <summary>
        /// Gets whether the end of file has been reached and the file is missing the final 0 byte marker.
        /// </summary>
        public bool BadEndOfFileMarker
        {
            get { return _badEndOfFille; }
        }

        /// <summary>
        /// Current <see cref="IMulticastLogEntry"/> with its associated position in the stream.
        /// The current entry must be a multi-cast one and, as usual, <see cref="MoveNext"/> must be called before getting the first entry.
        /// </summary>
        public MulticastLogEntryWithOffset CurrentMulticastWithOffset
        {
            get 
            {                 
                if( _currentMulticast == null ) throw new InvalidOperationException();
                return new MulticastLogEntryWithOffset( _currentMulticast, _currentPosition ); 
            }
        }

        /// <summary>
        /// Gets the inner <see cref="Stream.Position"/> of the <see cref="Current"/> entry.
        /// </summary>
        public long StreamOffset 
        {
            get { return _currentPosition; }
        }

        /// <summary>
        /// Attempts to read the next <see cref="ILogEntry"/>.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public bool MoveNext()
        {
            if( _stream == null ) return false;
            if( _streamVersion == -1 )
            {
                _streamVersion = _binaryReader.ReadInt32();
                if( _streamVersion != CurrentStreamVersion )
                {
                    throw new InvalidOperationException( String.Format( "Stream is not a log stream or its version is not handled (Current Version = {0}).", CurrentStreamVersion ) );
                }
            }
            _currentPosition = _stream.Position;
            ReadNextEntry();
            _currentMulticast = _current as IMulticastLogEntry;
            var f = CurrentFilter;
            if( f != null )
            {
                while( _current != null && (_currentMulticast == null || _currentMulticast.MonitorId != f.MonitorId) )
                {
                    if( _currentPosition > f.KnownLastMonitorEntryOffset )
                    {
                        _current = _currentMulticast = null;
                        break;
                    }
                    ReadNextEntry();
                    _currentMulticast = _current as IMulticastLogEntry;
                }
            }
            return _current != null;
        }

        void ReadNextEntry()
        {
            try
            {
                _current = LogEntry.Read( _binaryReader, _streamVersion, out _badEndOfFille );
            }
            catch( Exception ex )
            {
                _current = null;
                _readException = ex;
            }
        }

        /// <summary>
        /// Close the inner stream (.Net 4.5 only: if this reader has been asked to do so thanks to constructors' parameter mustClose sets to true).
        /// </summary>
        public void Dispose()
        {
            Close( false );
        }

        void Close( bool throwError )
        {
            if( _stream != null )
            {
                _current = null;
                _binaryReader.Dispose();
                _stream = null;
                _binaryReader = null;
            }
            if( throwError ) throw new InvalidOperationException( "Invalid log data." );
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

    }
}
