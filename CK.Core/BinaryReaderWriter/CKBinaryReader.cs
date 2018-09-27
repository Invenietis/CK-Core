using CK.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Specializes <see cref="BinaryReader"/> to expose helpers.
    /// </summary>
    public class CKBinaryReader : BinaryReader, ICKBinaryReader
    {
        /// <summary>
        /// Implements a simple object pool that handles objects written through <see cref="CKBinaryWriter.ObjectPool{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        public class ObjectPool<T>
        {
            readonly List<T> _objects;
            readonly ICKBinaryReader _r;

            /// <summary>
            /// Initializes a new object pool reader.
            /// </summary>
            /// <param name="r">The reader. Must not be null.</param>
            public ObjectPool( ICKBinaryReader r )
            {
                if( r == null ) throw new ArgumentNullException( nameof( r ) );
                _objects = new List<T>();
                _r = r;
            }

            /// <summary>
            /// Capture the <see cref="TryRead(out T)"/> result.
            /// When <see cref="Success"/> is false, <see cref="SetReadResult(T)"/> must be called
            /// as soon as the object has been read.
            /// </summary>
            public struct ReadState
            {
                ObjectPool<T> _pool;
                int _num;
                byte _writeMarker;

                /// <summary>
                /// Gets whether the object has been read.
                /// When false, <see cref="SetReadResult(T)"/> must be called as soon as the object has
                /// been read.
                /// </summary>
                public bool Success => _pool == null;

                /// <summary>
                /// Gets the write marker. It is 0 when the read value is the default value of
                /// <typeparamref name="T"/>, 1 when the value was already read (<see cref="Success"/> is true),
                /// and 2 (or more) if the value must actually been read from the stream.
                /// See <see cref="CKBinaryWriter.ObjectPool{T}.MustWrite(T, byte)"/>.
                /// </summary>
                public byte WriteMarker => _writeMarker;

                /// <summary>
                /// Sets the value read.
                /// This must be called only when <see cref="Success"/> is false otherwise
                /// an <see cref="InvalidOperationException"/> is thrown.
                /// </summary>
                /// <param name="read">The read value.</param>
                /// <returns>The read value.</returns>
                public T SetReadResult( T read )
                {
                    if( Success ) throw new InvalidOperationException();
                    _pool._objects[_num] = read;
                    return read;
                }

                internal ReadState( ObjectPool<T> p, byte marker )
                {
                    _pool = p;
                    _num = p._objects.Count;
                    _writeMarker = marker;
                    p._objects.Add( default( T ) );
                }

                internal ReadState( byte marker )
                {
                    _pool = null;
                    _num = 0;
                    _writeMarker = marker;
                }
            }

            /// <summary>
            /// Tries to read a previously written object.
            /// </summary>
            /// <param name="already">The available object if it exists.</param>
            /// <returns>
            /// The read state. When <see cref="ReadState.Success"/> is false, the object must be read
            /// and <see cref="ReadState.SetReadResult(T)"/> must be called.
            /// </returns>
            public ReadState TryRead( out T already )
            {
                byte b = _r.ReadByte();
                switch( b )
                {
                    case 0: already = default( T ); return new ReadState();
                    case 1: already = _objects[_r.ReadNonNegativeSmallInt32()]; return new ReadState( 1 );
                    default:already = default( T ); return new ReadState( this, b );
                }
            }

            /// <summary>
            /// Reads a value either from this pool or, when not yet read, thanks to an
            /// actual reader function.
            /// </summary>
            /// <param name="actualReader">Function that will be called if the value must actually be read.</param>
            /// <returns>The value.</returns>
            public T Read( Func<ReadState,ICKBinaryReader,T> actualReader )
            {
                byte b = _r.ReadByte();
                switch( b )
                {
                    case 0: return default( T );
                    case 1: return _objects[_r.ReadNonNegativeSmallInt32()];
                    default:
                        {
                            var s = new ReadState( this, b );
                            return s.SetReadResult( actualReader( s, _r ) );
                        }
                }
            }

        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryReader"/> based on the specified
        /// stream and using UTF-8 encoding.
        /// The stream will be closed once this reader is disposed.
        /// </summary>
        /// <param name="input">The input stream.</param>
        public CKBinaryReader( Stream input )
            : this( input, Encoding.UTF8, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryReader"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public CKBinaryReader( Stream input, Encoding encoding )
            : this( input, encoding, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryReader"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after this reader object is disposed; otherwise, false.</param>
        public CKBinaryReader( Stream input, Encoding encoding, bool leaveOpen )
            : base( input, encoding, leaveOpen )
        {
            StringPool = new ObjectPool<string>( this );
        }

        /// <summary>
        /// Gets the string pool (see <see cref="CKBinaryWriter.StringPool"/>).
        /// </summary>
        public ObjectPool<string> StringPool { get; }

        /// <summary>
        /// Reads in a 32-bit integer in compressed format.
        /// </summary>
        /// <returns>A 32-bit integer</returns>
        public int ReadNonNegativeSmallInt32() => Read7BitEncodedInt();

        /// <summary>
        /// Reads in a 32-bit integer in compressed format written by <see cref="CKBinaryWriter.WriteSmallInt32(int, int)"/>.
        /// </summary>
        /// <param name="minNegativeValue">The same negative value used to write the integer.</param>
        /// <returns>A 32-bit integer</returns>
        public int ReadSmallInt32( int minNegativeValue = -1 ) => Read7BitEncodedInt() + minNegativeValue;

        /// <summary>
        /// Reads and normalizes a string according to <see cref="Environment.NewLine"/>.
        /// The fact that the data has actually be saved with LF or CRLF must be known.
        /// </summary>
        /// <param name="streamIsCRLF">True if the <see cref="BinaryReader.BaseStream"/> contains 
        /// strings with CRLF end-of-line, false if the end-of-line is LF only.</param>
        /// <returns>String with actual <see cref="Environment.NewLine"/> for end-of-line.</returns>
        public string ReadString( bool streamIsCRLF )
        {
            string text = ReadString();
            return streamIsCRLF == StringAndStringBuilderExtension.IsCRLF ? text : text.NormalizeEOL();
        }

        /// <summary>
        /// Reads a potentially null string.
        /// </summary>
        /// <param name="streamIsCRLF">True if the <see cref="BinaryReader.BaseStream"/> contains 
        /// strings with CRLF end-of-line, false if the end-of-line is LF only.</param>
        /// <returns>The string or null.</returns>
        public string ReadNullableString( bool streamIsCRLF )
        {
            return ReadBoolean() ? ReadString( streamIsCRLF ) : null;
        }

        /// <summary>
        /// Reads a potentially null string.
        /// </summary>
        /// <returns>The string or null.</returns>
        public string ReadNullableString()
        {
            return ReadBoolean() ? ReadString() : null;
        }

        /// <summary>
        /// Reads a string, using the default <see cref="StringPool"/>.
        /// </summary>
        /// <returns>The string or null.</returns>
        public string ReadSharedString()
        {
            var r = StringPool.TryRead( out string s );
            return r.Success ? s : r.SetReadResult( ReadString() );
        }

        /// <summary>
        /// Reads a DateTime value.
        /// </summary>
        /// <returns>The DateTime read.</returns>
        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary( ReadInt64() );
        }

        /// <summary>
        /// Reads a TimeSpan value.
        /// </summary>
        /// <returns>The TimeSpan read.</returns>
        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks( ReadInt64() );
        }

        /// <summary>
        /// Reads a DateTimeOffset value.
        /// </summary>
        /// <returns>The DateTimeOffset read.</returns>
        public DateTimeOffset ReadDateTimeOffset()
        {
            return new DateTimeOffset( ReadDateTime(), TimeSpan.FromMinutes( ReadInt16() ) );
        }

        /// <summary>
        /// Reads a Guid value.
        /// </summary>
        /// <returns>The Guid read.</returns>
        public Guid ReadGuid()
        {
            return new Guid( ReadBytes( 16 ) );
        }
    }

}