using Microsoft.Toolkit.Diagnostics;
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
    /// Specializes <see cref="BinaryWriter"/> to expose helpers.
    /// </summary>
    public class CKBinaryWriter : BinaryWriter, ICKBinaryWriter
    {
        /// <summary>
        /// Implements a simple object pool that works with its <see cref="CKBinaryReader.ObjectPool{T}"/>
        /// companion.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        public class ObjectPool<T> where T : notnull
        {
            readonly Dictionary<T, int> _pool;
            readonly ICKBinaryWriter _w;

            /// <summary>
            /// Initializes a new object pool for a writer.
            /// </summary>
            /// <param name="w">The binary writer. Must not be null.</param>
            /// <param name="comparer">The comparer to use.</param>
            public ObjectPool( ICKBinaryWriter w, IEqualityComparer<T>? comparer = null )
            {
                Guard.IsNotNull( w, nameof( w ) );
                _pool = new Dictionary<T, int>( comparer );
                _w = w ?? throw new ArgumentNullException( nameof( w ) );
            }

            /// <summary>
            /// Registers the object if it has not been seen before and returns true: the
            /// actual object must be written.
            /// If the object has already been registered its index is written and false
            /// is returned.
            /// </summary>
            /// <param name="o">The object to write.</param>
            /// <param name="mustWriteMarker">
            /// Be default, '2' is written in the stream whenever the object is registered for the first
            /// time in this pool. Advanced scenarii can use this marker with any byte value greater or
            /// equal to 2.
            /// '1' followed by the object number is written whenever the object has already been handled.
            /// '0' is written for the default value of <typeparamref name="T"/>.
            /// </param>
            /// <returns>
            /// True if the object must be written, false if it has already been and
            /// there is nothing to do.
            /// </returns>
            public bool MustWrite( [AllowNull]T o, byte mustWriteMarker = 2 )
            {
                if( EqualityComparer<T>.Default.Equals( o, default ) )
                {
                    _w.Write( (byte)0 );
                    return false;
                }
                Debug.Assert( o != null );
                if( _pool.TryGetValue( o, out var num ) )
                {
                    _w.Write( (byte)1 );
                    _w.WriteNonNegativeSmallInt32( num );
                    return false;
                }
                _pool.Add( o, _pool.Count );
                _w.Write( mustWriteMarker );
                return true;
            }

            /// <summary>
            /// Writes a reference to the value or the value itself if it has not been registered yet
            /// thanks to an actual writer function.
            /// </summary>
            /// <param name="o">The value to write.</param>
            /// <param name="actualWriter">
            /// Actual writer. Must not be null.
            /// Note that it will not be called if the value is the default of the <typeparamref name="T"/>:
            /// for reference types, the actual writer will never have to handle null.
            /// </param>
            public void Write( T o, Action<ICKBinaryWriter, T> actualWriter )
            {
                if( MustWrite( o ) ) actualWriter( _w, o );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and using UTF-8 encoding.
        /// The stream will be closed once this writer is disposed.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public CKBinaryWriter( Stream output )
            : this( output, Encoding.UTF8, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and character encoding.
        /// The stream will be closed once this writer is disposed.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public CKBinaryWriter( Stream output, Encoding encoding )
            : this( output, encoding, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after this writer object is disposed; otherwise, false.</param>
        public CKBinaryWriter( Stream output, Encoding encoding, bool leaveOpen )
            : base( output, encoding, leaveOpen )
        {
            StringPool = new ObjectPool<string>( this, StringComparer.Ordinal );
        }

        /// <inheritdoc />
        public ObjectPool<string> StringPool { get; }

        /// <inheritdoc />
        public void WriteNonNegativeSmallInt32( int value ) => Write7BitEncodedInt( value );

        /// <inheritdoc />
        public void WriteSmallInt32( int value, int minNegativeValue = -1 ) => Write7BitEncodedInt( value - minNegativeValue );

        /// <inheritdoc />
        public void WriteNullableString( string? s )
        {
            if( s != null )
            {
                Write( true );
                Write( s );
            }
            else Write( false );
        }

        /// <inheritdoc />
        public void WriteSharedString( string? s )
        {
            if( StringPool.MustWrite( s ) )
            {
                Debug.Assert( s != null );
                Write( s );
            }
        }

        /// <inheritdoc />
        public void Write( DateTime d )
        {
            Write( d.ToBinary() );
        }

        /// <inheritdoc />
        public void Write( TimeSpan t )
        {
            Write( t.Ticks );
        }

        /// <inheritdoc />
        public void Write( DateTimeOffset ds )
        {
            Write( ds.DateTime );
            Write( (short)ds.Offset.TotalMinutes );
        }

        /// <inheritdoc />
        public void Write( Guid g )
        {
            Write( g.ToByteArray() );
        }

        /// <inheritdoc />
        public void WriteNullableByte( byte? b )
        {
            if( !b.HasValue ) Write( (byte)0xFE );
            else
            {
                // BinaryWriter is always little endian.
                // The LSB goes first and then comes the MSB: here we want 0xFF to be
                // the escaped value read as a byte, so we have to write the ushort 0xFEFF.
                byte v = b.Value;
                if( v == 0xFE ) Write( (ushort)0xFEFF );
                else if( v == 0xFF ) Write( (ushort)0xFFFF );
                else Write( v );
            }
        }

        /// <inheritdoc />
        public void WriteNullableBool( bool? b )
        {
            if( !b.HasValue ) Write( (byte)0x03 );
            else Write( b.Value ? (byte)0x01 : (byte)0x02 );
        }

        /// <inheritdoc />
        public void WriteNullableSByte( sbyte? b )
        {
            if( !b.HasValue ) Write( (byte)0x7F );
            else
            {
                // BinaryWriter is always little endian.
                // The LSB goes first and then comes the MSB: here we want 0x80 to be
                // the escaped value read as a byte, so we have to write the ushort 0x7F80.
                sbyte v = b.Value;
                if( v == 0x7F ) Write( (ushort)0x7F80 );
                else if( v == -128 ) Write( (ushort)0x8080 );
                else Write( v );
            }
        }

        static readonly byte[] _signedMax = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x01 };
        static readonly byte[] _signedMinusMax = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 };

        static readonly byte[] _unsignedMaxMinus1 = new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };
        static readonly byte[] _unsignedMax = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };


        /// <inheritdoc />
        public void WriteNullableInt16( short? v )
        {
            if( !v.HasValue ) Write( Int16.MaxValue );
            else
            {
                short i = v.Value;
                if( i == Int16.MaxValue ) Write( _signedMax, 6, 3 );
                else if( i == Int16.MinValue ) Write( _signedMinusMax, 6, 3 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableUInt16( ushort? v )
        {
            if( !v.HasValue ) Write( UInt16.MaxValue-1 );
            else
            {
                ushort i = v.Value;
                if( i == UInt16.MaxValue - 1 ) Write( _unsignedMaxMinus1, 6, 3 );
                else if( i == UInt16.MaxValue ) Write( _unsignedMax, 6, 3 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableInt32( int? v )
        {
            if( !v.HasValue ) Write( Int32.MaxValue );
            else
            {
                int i = v.Value;
                if( i == Int32.MaxValue ) Write( _signedMax, 4, 5 );
                else if( i == Int32.MinValue ) Write( _signedMinusMax, 4, 5 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableUInt32( uint? v )
        {
            if( !v.HasValue ) Write( UInt32.MaxValue - 1 );
            else
            {
                uint i = v.Value;
                if( i == UInt32.MaxValue - 1 ) Write( _unsignedMaxMinus1, 4, 5 );
                else if( i == UInt32.MaxValue ) Write( _unsignedMax, 4, 5 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableInt64( long? v )
        {
            if( !v.HasValue ) Write( Int64.MaxValue );
            else
            {
                long i = v.Value;
                if( i == Int64.MaxValue ) Write( _signedMax, 0, 9 );
                else if( i == Int64.MinValue ) Write( _signedMinusMax, 0, 9 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableUInt64( ulong? v )
        {
            if( !v.HasValue ) Write( UInt64.MaxValue - 1 );
            else
            {
                ulong i = v.Value;
                if( i == UInt64.MaxValue - 1 ) Write( _unsignedMaxMinus1, 0, 9 );
                else if( i == UInt64.MaxValue ) Write( _unsignedMax, 0, 9 );
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteNullableChar( char? v )
        {
            if( !v.HasValue ) Write( (char)(Char.MinValue + 1) );
            else
            {
                char i = v.Value;
                if( i == (char)(Char.MinValue + 1) ) { Write( Char.MinValue ); Write( (byte)0x01 ); }
                else if( i == Char.MinValue ) { Write( Char.MinValue ); Write( (byte)0x00 ); }
                else Write( i );
            }
        }

        /// <inheritdoc />
        public void WriteEnum<T>( T v ) where T : struct, Enum
        {
            var u = typeof( T ).GetEnumUnderlyingType();
            if( u == typeof( int ) ) Write( (int)(object)v );
            else if( u == typeof( byte ) ) Write( (byte)(object)v );
            else if( u == typeof( short ) ) Write( (short)(object)v );
            else if( u == typeof( long ) ) Write( (long)(object)v );
            else if( u == typeof( sbyte ) ) Write( (sbyte)(object)v );
            else if( u == typeof( uint ) ) Write( (uint)(object)v );
            else if( u == typeof( ushort ) ) Write( (ushort)(object)v );
            else if( u == typeof( ulong ) ) Write( (ulong)(object)v );
            else throw new NotSupportedException( $"Unhandled base enum type: {u}" );
        }

        /// <inheritdoc />
        public void WriteNullableEnum<T>( T? v ) where T : struct, Enum
        {
            // This is ABSOLUTELY ugly... but it does the job.
            // This SHOULD be improved!
            var u = typeof( T ).GetEnumUnderlyingType();
            if( u == typeof( int ) ) { if( v.HasValue ) WriteNullableInt32( (int)(object)v.Value ); else WriteNullableInt32( null ); }
            else if( u == typeof( byte ) ) { if( v.HasValue ) WriteNullableByte( (byte)(object)v.Value ); else WriteNullableByte( null ); }
            else if( u == typeof( short ) ) { if( v.HasValue ) WriteNullableInt16( (short)(object)v.Value ); else WriteNullableInt16( null ); }
            else if( u == typeof( long ) ) { if( v.HasValue ) WriteNullableInt64( (long)(object)v.Value ); else WriteNullableInt64( null ); }
            else if( u == typeof( sbyte ) ) { if( v.HasValue ) WriteNullableSByte( (sbyte)(object)v.Value ); else WriteNullableSByte( null ); }
            else if( u == typeof( uint ) ) { if( v.HasValue ) WriteNullableUInt32( (uint)(object)v.Value ); else WriteNullableUInt32( null ); }
            else if( u == typeof( ushort ) ) { if( v.HasValue ) WriteNullableUInt16( (ushort)(object)v.Value ); else WriteNullableUInt16( null ); }
            else if( u == typeof( ulong ) ) { if( v.HasValue ) WriteNullableUInt64( (ulong)(object)v.Value ); else WriteNullableUInt64( null ); }
            else throw new NotSupportedException( $"Unhandled base enum type: {u}" );
        }

        /// <inheritdoc />
        public void WriteNullableDateTime( DateTime? v )
        {
            // No risk and still efficient: there is a MaxTicks that is below
            // long.MaxValue: null and all DateTime requires 8 bytes (never 9).
            Debug.Assert( DateTime.MaxValue.Ticks < long.MaxValue );
            WriteNullableInt64( v?.ToBinary() );
        }

        /// <inheritdoc />
        public void WriteNullableTimeSpan( TimeSpan? t )
        {
            WriteNullableInt64( t?.Ticks );
        }

        /// <inheritdoc />
        public void WriteNullableDateTimeOffset( DateTimeOffset? ds )
        {
            WriteNullableInt64( ds?.DateTime.ToBinary() );
            if( ds.HasValue ) Write( (short)ds.Value.Offset.TotalMinutes );
        }

        /// <inheritdoc />
        public void WriteNullableGuid( Guid? g )
        {
            if( g.HasValue )
            {
                Write( true );
                Write( g.Value );
            }
            else
            {
                Write( false );
            }
        }

    }

}
