using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    /// <summary>
    /// Container for an uncompressed log stream and other meta-information.
    /// </summary>
    internal struct LogReaderStreamInfo
    {
        /// <summary>
        /// Gets the log stream: it is opened and uncompressed. Its position is before the version number if Version is equal to -1; otherwise, it is after.
        /// </summary>
        /// <value>
        /// </value>
        public readonly Stream LogStream;

        /// <summary>
        /// Gets a value indicating whether the log stream is contained in a compressed stream.
        /// <c>true</c> if the log stream is contained in a compressed stream; otherwise, <c>false</c>.
        /// </summary>
        public readonly bool IsCompressed;

        /// <summary>
        /// The version.
        /// </summary>
        public readonly int Version;

        /// <summary>
        /// The length of the version header.
        /// </summary>
        public readonly int HeaderLength;

        LogReaderStreamInfo( Stream s, bool c, int v, int hLen )
        {
            LogStream = s;
            IsCompressed = c;
            Version = v;
            HeaderLength = hLen;
        }

        /// <summary>
        /// Opens a log stream, handling version and compression.
        /// On success, its Stream positionned after the header (before the first log entry).
        /// Supports headers:
        /// - Old version header (05 00 00 00)
        /// - CKMOD header (43 4B 4D 4F 4E) followed by version
        /// - RFC 1952 GZIP header (1F 8B), will be handled automatically by: a GZip stream reader will be created.
        /// </summary>
        /// <param name="seekableStream">Log stream to read.</param>
        /// <returns>LogReader information.</returns>
        public static LogReaderStreamInfo OpenStream( Stream seekableStream )
        {
            return DoOpenLogStream( seekableStream, false );
        }

        static LogReaderStreamInfo DoOpenLogStream( Stream s, bool isInsideGzip  )
        {
            // Find if uncompressed byte is 05 (raw version) or 43 (log header version)
            int firstByte = s.ReadByte(); // ReadByte casts to int to handle EOF (-1).

            if( !isInsideGzip && firstByte == 0x1f )
            {
                // GZIP Magic number is 0x1f8b.
                if( s.ReadByte() == 0x8b )
                {
                    s.Seek( -2, SeekOrigin.Current );
                    return DoOpenLogStream( new GZipStreamReader( s ), true );
                }
                throw new InvalidDataException( "Invalid compression header." );
            }
            if( firstByte == 0x43 )
            {
                // CKMON header is 43 4B 4D 4F 4E. That's C, K, M, O and N (ASCII).
                int b = s.ReadByte(); // 2:4B
                if( b != 0x4b ) throw new InvalidDataException( "Invalid header." );
                b = s.ReadByte(); // 3:4D
                if( b != 0x4d ) throw new InvalidDataException( "Invalid header." );
                b = s.ReadByte(); // 4:4F
                if( b != 0x4f ) throw new InvalidDataException( "Invalid header." );
                b = s.ReadByte(); // 5:4E
                if( b != 0x4e ) throw new InvalidDataException( "Invalid header." );

                // Read version number. Note: We don't have a BinaryReader on hand.
                byte[] versionBytes = new byte[4];
                if( s.Read( versionBytes, 0, 4 ) < 4 ) throw new InvalidDataException( "Invalid header." );
                int version = BitConverter.ToInt32( versionBytes, 0 );
                return new LogReaderStreamInfo( s, isInsideGzip, version, 9 );
            }
            if( firstByte <= 0x05 ) // Old versions without CKMON header
            {
                // BinaryWriter/Reader always uses little endian, so (int)5 will be 05 00 00 00 instead of 00 00 00 05.
                // See http://msdn.microsoft.com/en-us/library/24e33k1w%28v=vs.110%29.aspx

                // Read version number. Note: We don't have a BinaryReader on hand.
                byte[] versionBytes = new byte[4];
                versionBytes[0] = (byte)firstByte;
                int version = 0;
                if( s.Read( versionBytes, 1, 3 ) < 3
                    || (version = BitConverter.ToInt32( versionBytes, 0 )) <= 0
                    || version > 5 )
                {
                    throw new InvalidDataException( String.Format( "Invalid uncompressed version header ({0}).", version ) );
                }
                return new LogReaderStreamInfo( s, isInsideGzip, version, 4 );
            }
            throw new InvalidDataException( String.Format( "Invalid CKMON header. Expected byte header to be 0x1f, 0x43 or <= 0x05, but got 0x{0:x2}.", firstByte ) );
        }


    }
}
