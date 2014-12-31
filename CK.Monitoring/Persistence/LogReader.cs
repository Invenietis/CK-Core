#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\LogReader.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
using System.IO.Compression;
using System.Diagnostics;

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

        /// <summary>
        /// The file header for .ckmon files starting from CurrentStreamVersion = 5.
        /// That's C, K, M, O and N (ASCII).
        /// </summary>
        public static readonly byte[] FileHeader = new byte[] { 0x43, 0x4b, 0x4d, 0x4f, 0x4e };

        /// <summary>
        /// The file header for gzipped .ckmon files.
        /// </summary>
        public static readonly byte[] GzipFileHeader = new byte[] { 0x1f, 0x8b };

#if net40
        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on an uncompressed stream with an explicit version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="streamVersion">Version of the log stream. Use -1 to read the version if the stream starts with it.</param>
        public LogReader( Stream stream, int streamVersion )
        {
            if( streamVersion < 4 ) 
                throw new ArgumentException( "Must be greater or equal to 4 (the first version).", "streamVersion" );
            _stream = stream;
            _binaryReader = new BinaryReader( stream, Encoding.UTF8 );
            _streamVersion = streamVersion;
        }
#else
        /// <summary>
        /// Initializes a new <see cref="LogReader"/> on an uncompressed stream with an explicit version number.
        /// </summary>
        /// <param name="stream">Stream to read logs from.</param>
        /// <param name="streamVersion">Version of the log stream. Use -1 to read the version if the stream starts with it.</param>
        /// <param name="mustClose">
        /// Defaults to true (the stream will be automatically closed).
        /// False to let the stream opened once this reader is disposed, the end of the log data is reached or an error is encountered.
        /// </param>
        public LogReader( Stream stream, int streamVersion, bool mustClose = true )
        {
            if( streamVersion < 5 )
                throw new ArgumentException( "Must be greater or equal to 5 (the first version).", "streamVersion" );
            _stream = stream;
            _binaryReader = new BinaryReader( stream, Encoding.UTF8, !mustClose );
            _streamVersion = streamVersion;
        }
#endif

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of a compressed or uncompressed file.
        /// The file will be closed when <see cref="LogReader.Dispose"/> will be called.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        public static LogReader Open( string path )
        {
            /**
             * .ckmon files exist in different file versions, depending on headers.
             * The file can be compressed using GZipStream, in which case the header will be set accordingly to 1F 8B.
             * Starting from version 6, the file will start with 43 4B 4D 4F 4E, followed by the version number, instead of only the version number.
             * */

            FileStream fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.SequentialScan );

            LogStreamInfo i = OpenLogStream( fs );

            return new LogReader( i.LogStream, i.KnownVersion );
        }

        /// <summary>
        /// Creates a log stream while trying to guess version and compression.
        /// Supports headers:
        /// - Old version header (05 00 00 00)
        /// - CKMOD ID header (43 4B 4D 4F 4E) followed by version
        /// - RFC 1952 GZIP header (1F 8B), will recurse with a decompressed stream
        /// </summary>
        /// <param name="s">Log stream</param>
        /// <param name="isInsideGzip">Whether the GZIP header has been already parsed and the given stream is decompressed. If true, gzip headers will not be checked.</param>
        /// <returns>LogReader, set before the first log entry.</returns>
        static LogStreamInfo OpenLogStream( Stream s, bool isInsideGzip = false )
        {
            // Find if uncompressed byte is 05 (raw version) or 43 (log header version)
            int firstByte = s.ReadByte(); // ReadByte casts to int to handle EOF (-1).

            if( !isInsideGzip && firstByte == 0x1f )
            {
                // GZIP Magic number is 0x1f8b.
                int secondByte = s.ReadByte(); // Read 2:8B
                if( secondByte == 0x8b )
                {
                    GZipStream gzip = new GZipStream( s, CompressionMode.Decompress );
                    return OpenLogStream( gzip, true );
                }
                else
                {
                    throw new InvalidDataException( "Invalid compression header." );
                }
            }
            else if( firstByte == 0x43 )
            {
                // CKMON header is 43 4B 4D 4F 4E. That's C, K, M, O and N (ASCII).
                int b = s.ReadByte(); // 2:4B
                if( b != 0x4b ) { throw new InvalidDataException( "Invalid header." ); }
                b = s.ReadByte(); // 3:4D
                if( b != 0x4d ) { throw new InvalidDataException( "Invalid header." ); }
                b = s.ReadByte(); // 4:4F
                if( b != 0x4f ) { throw new InvalidDataException( "Invalid header." ); }
                b = s.ReadByte(); // 5:4E
                if( b != 0x4e ) { throw new InvalidDataException( "Invalid header." ); }

                // Read version number. Note: We don't have a BinaryReader on hand.
                byte[] versionBytes = new byte[4];
                s.Read( versionBytes, 0, 4 );
                int version = BitConverter.ToInt32( versionBytes, 0 );

                return LogStreamInfo.From( s, isInsideGzip, version );
            }
            else if( firstByte <= 0x05 ) // Old versions without CKMON header
            {
                // BinaryWriter/Reader always uses little endian, so (int)5 will be 05 00 00 00 instead of 00 00 00 05.
                // See http://msdn.microsoft.com/en-us/library/24e33k1w%28v=vs.110%29.aspx

                // Read version number. Note: We don't have a BinaryReader on hand.
                byte[] versionBytes = new byte[4];
                versionBytes[0] = (byte)firstByte;
                s.Read( versionBytes, 1, 3 );
                int version = BitConverter.ToInt32( versionBytes, 0 );

                if( version <= 0 || version > 5 )
                {
                    throw new InvalidDataException( String.Format( "Invalid uncompressed version header ({0}).", version ) );
                }

                return LogStreamInfo.From( s, isInsideGzip, version );
            }
            throw new InvalidDataException( String.Format( "Invalid CKMON header. Expected byte header to be 0x1f, 0x43 or <= 0x05, but got 0x{0:x2}.", firstByte ) );
        }

        /// <summary>
        /// Opens a <see cref="LogReader"/> to read the content of an compressed or uncompressed file starting at the specified offset. 
        /// The file version must known since the header of the file will not be read.
        /// </summary>
        /// <param name="path">Path of the log file.</param>
        /// <param name="offset">Offset where the stream position must be initially set.</param>
        /// <param name="version">Version of the log data.</param>
        /// <param name="filter">An optional <see cref="MulticastFilter"/>.</param>
        /// <returns>A <see cref="LogReader"/> that will close the file when disposed.</returns>
        public static LogReader Open( string path, long offset, int version, MulticastFilter filter = null )
        {
            FileStream fileStream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.SequentialScan );

            LogStreamInfo info = OpenLogStream( fileStream );
            Stream s = info.LogStream;

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
            if( _streamVersion != CurrentStreamVersion )
            {
                throw new InvalidOperationException( String.Format( "Stream is not a log stream or its version is not handled (Current Version = {0}).", CurrentStreamVersion ) );
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

    /// <summary>
    /// Container for an uncompressed log stream and other meta-information.
    /// </summary>
    struct LogStreamInfo
    {
        /// <summary>
        /// Gets the log stream.
        /// </summary>
        /// <value>
        /// The open, uncompressed log stream. Its position is before the version number if Version is equal to -1; otherwise, it is after.
        /// </value>
        internal Stream LogStream { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the log stream is contained in a compressed stream.
        /// </summary>
        /// <value>
        /// <c>true</c> if the log stream is contained in a compressed stream; otherwise, <c>false</c>.
        /// </value>
        internal bool IsCompressed { get; private set; }

        /// <summary>
        /// Gets the known version.
        /// </summary>
        /// <value>
        /// The known version.
        /// </value>
        internal int KnownVersion { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogStreamInfo"/> struct.
        /// </summary>
        /// <param name="s">The open, uncompressed log stream..</param>
        /// <param name="isCompressed">Tf set to <c>true</c>, indicates that the log stream is contained in a compressed stream.</param>
        /// <param name="knownVersion">The known version. If -1, indicated that the version is unknown and appears after the current position in the stream.</param>
        internal static LogStreamInfo From( Stream s, bool isCompressed = false, int knownVersion = -1 )
        {
            return new LogStreamInfo()
            {
                LogStream = s,
                IsCompressed = isCompressed,
                KnownVersion = knownVersion
            };
        }
    }
}
