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
        /// Writes a 32-bit 0 or positive integer in compressed format. See remarks.
        /// </summary>
        /// <param name="value">A 32-bit integer (should not be negative).</param>
        /// <remarks>
        /// Using this method to write a negative integer is the same as using ot with a large
        /// positive number: the storage will actually require more than 4 bytes.
        /// It is perfectly valid, except that it is more "expansion" than "compression" :). 
        /// </remarks>
        public void WriteNonNegativeSmallInt32( int value ) => Write7BitEncodedInt( value );

        /// <summary>
        /// Writes a 32-bit integer in compressed format, accomodating rooms for some negative values.
        /// The <paramref name="minNegativeValue"/> simply offsets the written value.
        /// Use <see cref="CKBinaryReader.ReadNonNegativeSmallInt32(int,int)"/> with the 
        /// same <paramref name="minNegativeValue"/> to read it back.
        /// </summary>
        /// <param name="value">A 32-bit integer (greater or equal to <param name="minNegativeValue"/>).</param>
        /// <param name="minNegativeValue">Lowest possible negative value.</param>
        /// <remarks>
        /// <para>
        /// Writing a negative value lower than the <paramref name="minNegativeValue"/> is totally possible, however
        /// more than 4 bytes will be required for them.
        /// </para>
        /// <para>
        ///The default value of -1 is perfect to write small integers that are greater or equal to -1.
        /// </para>
        /// </remarks>
        public void WriteSmallInt32( int value, int minNegativeValue = -1 ) => Write7BitEncodedInt( value - minNegativeValue );

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
