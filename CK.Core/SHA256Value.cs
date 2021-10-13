using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CK.Text;
using System.Threading.Tasks;
using System.Text;

namespace CK.Core
{

    /// <summary>
    /// Immutable SHA256 value. It is a wrapper around a 32 bytes array and its string representation.
    /// Default value is <see cref="ZeroSHA256"/>.
    /// </summary>
    public struct SHA256Value : IEquatable<SHA256Value>, IComparable<SHA256Value>
    {
        /// <summary>
        /// The "zero" SHA256 (32 bytes full of zeros).
        /// This is the default value of a new SHA256Value().
        /// </summary>
        public static readonly SHA256Value ZeroSHA256;

        /// <summary>
        /// The empty SHA256 is the actual SHA256 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA256 algorithm).
        /// </summary>
        public static readonly SHA256Value EmptySHA256;

        /// <summary>
        /// Computes the SHA256 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA256 of the file.</returns>
        public static SHA256Value ComputeFileSHA256( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new SHA256Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// Computes the SHA256 of a raw byte array.
        /// </summary>
        /// <param name="data">Byte array. Can be null.</param>
        /// <returns>The SHA256 of the data: <see cref="EmptySHA256"/> if data is null or empty.</returns>
        public static SHA256Value ComputeSHA256( byte[] data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA256;
            using( var n = new SHA256Managed() )
            {
                return new SHA256Value( n.ComputeHash( data ) );
            }
        }

        /// <summary>
        /// Computes the SHA256 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA256 of the data: <see cref="EmptySHA256"/> if data is null or empty.</returns>
        public static SHA256Value ComputeSHA256( string data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA256;
            return ComputeSHA256( Encoding.Default.GetBytes( data ) );
        }

        /// <summary>
        /// Computes the SHA256 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA256 of the file.</returns>
        public static async Task<SHA256Value> ComputeFileSHA256Async( string fullPath, Func<Stream,Stream>? wrapReader = null )
        {
            using( var shaCompute = new SHA256Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// A SHA256 is a non null 32 bytes long array.
        /// </summary>
        /// <param name="SHA256">The potential SHA256.</param>
        /// <returns>True when 32 bytes long array, false otherwise.</returns>
        public static bool IsValidSHA256( IReadOnlyList<byte> SHA256 )
        {
            return SHA256 != null && SHA256.Count == 32;
        }

        /// <summary>
        /// Parse a 64 length hexadecimal string to a SHA256 value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <returns>The value.</returns>
        public static SHA256Value Parse( string s, int offset = 0 )
        {
            SHA256Value v;
            if( !TryParse( s, offset, out v ) ) throw new ArgumentException( "Invalid SHA256.", nameof(s) );
            return v;
        }

        /// <summary>
        /// Tries to parse a 64 length hexadecimal string to a SHA256 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <param name="value">The value on success, <see cref="ZeroSHA256"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( string s, int offset, out SHA256Value value )
        {
            value = ZeroSHA256;
            if( s == null || offset + 64 > s.Length ) return false;
            bool zero = true;
            byte[] b = new byte[32];
            for( int i = 0; i < 64; ++i )
            {
                int vH = s[offset + i].HexDigitValue();
                if( vH == -1 ) return false;
                if( vH != 0 ) zero = false;
                int vL = s[offset + ++i].HexDigitValue();
                if( vL == -1 ) return false;
                if( vL != 0 ) zero = false;
                b[i >> 1] = (byte)(vH << 4 | vL);
            }
            if( !zero ) value = new SHA256Value( b, BuildString( b ) );
            return true;
        }

        /// <summary>
        /// Defines equality operator.
        /// </summary>
        /// <param name="x">First SHA256.</param>
        /// <param name="y">Second SHA256.</param>
        /// <returns>True if x equals y, otherwise false.</returns>
        static public bool operator ==( SHA256Value x, SHA256Value y ) => x.Equals( y );

        /// <summary>
        /// Defines inequality operator.
        /// </summary>
        /// <param name="x">First SHA256.</param>
        /// <param name="y">Second SHA256.</param>
        /// <returns>True if x is not equal to y, otherwise false.</returns>
        static public bool operator !=( SHA256Value x, SHA256Value y ) => !x.Equals( y );

        static SHA256Value()
        {
            ZeroSHA256 = new SHA256Value( true );
            var emptyBytes = new byte[] { 227, 176 ,196 ,66, 152, 252, 28, 20, 154, 251, 244, 200, 153, 111, 185, 36, 39, 174, 65, 228, 100, 155, 147, 76, 164, 149, 153, 27, 120, 82, 184, 85 };
            EmptySHA256 = new SHA256Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            using( var h = new SHA256Managed() )
            {
                Debug.Assert( h.ComputeHash( Array.Empty<byte>() ).SequenceEqual( EmptySHA256._bytes ) );
            }
#endif
        }


        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// Initializes a new <see cref="SHA256Value"/> from its 20 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA256Value( IReadOnlyList<byte> twentyBytes )
        {
            if( !IsValidSHA256( twentyBytes ) ) throw new ArgumentException( "Invalid SHA256.", nameof( twentyBytes ) );
            if( twentyBytes.SequenceEqual( ZeroSHA256._bytes ) )
            {
                _bytes = ZeroSHA256._bytes;
                _string = ZeroSHA256._string;
            }
            else
            {
                _bytes = twentyBytes.ToArray();
                _string = BuildString( _bytes );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SHA256Value"/> from a binary reader.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        public SHA256Value( BinaryReader reader )
        {
            _bytes = reader.ReadBytes( 32 );
            if( _bytes.Length < 32 ) throw new EndOfStreamException( $"Expected SHA256 (32 bytes). Got only {_bytes.Length} bytes." );
            if( _bytes.SequenceEqual( ZeroSHA256._bytes ) )
            {
                _bytes = ZeroSHA256._bytes;
                _string = ZeroSHA256._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA256Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 32 );
            if( b.SequenceEqual( ZeroSHA256._bytes ) )
            {
                _bytes = ZeroSHA256._bytes;
                _string = ZeroSHA256._string;
            }
            else
            {
                _bytes = b;
                _string = BuildString( b );
            }
        }

        SHA256Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 32 && !b.SequenceEqual( ZeroSHA256._bytes ) && s != null && s.Length == 64 );
            _bytes = b;
            _string = s;
        }

        SHA256Value( bool forZeroSHA256Only )
        {
            _bytes = new byte[32];
            _string = new string( '0', 32 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="ZeroSHA256"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == ZeroSHA256._bytes;

        /// <summary>
        /// Tests whether this SHA256 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA256Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA256Value other )
        {
            return _bytes == other._bytes 
                    || (_bytes == null && other._bytes == ZeroSHA256._bytes)
                    || (_bytes == ZeroSHA256._bytes && other._bytes == null)
                    || _bytes.SequenceEqual( other._bytes );
        }

        /// <summary>
        /// Gets the SHA256 as a 32 bytes read only list.
        /// </summary>
        /// <returns>The SHA256 bytes.</returns>
        public IReadOnlyList<byte> GetBytes() => _bytes ?? ZeroSHA256._bytes;

        /// <summary>
        /// Writes this SHA256 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Target binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( _bytes ?? ZeroSHA256._bytes );

        /// <summary>
        /// Overridden to test actual SHA256 equality.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>True if other is a SHA256Value with the same value, false otherwise.</returns>
        public override bool Equals( object? obj ) => obj is SHA256Value s && Equals( s );

        /// <summary>
        /// Gets the hash code of this SHA256.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _bytes == null ? 0 : (_bytes[0] << 24) | (_bytes[1] << 16) | (_bytes[2] << 8) | _bytes[3];

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA256 as a string.</returns>
        public override string ToString() => _string ?? ZeroSHA256._string;
        
        static string BuildString( byte[] b )
        { 
            Debug.Assert( b != null && b != ZeroSHA256._bytes );
            char[] a = new char[64];
            for( int i = 0; i < 64; )
            {
                byte x = b[i>>1];
                a[i++] = GetHexValue( x >> 4 );
                a[i++] = GetHexValue( x & 15 );
            }
            return new string( a );
        }

        static char GetHexValue( int i ) => i < 10 ? (char)(i + '0') : (char)(i - 10 + 'a');

        /// <summary>
        /// Compares this value to another one.
        /// </summary>
        /// <param name="other">The other SHA256 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA256Value other )
        {
            if( _bytes == other._bytes ) return 0;
            if( _bytes == null || _bytes == ZeroSHA256._bytes )
                return other._bytes != null && other._bytes != ZeroSHA256._bytes
                        ? -1
                        : 0;
            if( other._bytes == null || other._bytes == ZeroSHA256._bytes ) return +1;
            for( int i = 0; i < _bytes.Length; ++i )
            {
                int cmp = _bytes[i] - other._bytes[i];
                if( cmp != 0 ) return cmp;
            }
            return 0;
        }
    }
}
