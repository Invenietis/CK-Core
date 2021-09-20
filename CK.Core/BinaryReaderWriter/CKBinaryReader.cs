using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            readonly List<T?> _objects;
            readonly ICKBinaryReader _r;

            /// <summary>
            /// Initializes a new object pool reader.
            /// </summary>
            /// <param name="r">The reader. Must not be null.</param>
            public ObjectPool( ICKBinaryReader r )
            {
                _objects = new List<T?>();
                _r = r ?? throw new ArgumentNullException( nameof( r ) );
            }

            /// <summary>
            /// Captures the <see cref="TryRead(out T)"/> result.
            /// When <see cref="Success"/> is false, <see cref="SetReadResult(T)"/> must be called
            /// as soon as the object has been read.
            /// </summary>
            public readonly struct ReadState
            {
                readonly ObjectPool<T>? _pool;
                readonly int _num;
                readonly byte _writeMarker;

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
                    if( _pool == null ) throw new InvalidOperationException();
                    _pool._objects[_num] = read;
                    return read;
                }

                internal ReadState( ObjectPool<T> p, byte marker )
                {
                    _pool = p;
                    _num = p._objects.Count;
                    _writeMarker = marker;
                    p._objects.Add( default );
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
            public ReadState TryRead( [MaybeNull]out T already )
            {
                byte b = _r.ReadByte();
                switch( b )
                {
                    case 0: already = default; return new ReadState();
                    case 1: already = _objects[_r.ReadNonNegativeSmallInt32()]; return new ReadState( 1 );
                    default:already = default; return new ReadState( this, b );
                }
            }

            /// <summary>
            /// Reads a value either from this pool if it has already been read or, when not yet read,
            /// thanks to an actual reader function.
            /// </summary>
            /// <param name="actualReader">Function that will be called if the value must actually be read.</param>
            /// <returns>The value.</returns>
            public T? Read( Func<ReadState,ICKBinaryReader,T> actualReader )
            {
                byte b = _r.ReadByte();
                switch( b )
                {
                    case 0: return default;
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

        /// <inheritdoc/>
        public ObjectPool<string> StringPool { get; }

        /// <inheritdoc/>
        public int ReadNonNegativeSmallInt32() => Read7BitEncodedInt();

        /// <inheritdoc/>
        public int ReadSmallInt32( int minNegativeValue = -1 ) => Read7BitEncodedInt() + minNegativeValue;

        /// <inheritdoc/>
        public string ReadString( bool streamIsCRLF )
        {
            string text = ReadString();
            return streamIsCRLF == StringAndStringBuilderExtension.IsCRLF ? text : text.NormalizeEOL();
        }

        /// <inheritdoc/>
        public string? ReadNullableString( bool streamIsCRLF )
        {
            return ReadBoolean() ? ReadString( streamIsCRLF ) : null;
        }

        /// <inheritdoc/>
        public string? ReadNullableString()
        {
            return ReadBoolean() ? ReadString() : null;
        }

        /// <inheritdoc/>
        public string? ReadSharedString()
        {
            var r = StringPool.TryRead( out string? s );
            return r.Success ? s : r.SetReadResult( ReadString() );
        }

        /// <inheritdoc/>
        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary( ReadInt64() );
        }

        /// <inheritdoc/>
        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks( ReadInt64() );
        }

        /// <inheritdoc/>
        public DateTimeOffset ReadDateTimeOffset()
        {
            return new DateTimeOffset( ReadDateTime(), TimeSpan.FromMinutes( ReadInt16() ) );
        }

        /// <inheritdoc/>
        public Guid ReadGuid()
        {
            return new Guid( ReadBytes( 16 ) );
        }

        /// <inheritdoc/>
        public byte? ReadNullableByte()
        {
            var v = ReadByte();
            if( v == 0xFE ) return null;
            if( v == 0xFF ) v = ReadByte();
            return v;
        }

        /// <inheritdoc/>
        public bool? ReadNullableBool()
        {
            return ReadByte() switch
            {
                1 => true,
                2 => false,
                3 => null,
                _ => throw new InvalidDataException(),
            };
        }

        /// <inheritdoc/>
        public sbyte? ReadNullableSByte()
        {
            var v = ReadSByte();
            if( v == 0x7F ) return null;
            if( v == -128 ) v = ReadSByte();
            return v;
        }

        /// <inheritdoc/>
        public ushort? ReadNullableUInt16()
        {
            var v = ReadUInt16();
            if( v == UInt16.MaxValue-1 ) return null;
            if( v == UInt16.MaxValue ) v = (ushort)(UInt16.MaxValue - ReadByte());
            return v;
        }

        /// <inheritdoc/>
        public short? ReadNullableInt16()
        {
            var v = ReadInt16();
            if( v == Int16.MaxValue ) return null;
            if( v == Int16.MinValue ) v = ReadByte() == 0 ? Int16.MinValue : Int16.MaxValue;
            return v;
        }

        /// <inheritdoc/>
        public uint? ReadNullableUInt32()
        {
            var v = ReadUInt32();
            if( v == UInt32.MaxValue - 1 ) return null;
            if( v == UInt32.MaxValue ) v = UInt32.MaxValue - ReadByte();
            return v;
        }

        /// <inheritdoc/>
        public int? ReadNullableInt32()
        {
            var v = ReadInt32();
            if( v == Int32.MaxValue ) return null;
            if( v == Int32.MinValue ) v = ReadByte() == 0 ? Int32.MinValue : Int32.MaxValue;
            return v;
        }

        /// <inheritdoc/>
        public ulong? ReadNullableUInt64()
        {
            var v = ReadUInt64();
            if( v == UInt64.MaxValue - 1 ) return null;
            if( v == UInt64.MaxValue ) v = UInt64.MaxValue - ReadByte();
            return v;
        }

        /// <inheritdoc/>
        public long? ReadNullableInt64()
        {
            var v = ReadInt64();
            if( v == Int64.MaxValue ) return null;
            if( v == Int64.MinValue ) v = ReadByte() == 0 ? Int64.MinValue : Int64.MaxValue;
            return v;
        }


        /// <inheritdoc/>
        public char? ReadNullableChar()
        {
            var v = ReadChar();
            if( v == (char)(Char.MinValue + 1) ) return null;
            if( v == Char.MinValue ) v = ReadByte() == 0x01 ? (char) (Char.MinValue + 1) : Char.MinValue;
            return v;
        }

        /// <inheritdoc/>
        public T ReadEnum<T>() where T : struct, Enum
        {
            var u = typeof( T ).GetEnumUnderlyingType();
            if( u == typeof( int ) ) return (T)(object)ReadInt32();
            if( u == typeof( byte ) ) return (T)(object)ReadByte();
            if( u == typeof( short ) ) return (T)(object)ReadInt16();
            if( u == typeof( long ) ) return (T)(object)ReadInt64();
            if( u == typeof( sbyte ) ) return (T)(object)ReadSByte();
            if( u == typeof( uint ) ) return (T)(object)ReadUInt32();
            if( u == typeof( ushort ) ) return (T)(object)ReadUInt16();
            if( u == typeof( ulong ) ) return (T)(object)ReadUInt64();
            throw new NotSupportedException( $"Unhandled base enum type: {u}" );
        }

        /// <inheritdoc/>
        public T? ReadNullableEnum<T>() where T : struct, Enum
        {
            var u = typeof( T ).GetEnumUnderlyingType();
            if( u == typeof( int ) ) { var v = ReadNullableInt32(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( byte ) ) { var v = ReadNullableByte(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( short ) ) { var v = ReadNullableInt16(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( long ) ) { var v = ReadNullableInt64(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( sbyte ) ) { var v = ReadNullableSByte(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( uint ) ) { var v = ReadNullableUInt32(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( ushort ) ) { var v = ReadNullableUInt16(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            if( u == typeof( ulong ) ) { var v = ReadNullableUInt64(); if( v.HasValue ) return (T)(object)v.Value; else return null; }
            throw new NotSupportedException( $"Unhandled base enum type: {u}" );
        }

        /// <inheritdoc />
        public DateTime? ReadNullableDateTime()
        {
            long? t = ReadNullableInt64();
            return t.HasValue ? DateTime.FromBinary( t.Value ) : null;

        }

        /// <inheritdoc />
        public TimeSpan? ReadNullableTimeSpan()
        {
            long? t = ReadNullableInt64();
            return t.HasValue ? new TimeSpan( t.Value ) : null;
        }

        /// <inheritdoc />
        public DateTimeOffset? ReadNullableDateTimeOffset()
        {
            long? t = ReadNullableInt64();
            return t.HasValue
                    ? new DateTimeOffset( DateTime.FromBinary( t.Value ), TimeSpan.FromMinutes( ReadInt16() ) )
                    : null;
        }

        /// <inheritdoc />
        public Guid? ReadNullableGuid()
        {
            return ReadBoolean() ? ReadGuid() : null;
        }
    }

}
