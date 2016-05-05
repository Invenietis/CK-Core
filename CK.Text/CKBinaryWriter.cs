using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Text
{
    /// <summary>
    /// Specializes <see cref="BinaryWriter"/> to expose helpers.
    /// </summary>
    public class CKBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public CKBinaryWriter( Stream output )
            : base( output )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public CKBinaryWriter( Stream output, Encoding encoding )
            : base( output, encoding )
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
        }

        /// <summary>
        /// Writes a 32-bit integer in compressed format.
        /// </summary>
        /// <returns>A 32-bit integer</returns>
        public void WriteSmallInt32( int value ) => Write7BitEncodedInt( value );

        /// <summary>
        /// Writes a potentially null string.
        /// </summary>
        /// <param name="s">String to write.</param>
        public void WriteNullableString( string s )
        {
            if( s != null )
            {
                Write( true );
                Write( s );
            }
            else Write( false );
        }
    }

}
