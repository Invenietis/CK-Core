using System;
using System.IO;

namespace CK.Core
{
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
        /// should definitly be a good idea...
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
        /// Writes a 32-bit integer in compressed format, accomodating rooms for some negative values.
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
        void WriteNullableString( string s );

        /// <summary>
        /// Writes a string, using the default <see cref="StringPool"/>.
        /// </summary>
        /// <param name="s">The string to write. Can be null.</param>
        void WriteSharedString( string s );

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
        /// Writes a DateTimeOffset value.
        /// </summary>
        /// <param name="g">The value to write.</param>
        void Write( Guid g );

    }
}
