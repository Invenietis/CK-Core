using CK.Core;
using CK.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CK.Monitoring
{
    /// <summary>
    /// A log reader acts as an enumerator of <see cref="ILogEntry"/> that are stored in a <see cref="Stream"/>.
    /// </summary>
    public sealed class LogReader : IEnumerator<ILogEntry>
    {
        Stream _stream;
        CKBinaryReader _binaryReader;
        readonly int _streamVersion;
        readonly int _headerLength;
        ILogEntry _current;
        IMulticastLogEntry _currentMulticast;
        long _currentPosition;
        Exception _readException;
        bool _badEndOfFille;

        /// <summary>
        /// Current version stamp. Writes are done with this version, but reads MUST handle it.
        /// The first released version is 5.
        /// </summary>
        public const int CurrentStreamVersion = 6;

        /// <summary>
        /// The file header for .ckmon files starting from CurrentStreamVersion = 5.
        /// That's C, K, M, O and N (ASCII).
        /// </summary>
        public static readonly byte[] FileHeader = new byte[] { 0x43, 0x4b, 0x4d, 0x4f, 0x4e };

        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on an uncompressed stream with an explicit version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="streamVersion">Version of the log stream.</param>
        /// <param name="headerLength">Length of the header. This will be substracted to the actual stream position to compute the <see cref="StreamOffset"/>.</param>
        /// <param name="mustClose">
        /// Defaults to true (the stream will be automatically closed).
        /// False to let the stream opened once this reader is disposed, the end of the log data is reached or an error is encountered.
        /// </param>
        public LogReader( Stream stream, int streamVersion, int headerLength, bool mustClose = true )
        {
            if( streamVersion < 5 )
                throw new ArgumentException( "Must be greater or equal to 5 (the first version).", "streamVersion" );
            _stream = stream;
            _binaryReader = new CKBinaryReader( stream, Encoding.UTF8, !mustClose );
            _streamVersion = streamVersion;
            _headerLength = headerLength;
        }

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a compressed or uncompressed file.
        /// The file will be closed when <see cref="LogReader.Dispose"/> will be called.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <param name="dataOffset">
        /// An optional offset where the stream position must be initially set: this is the position of an entry in the actual (potentially uncompressed stream),
        /// not the offset in the original stream.
        /// </param>
        /// <param name="filter">An optional <see cref="MulticastFilter"/>.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        /// <remarks>
        /// .ckmon files exist in different file versions, depending on headers.
        /// The file can be compressed using GZipStream, in which case the header will be the magic GZIP header: 1F 8B.
        /// New header (applies to version 5), the file will start with 43 4B 4D 4F 4E (CKMON in ASCII), followed by the version number, instead of only the version number.
        /// </remarks>
        public static LogReader Open( string path, long dataOffset = 0, MulticastFilter filter = null )
        {
            if( path == null ) throw new ArgumentNullException( "path" );
            FileStream fs = null;
            try
            {
                fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan );
                return Open( fs, dataOffset, filter );
            }
            catch
            {
                if( fs != null ) fs.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a compressed or uncompressed stream.
        /// The stream will be closed when <see cref="LogReader.Dispose"/> will be called.
        /// </summary>
        /// <param name="seekableStream">Stream that must support Seek operations (<see cref="Stream.CanSeek"/> must be true).</param>
        /// <param name="dataOffset">
        /// An optional offset where the stream position must be initially set: this is the position of an entry in the actual (potentially uncompressed stream),
        /// not the offset in the original stream.
        /// </param>
        /// <param name="filter">An optional <see cref="MulticastFilter"/>.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        /// <remarks>
        /// .ckmon files exist in different file versions, depending on headers.
        /// The file can be compressed using GZipStream, in which case the header will be the magic GZIP header: 1F 8B.
        /// New header (applies to version 5), the file will start with 43 4B 4D 4F 4E (CKMON in ASCII), followed by the version number, instead of only the version number.
        /// </remarks>
        public static LogReader Open( Stream seekableStream, long dataOffset = 0, MulticastFilter filter = null )
        {
            if( seekableStream == null ) throw new ArgumentNullException( "seekableStream" );
            if( !seekableStream.CanSeek ) throw new ArgumentException( "Stream must support seek operations.", "seekableStream" );
            LogReaderStreamInfo i = LogReaderStreamInfo.OpenStream( seekableStream );
            var s = i.LogStream;
            if( dataOffset > 0 )
            {
                if( s.CanSeek )
                {
                    s.Seek( dataOffset, SeekOrigin.Current );
                }
                else
                {
                    var buffer = new byte[8192];
                    int toRead;
                    while( (toRead = (int)Math.Min( 8192, dataOffset )) > 0 && s.Read( buffer, 0, toRead ) == toRead )
                    {
                        dataOffset -= toRead;
                    }
                }
            }
            var r = new LogReader( s, i.Version, i.HeaderLength );
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
        /// Note that the <see cref="Current"/> will be <see cref="ILogEntry"/> objects: multi-cast entry properties (<see cref="IMulticastLogInfo.MonitorId"/> 
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
            if( _streamVersion < 5 || _streamVersion > CurrentStreamVersion )
            {
                throw new InvalidOperationException( String.Format( "Stream is not a log stream or its version is not handled (Current Version = {0}).", CurrentStreamVersion ) );
            }
            _currentPosition = _stream.Position - _headerLength;
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
