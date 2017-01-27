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
    public class CKBinaryReader : BinaryReader
    {
        /// <summary>
        /// Initializes a new <see cref="CKBinaryReader"/> based on the specified
        /// stream and using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        public CKBinaryReader( Stream input )
            : base( input )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryReader"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public CKBinaryReader( Stream input, Encoding encoding )
            : base( input, encoding )
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
        }

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

    }

}
