using System;
using System.IO;

namespace CK.Core;

/// <summary>
/// Reproduces <see cref="System.IO.BinaryWriter"/> interface and adds support for
/// standard basic types like <see cref="WriteSharedString(string)"/> or <see cref="Write(Guid)"/>.
/// </summary>
public interface ICKBinaryWriter
{
    #region BinaryWriter methods.

    /// <summary>
    /// Gets the underlying stream.
    /// Note that calling <see cref="Flush"/> before any direct writes into this stream
    /// should definitely be a good idea...
    /// </summary>
    Stream BaseStream { get; }

    /// <summary>
    /// Clears all buffers for the current writer and causes any buffered data to be
    /// written to the underlying device.
    /// </summary>
    void Flush();

    /// <summary>
    /// Writes an eight-byte unsigned integer to the current stream and advances the
    /// stream position by eight bytes.
    /// </summary>
    /// <param name="value">The eight-byte unsigned integer to write.</param>
    void Write( ulong value );

    /// <summary>
    /// Writes a four-byte unsigned integer to the current stream and advances the stream
    /// position by four bytes.
    /// </summary>
    /// <param name="value">The four-byte unsigned integer to write.</param>
    void Write( uint value );

    /// <summary>
    /// Writes a two-byte unsigned integer to the current stream and advances the stream
    /// position by two bytes.
    /// </summary>
    /// <param name="value">The two-byte unsigned integer to write.</param>
    void Write( ushort value );

    /// <summary>
    /// Writes a length-prefixed string to this stream in the current encoding of the
    /// System.IO.BinaryWriter, and advances the current position of the stream in accordance
    /// with the encoding used and the specific characters being written to the stream.
    /// </summary>
    /// <param name="value">
    /// The value to write. Can not be null. Use <see cref="WriteNullableString(string)"/>
    /// or <see cref="WriteSharedString(string)"/> whenever null must be handled.
    /// </param>
    void Write( string value );

    /// <summary>
    /// Writes a two-byte floating-point value.
    /// </summary>
    /// <param name="value">The two-byte floating-point value to write.</param>
    void Write( Half value );

    /// <summary>
    /// Writes a four-byte floating-point value to the current stream and advances the
    /// stream position by four bytes.
    /// </summary>
    /// <param name="value">The four-byte floating-point value to write.</param>
    void Write( float value );

    /// <summary>
    /// Writes a signed byte to the current stream and advances the stream position by
    /// one byte.
    /// /// </summary>
    /// <param name="value">The signed byte to write.</param>
    void Write( sbyte value );

    /// <summary>
    /// Writes an eight-byte signed integer to the current stream and advances the stream
    /// position by eight bytes.
    /// </summary>
    /// <param name="value">The eight-byte signed integer to write.</param>
    void Write( long value );

    /// <summary>
    /// Writes a four-byte signed integer to the current stream and advances the stream
    /// position by four bytes.
    /// </summary>
    /// <param name="value">The four-byte signed integer to write.</param>
    void Write( int value );

    /// <summary>
    /// Writes a two-byte signed integer to the current stream and advances the stream
    /// position by two bytes.
    /// </summary>
    /// <param name="value">The two-byte signed integer to write.</param>
    void Write( short value );

    /// <summary>
    /// Writes a decimal value to the current stream and advances the stream position
    /// by sixteen bytes.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    void Write( decimal value );

    /// <summary>
    /// Writes a section of a character array to the current stream, and advances the
    /// current position of the stream in accordance with the Encoding used and perhaps
    /// the specific characters being written to the stream.
    /// </summary>
    /// <param name="chars">A character array containing the data to write. Must not be null.</param>
    /// <param name="index">The starting point in chars from which to begin writing.</param>
    /// <param name="count">The number of characters to write.</param>
    void Write( char[] chars, int index, int count );

    /// <summary>
    /// Writes a character array to the current stream and advances the current position
    /// of the stream in accordance with the Encoding used and the specific characters
    /// being written to the stream.
    /// </summary>
    /// <param name="chars">A character array containing the data to write. Must not be null.</param>
    void Write( char[] chars );

    /// <summary>
    /// Writes a region of a byte array to the current stream. 
    /// </summary>
    /// <param name="buffer">A byte array containing the data to write. Must not be null.</param>
    /// <param name="index">The starting point in buffer at which to begin writing.</param>
    /// <param name="count">The number of bytes to write.</param>
    void Write( byte[] buffer, int index, int count );

    /// <summary>
    /// Writes a byte array to the underlying stream.
    /// </summary>
    /// <param name="buffer">A byte array containing the data to write. Must not be null.</param>
    void Write( byte[] buffer );

    /// <summary>
    /// Writes a span of bytes to the current stream.
    /// </summary>
    /// <param name="buffer">The span of bytes to write.</param>
    void Write( ReadOnlySpan<byte> buffer );

    /// <summary>
    /// Writes a span of characters to the current stream, and advances the current position
    /// of the stream in accordance with the Encoding used and perhaps the specific characters
    /// being written to the stream.
    /// </summary>
    /// <param name="chars">A span of chars to write.</param>
    void Write( ReadOnlySpan<char> chars );

    /// <summary>
    /// Writes an unsigned byte to the current stream and advances the stream position
    /// by one byte.
    /// </summary>
    /// <param name="value">The unsigned byte to write.</param>
    void Write( byte value );

    /// <summary>
    /// Writes a one-byte Boolean value to the current stream, with 0 representing false
    /// and 1 representing true.
    /// </summary>
    /// <param name="value">The Boolean value to write (0 or 1).</param>
    void Write( bool value );

    /// <summary>
    /// Writes an eight-byte floating-point value to the current stream and advances
    /// the stream position by eight bytes.
    /// </summary>
    /// <param name="value">The eight-byte floating-point value to write.</param>
    void Write( double value );

    /// <summary>
    /// Writes a Unicode character to the current stream and advances the current position
    /// of the stream in accordance with the Encoding used and the specific characters
    /// being written to the stream.
    /// The character must not be a surrogate.
    /// </summary>
    /// <param name="c">The non-surrogate, Unicode character to write.</param>
    void Write( char c );

    #endregion

    /// <summary>
    /// Gets the string pool used by <see cref="WriteSharedString"/> method.
    /// It uses a <see cref="StringComparer.Ordinal"/> comparer.
    /// </summary>
    CKBinaryWriter.ObjectPool<string> StringPool { get; }

    /// <summary>
    /// Writes a 32-bit 0 or positive integer in compressed format. See remarks.
    /// </summary>
    /// <param name="value">A 32-bit integer (should not be negative).</param>
    /// <remarks>
    /// Using this method to write a negative integer is the same as using it with a large
    /// positive number: the storage will actually require more than 4 bytes.
    /// It is perfectly valid, except that it is more "expansion" than "compression" :). 
    /// </remarks>
    void WriteNonNegativeSmallInt32( int value );

    /// <summary>
    /// Writes a 32-bit integer in compressed format, accommodating rooms for some negative values.
    /// The <paramref name="minNegativeValue"/> simply offsets the written value.
    /// Use <see cref="CKBinaryReader.ReadSmallInt32(int)"/> with the 
    /// same <paramref name="minNegativeValue"/> to read it back.
    /// </summary>
    /// <param name="value">A 32-bit integer (greater or equal to <paramref name="minNegativeValue"/>).</param>
    /// <param name="minNegativeValue">Lowest possible negative value.</param>
    /// <remarks>
    /// <para>
    /// Writing a negative value lower than the <paramref name="minNegativeValue"/> is totally possible, however
    /// more than 4 bytes will be required for them.
    /// </para>
    /// <para>
    /// The default value of -1 is perfect to write small integers that are greater or equal to -1.
    /// </para>
    /// </remarks>
    void WriteSmallInt32( int value, int minNegativeValue = -1 );

    /// <summary>
    /// Writes a potentially null string.
    /// You can use <see cref="WriteSharedString(string)"/> if the string
    /// has good chances to appear multiple times. 
    /// </summary>
    /// <param name="s">String to write.</param>
    void WriteNullableString( string? s );

    /// <summary>
    /// Writes a string, using the default <see cref="StringPool"/>.
    /// </summary>
    /// <param name="s">The string to write. Can be null.</param>
    void WriteSharedString( string? s );

    /// <summary>
    /// Writes a DateTime value.
    /// </summary>
    /// <param name="d">The value to write.</param>
    void Write( DateTime d );

    /// <summary>
    /// Writes a TimeSpan value.
    /// </summary>
    /// <param name="t">The value to write.</param>
    void Write( TimeSpan t );

    /// <summary>
    /// Writes a DateTimeOffset value.
    /// </summary>
    /// <param name="ds">The value to write.</param>
    void Write( DateTimeOffset ds );

    /// <summary>
    /// Writes a Guid value.
    /// </summary>
    /// <param name="g">The value to write.</param>
    void Write( Guid g );

    /// <summary>
    /// Writes a nullable byte value.
    /// Null and values in [0,253] use 1 byte.
    /// 254 and 255 use 2 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableByte( byte? b );

    /// <summary>
    /// Writes a nullable bool value.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableBool( bool? b );

    /// <summary>
    /// Writes a nullable signed byte value.
    /// Null and values in [-127,126] use 1 byte.
    /// -128 and 127 use 2 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableSByte( sbyte? b );

    /// <summary>
    /// Writes a nullable short value (<see cref="Int16"/>).
    /// Null and values between <see cref="Int16.MinValue"/> and <see cref="Int16.MaxValue"/> use 2 bytes.
    /// <see cref="Int16.MinValue"/> and <see cref="Int16.MaxValue"/> use 3 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableInt16( short? b );

    /// <summary>
    /// Writes a nullable unsigned short value (<see cref="UInt16"/>).
    /// Null and values below <see cref="UInt16.MaxValue"/>-1 use 2 bytes.
    /// <see cref="UInt16.MaxValue"/>-1 and <see cref="UInt16.MaxValue"/> use 3 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableUInt16( ushort? b );

    /// <summary>
    /// Writes a nullable int value (<see cref="Int32"/>).
    /// Null and values between <see cref="Int32.MinValue"/> and <see cref="Int32.MaxValue"/> use 4 bytes.
    /// <see cref="Int32.MinValue"/> and <see cref="Int32.MaxValue"/> use 5 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableInt32( int? b );

    /// <summary>
    /// Writes a nullable unsigned int value (<see cref="UInt32"/>).
    /// Null and values below <see cref="UInt32.MaxValue"/>-1 use 4 bytes.
    /// <see cref="UInt32.MaxValue"/>-1 and <see cref="UInt32.MaxValue"/> use 5 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableUInt32( uint? b );

    /// <summary>
    /// Writes a nullable long value (<see cref="Int64"/>).
    /// Null and values between <see cref="Int64.MinValue"/> and <see cref="Int64.MaxValue"/> use 8 bytes.
    /// <see cref="Int64.MinValue"/> and <see cref="Int64.MaxValue"/> use 9 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableInt64( long? b );

    /// <summary>
    /// Writes a nullable unsigned long value (<see cref="UInt64"/>).
    /// Null and values below <see cref="UInt32.MaxValue"/>-1 use 8 bytes.
    /// <see cref="UInt64.MaxValue"/>-1 and <see cref="UInt64.MaxValue"/> use 9 bytes.
    /// </summary>
    /// <param name="b">The value to write.</param>
    void WriteNullableUInt64( ulong? b );

    /// <summary>
    /// Writes the enum value as its number value (<see cref="Write(byte)"/> ... <see cref="Write(ulong)"/>)
    /// depending on its <see cref="Type.GetEnumUnderlyingType()"/>.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="v">The enum value.</param>
    [Obsolete( "Simply write the cast to the enum's integral type like: w.Write( (uint)e ); instead.", true )]
    void WriteEnum<T>( T v ) where T : struct, Enum;

    /// <summary>
    /// Writes the enum value as its nullable number value (<see cref="WriteNullableByte(byte?)"/> ... <see cref="WriteNullableUInt64(ulong?)"/>)
    /// depending on its <see cref="Type.GetEnumUnderlyingType()"/>.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="v">The enum value.</param>
    [Obsolete( "Simply write the cast to the enum's integral type like: w.WriteNullableUInt32( (uint?)e ); instead.", true )]
    void WriteNullableEnum<T>( T? v ) where T : struct, Enum;

    /// <summary>
    /// Writes a nullable unicode character (<see cref="Char"/>).
    /// Actual byte length depends directly on the used string encoding.
    /// Null and values above <see cref="Char.MinValue"/>+1 use one character (2 bytes below 0xff, 3 bytes below 0xffff, etc.).
    /// <see cref="Char.MinValue"/>+1 and <see cref="Char.MinValue"/> use one character (2 bytes below 0xff, 3 bytes below 0xffff, etc.), plus one byte.
    /// </summary>
    /// <param name="v">The value to write.</param>
    void WriteNullableChar( char? v );

    /// <summary>
    /// Writes a nullable DateTime value.
    /// </summary>
    /// <param name="v">The value to write.</param>
    void WriteNullableDateTime( DateTime? v );

    /// <summary>
    /// Writes a nullable TimeSpan value.
    /// </summary>
    /// <param name="t">The value to write.</param>
    void WriteNullableTimeSpan( TimeSpan? t );

    /// <summary>
    /// Writes a nullable DateTimeOffset value.
    /// </summary>
    /// <param name="ds">The value to write.</param>
    void WriteNullableDateTimeOffset( DateTimeOffset? ds );

    /// <summary>
    /// Writes a nullable Guid value.
    /// </summary>
    /// <param name="g">The value to write.</param>
    void WriteNullableGuid( Guid? g );

    /// <summary>
    /// Writes a nullable Single (<see cref="float"/>) value.
    /// Simple pattern here: a 0 byte marker for null
    /// and a 1 byte marker followed by the float for non null.
    /// </summary>
    /// <param name="f">The value to write.</param>
    /// <remarks>
    /// We could have played with the 2 NaN: 0x7FFFFFFF  and 0xFFFFFFFF 
    /// (the sign bit differs) but we prefer to stay on the safe side here since
    /// it would mean to "normalize" one of the NaN to be the actual NaN (the other being
    /// the null).
    /// </remarks>
    void WriteNullableSingle( float? f );

    /// <summary>
    /// Writes a nullable double value.
    /// Simple pattern here: a 0 byte marker for null
    /// and a 1 byte marker followed by the double for non null.
    /// </summary>
    /// <param name="d">The value to write.</param>
    /// <remarks>
    /// We could have played with the 2 NaN: 0x7FFFFFFFFFFFFFFF and 0xFFFFFFFFFFFFFFFF
    /// (the sign bit differs) but we prefer to stay on the safe side here since
    /// it would mean to "normalize" one of the NaN to be the actual NaN (the other being
    /// the null).
    /// </remarks>
    void WriteNullableDouble( double? d );
}
