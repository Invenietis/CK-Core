using System;
using System.IO;

namespace CK.Core
{
    /// <summary>
    /// Reproduces <see cref="System.IO.BinaryReader"/> interface and adds support for
    /// standard basic types like <see cref="ReadDateTime"/>.
    /// </summary>
    public interface ICKBinaryReader : IDisposable
    {
        #region BinaryReader methods.

        /// <summary>
        /// Get the the underlying stream.
        /// </summary>
        Stream BaseStream { get; }

        /// <summary>
        /// Returns the next available character and does not advance the byte or character
        /// position.
        /// </summary>
        /// <returns>
        /// The next available character, or -1 if no more characters are available or the
        /// stream does not support seeking.
        /// </returns>
        int PeekChar();

        /// <summary>
        /// Reads characters from the underlying stream and advances the current position
        /// of the stream in accordance with the Encoding used and the specific character
        /// being read from the stream.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if no characters are currently
        /// available.
        /// </returns>
        int Read();

        /// <summary>
        /// Reads the specified number of bytes from the stream, starting from a specified
        /// point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>
        /// The number of bytes read into buffer. This might be less than the number of bytes
        /// requested if that many bytes are not available, or it might be zero if the end
        /// of the stream is reached.
        ///</returns>
        int Read( byte[] buffer, int index, int count );

        /// <summary>
        /// Reads the specified number of characters from the stream, starting from a specified
        /// point in the character array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>
        /// The total number of characters read into the buffer. This might be less than
        /// the number of characters requested if that many characters are not currently
        /// available, or it might be zero if the end of the stream is reached.
        /// </returns>
        int Read( char[] buffer, int index, int count );

        /// <summary>
        /// Reads a Boolean value from the current stream and advances the current position
        /// of the stream by one byte.
        /// </summary>
        /// <returns>true if the byte is nonzero; otherwise, false.</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads the next byte from the current stream and advances the current position
        /// of the stream by one byte.
        /// </summary>
        /// <returns>The next byte read from the current stream.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads the specified number of bytes from the current stream into a byte array
        /// and advances the current position by that number of bytes.
        /// </summary>
        /// <param name="count">
        /// The number of bytes to read. This value must be 0 or a non-negative number or
        /// an exception will occur.
        /// </param>
        /// <returns>
        /// A byte array containing data read from the underlying stream. This might be less
        /// than the number of bytes requested if the end of the stream is reached.
        /// </returns>
        byte[] ReadBytes( int count );

        /// <summary>
        /// Reads the next character from the current stream and advances the current position
        /// of the stream in accordance with the Encoding used and the specific character
        /// being read from the stream.
        /// </summary>
        /// <returns>A character read from the current stream.</returns>
        char ReadChar();

        /// <summary>
        /// Reads the specified number of characters from the current stream, returns the
        /// data in a character array, and advances the current position in accordance with
        /// the Encoding used and the specific character being read from the stream.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>
        /// A character array containing data read from the underlying stream. This might
        /// be less than the number of characters requested if the end of the stream is reached.
        /// </returns>
        char[] ReadChars( int count );

        /// <summary>
        /// Reads a decimal value from the current stream and advances the current position
        /// of the stream by sixteen bytes.
        /// </summary>
        /// <returns>A decimal value read from the current stream.</returns>
        decimal ReadDecimal();

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream and advances the
        /// current position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte floating point value read from the current stream.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current
        /// position of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        short ReadInt16();

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current
        /// position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        int ReadInt32();

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current
        /// position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        long ReadInt64();

        /// <summary>
        /// Reads a signed byte from this stream and advances the current position of the
        /// stream by one byte.
        /// </summary>
        /// <returns>A signed byte read from the current stream.</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the
        /// current position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        float ReadSingle();

        /// <summary>
        /// Reads a string from the current stream. The string is prefixed with the length,
        /// encoded as an integer seven bits at a time.
        /// </summary>
        /// <returns>The string being read.</returns>
        string ReadString();

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using little-endian encoding
        /// and advances the position of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        ushort ReadUInt16();

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the position
        /// of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        uint ReadUInt32();

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position
        /// of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        ulong ReadUInt64();

        #endregion

        /// <summary>
        /// Gets the string pool (see <see cref="ICKBinaryWriter.StringPool"/>) used
        /// by <see cref="ReadSharedString"/> method.
        /// </summary>
        CKBinaryReader.ObjectPool<string> StringPool { get; }

        /// <summary>
        /// Reads a DateTime value.
        /// </summary>
        /// <returns>The DateTime read.</returns>
        DateTime ReadDateTime();

        /// <summary>
        /// Reads a TimeSpan value.
        /// </summary>
        /// <returns>The TimeSpan read.</returns>
        TimeSpan ReadTimeSpan();

        /// <summary>
        /// Reads a DateTimeOffset value.
        /// </summary>
        /// <returns>The DateTimeOffset read.</returns>
        DateTimeOffset ReadDateTimeOffset();

        /// <summary>
        /// Reads a Guid value.
        /// </summary>
        /// <returns>The Guid read.</returns>
        Guid ReadGuid();

        /// <summary>
        /// Reads in a 32-bit integer in compressed format.
        /// </summary>
        /// <returns>A 32-bit integer</returns>
        int ReadNonNegativeSmallInt32();

        /// <summary>
        /// Reads a potentially null string.
        /// </summary>
        /// <param name="streamIsCRLF">
        /// True if the <see cref="BinaryReader.BaseStream"/> contains strings with CRLF end-of-line,
        /// false if the end-of-line is LF only.
        /// </param>
        /// <returns>The string that can be null.</returns>
        string ReadNullableString( bool streamIsCRLF );

        /// <summary>
        /// Reads a potentially null string.
        /// </summary>
        /// <returns>The string that can be null.</returns>
        string ReadNullableString();

        /// <summary>
        /// Reads in a 32-bit integer in compressed format written by <see cref="CKBinaryWriter.WriteSmallInt32(int, int)"/>. 
        /// </summary>
        /// <param name="minNegativeValue">The same negative value used to write the integer.</param>
        /// <returns>A 32-bit integer.</returns>
        int ReadSmallInt32( int minNegativeValue = -1 );

        /// <summary>
        /// Reads and normalizes a string according to System.Environment.NewLine. The fact
        /// that the data has actually be saved with LF or CRLF must be known.
        /// </summary>
        /// <param name="streamIsCRLF">
        /// True if the read stream contains strings with CRLF end-of-line,
        /// false if the end-of-line is LF only.
        /// </param>
        /// <returns>String with actual System.Environment.NewLine for end-of-line.</returns>
        string ReadString( bool streamIsCRLF );

        /// <summary>
        /// Reads a string, using the default <see cref="StringPool"/>.
        /// </summary>
        /// <returns>The string or null.</returns>
        string ReadSharedString();
    }
}
