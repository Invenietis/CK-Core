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
        int _streamVersion;
        int _multiCastCurrentGroupDepth;
        long _currentPosition;

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
            _multiCastCurrentGroupDepth  = -1;
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
            _multiCastCurrentGroupDepth = -1;
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
        /// Current <see cref="ILogEntry"/> with its associated position in the stream.
        /// As usual, <see cref="MoveNext"/> must be called before getting the first entry.
        /// </summary>
        public LogEntryWithOffset CurrentWithOffset
        {
            get { return new LogEntryWithOffset( Current, _currentPosition ); }
        }

        /// <summary>
        /// Gets the group depth of the <see cref="Current"/> entry if the underlying entry is a <see cref="IMulticastLogEntry"/>, -1 otherwise.
        /// This captures the group depth when a <see cref="CurrentFilter"/> is set (Current is then a mere Unicast entry).
        /// </summary>
        public int MultiCastCurrentGroupDepth 
        { 
            get { return _multiCastCurrentGroupDepth; } 
        }

        /// <summary>
        /// Gets whether the current entry was, in the stream, a <see cref="IMulticastLogEntry"/>.
        /// </summary>
        public bool IsCurrentUnderlyingEntryMulticast
        { 
            get { return _multiCastCurrentGroupDepth >= 0; } 
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
            // The API is designed for performance: we can here skip unicast entries and multi-cast entries from 
            // other monitors when a CurrentFilter is set. To efficiently skip data, we need a:
            //
            // _current = LogEntry.ReadNextFiltered( _binaryReader, _streamVersion, CurrentFilter, out _multiCastCurrentGroupDepth );
            //
            // This method should use an enhanced binary reader that should be able to skip strings and other serialized objects 
            // as much as possible.
            // It will return a mere ILogEntry and not a multi-cast one.
            // 
            // For the moment, I use CreateUnicastLogEntry to remove the Guid and the group depth from memory.
            // For better performance, what is described above should be implemented once...
            //
            _current = LogEntry.Read( _binaryReader, _streamVersion );
            IMulticastLogEntry m = _current as IMulticastLogEntry;
            var f = CurrentFilter;
            if( f != null )
            {
                while( _current != null && (m == null || m.MonitorId != f.MonitorId) )
                {
                    if( _currentPosition > f.KnownLastMonitorEntryOffset )
                    {
                        m = null;
                        break;
                    }
                    _current = LogEntry.Read( _binaryReader, _streamVersion );
                }
                _current = m == null ? null : m.CreateUnicastLogEntry();
            }
            _multiCastCurrentGroupDepth = m != null ? m.GroupDepth : -1;
            return _current != null;
        }

        /// <summary>
        /// Replays mono activity. Multi-cast entries (<see cref="IMulticastLogEntry"/>) are ignored.
        /// </summary>
        /// <param name="destination">Target <see cref="IActivityMonitor"/>.</param>
        public void ReplayUnicast( IActivityMonitor destination )
        {
            if( destination == null ) throw new ArgumentNullException( "destinations" );
            while( this.MoveNext() )
            {
                var log = this.Current;
                if( !(log is IMulticastLogEntry) )
                {
                    switch( log.LogType )
                    {
                        case LogEntryType.Line:
                            destination.UnfilteredLog( log.Tags, log.LogLevel, log.Text, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                            break;
                        case LogEntryType.OpenGroup:
                            destination.UnfilteredOpenGroup( log.Tags, log.LogLevel, null, log.Text, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                            break;
                        case LogEntryType.CloseGroup:
                            destination.CloseGroup( log.LogTime, log.Conclusions );
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Replays multiple activities. Unicast entries (<see cref="ILogEntry"/> that are not <see cref="IMulticastLogEntry"/>) are ignored.
        /// </summary>
        /// <param name="destinations">
        /// Must provide a <see cref="IActivityMonitor"/> for each identifier. 
        /// The <see cref="IMulticastLogEntry.GroupDepth"/> is provided for each entry.
        /// </param>
        public void ReplayMulticast( Func<Guid,int,IActivityMonitor> destinations )
        {
            if( destinations == null ) throw new ArgumentNullException( "destinations" );
            while( this.MoveNext() )
            {
                var log = this.Current as IMulticastLogEntry;
                if( log != null )
                {
                    IActivityMonitor d = destinations( log.MonitorId, log.GroupDepth );
                    if( d != null )
                    {
                        switch( this.Current.LogType )
                        {
                            case LogEntryType.Line:
                                d.UnfilteredLog( log.Tags, log.LogLevel, log.Text, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                                break;
                            case LogEntryType.OpenGroup:
                                d.UnfilteredOpenGroup( log.Tags, log.LogLevel, null, log.Text, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                                break;
                            case LogEntryType.CloseGroup:
                                d.CloseGroup( log.LogTime, log.Conclusions );
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Close the inner stream if this reader has been asked to do so (thanks to constructors' parameter mustClose sets to true).
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
