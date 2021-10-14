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
    /// Immutable SHA512 value. It is a wrapper around a 64 bytes array and its string representation.
    /// Default value is <see cref="ZeroSHA512"/>.
    /// </summary>
    public readonly struct SHA512Value : IEquatable<SHA512Value>, IComparable<SHA512Value>
    {
        /// <summary>
        /// The "zero" SHA512 (64 bytes full of zeros).
        /// This is the default value of a new SHA512Value().
        /// </summary>
        public static readonly SHA512Value ZeroSHA512;

        /// <summary>
        /// The empty SHA512 is the actual SHA512 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA512 algorithm).
        /// </summary>
        public static readonly SHA512Value EmptySHA512;

        /// <summary>
        /// Computes the SHA512 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA512 of the file.</returns>
        public static SHA512Value ComputeFileSHA512( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new SHA512Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// Computes the SHA512 of a raw byte array.
        /// </summary>
        /// <param name="data">Byte array. Can be null.</param>
        /// <returns>The SHA512 of the data: <see cref="EmptySHA512"/> if data is null or empty.</returns>
        public static SHA512Value ComputeSHA512( byte[] data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA512;
            using( var n = new SHA512Managed() )
            {
                return new SHA512Value( n.ComputeHash( data ) );
            }
        }

        /// <summary>
        /// Computes the SHA512 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA512 of the data: <see cref="EmptySHA512"/> if data is null or empty.</returns>
        public static SHA512Value ComputeSHA512( string data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA512;
            return ComputeSHA512( Encoding.Default.GetBytes( data ) );
        }

        /// <summary>
        /// Computes the SHA512 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA512 of the file.</returns>
        public static async Task<SHA512Value> ComputeFileSHA512Async( string fullPath, Func<Stream,Stream>? wrapReader = null )
        {
            using( var shaCompute = new SHA512Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// A SHA512 is a non null 64 bytes long array.
        /// </summary>
        /// <param name="SHA512">The potential SHA512.</param>
        /// <returns>True when 64 bytes long array, false otherwise.</returns>
        public static bool IsValidSHA512( IReadOnlyList<byte> SHA512 )
        {
            return SHA512 != null && SHA512.Count == 64;
        }

        /// <summary>
        /// Parse a 128 length hexadecimal string to a SHA512 value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <returns>The value.</returns>
        public static SHA512Value Parse( string s, int offset = 0 )
        {
            SHA512Value v;
            if( !TryParse( s, offset, out v ) ) throw new ArgumentException( "Invalid SHA512.", nameof(s) );
            return v;
        }

        /// <summary>
        /// Tries to parse a 128 length hexadecimal string to a SHA512 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <param name="value">The value on success, <see cref="ZeroSHA512"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( string s, int offset, out SHA512Value value )
        {
            value = ZeroSHA512;
            if( s == null || offset + 128 > s.Length ) return false;
            bool zero = true;
            byte[] b = new byte[64];
            for( int i = 0; i < 128; ++i )
            {
                int vH = s[offset + i].HexDigitValue();
                if( vH == -1 ) return false;
                if( vH != 0 ) zero = false;
                int vL = s[offset + ++i].HexDigitValue();
                if( vL == -1 ) return false;
                if( vL != 0 ) zero = false;
                b[i >> 1] = (byte)(vH << 4 | vL);
            }
            if( !zero ) value = new SHA512Value( b, BuildString( b ) );
            return true;
        }

        /// <summary>
        /// Defines equality operator.
        /// </summary>
        /// <param name="x">First SHA512.</param>
        /// <param name="y">Second SHA512.</param>
        /// <returns>True if x equals y, otherwise false.</returns>
        static public bool operator ==( SHA512Value x, SHA512Value y ) => x.Equals( y );

        /// <summary>
        /// Defines inequality operator.
        /// </summary>
        /// <param name="x">First SHA512.</param>
        /// <param name="y">Second SHA512.</param>
        /// <returns>True if x is not equal to y, otherwise false.</returns>
        static public bool operator !=( SHA512Value x, SHA512Value y ) => !x.Equals( y );

        static SHA512Value()
        {
            ZeroSHA512 = new SHA512Value( true );
            var emptyBytes = new byte[] { 207, 131, 225, 53, 126, 239, 184, 189, 241, 84, 40, 80, 214, 109, 128, 7, 214, 32, 228, 5, 11, 87, 21, 220, 131, 244, 169, 33, 211, 108, 233, 206, 71, 208, 209, 60, 93, 133, 242, 176, 255, 131, 24, 210, 135, 126, 236, 47, 99, 185, 49, 189, 71, 65, 122, 129, 165, 56, 50, 122, 249, 39, 218, 62 };
            EmptySHA512 = new SHA512Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            using( var h = new SHA512Managed() )
            {
                Debug.Assert( h.ComputeHash( Array.Empty<byte>() ).SequenceEqual( EmptySHA512._bytes ) );
            }
#endif
        }


        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// Initializes a new <see cref="SHA512Value"/> from its 20 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA512Value( IReadOnlyList<byte> twentyBytes )
        {
            if( !IsValidSHA512( twentyBytes ) ) throw new ArgumentException( "Invalid SHA512.", nameof( twentyBytes ) );
            if( twentyBytes.SequenceEqual( ZeroSHA512._bytes ) )
            {
                _bytes = ZeroSHA512._bytes;
                _string = ZeroSHA512._string;
            }
            else
            {
                _bytes = twentyBytes.ToArray();
                _string = BuildString( _bytes );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SHA512Value"/> from a binary reader.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        public SHA512Value( BinaryReader reader )
        {
            _bytes = reader.ReadBytes( 64 );
            if( _bytes.Length < 64 ) throw new EndOfStreamException( $"Expected SHA512 (64 bytes). Got only {_bytes.Length} bytes." );
            if( _bytes.SequenceEqual( ZeroSHA512._bytes ) )
            {
                _bytes = ZeroSHA512._bytes;
                _string = ZeroSHA512._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA512Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 64 );
            if( b.SequenceEqual( ZeroSHA512._bytes ) )
            {
                _bytes = ZeroSHA512._bytes;
                _string = ZeroSHA512._string;
            }
            else
            {
                _bytes = b;
                _string = BuildString( b );
            }
        }

        SHA512Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 64 && !b.SequenceEqual( ZeroSHA512._bytes ) && s != null && s.Length == 128 );
            _bytes = b;
            _string = s;
        }

        SHA512Value( bool forZeroSHA512Only )
        {
            _bytes = new byte[64];
            _string = new string( '0', 128 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="ZeroSHA512"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == ZeroSHA512._bytes;

        /// <summary>
        /// Tests whether this SHA512 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA512Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA512Value other )
        {
            return _bytes == other._bytes 
                    || (_bytes == null && other._bytes == ZeroSHA512._bytes)
                    || (_bytes == ZeroSHA512._bytes && other._bytes == null)
                    || _bytes.SequenceEqual( other._bytes );
        }

        /// <summary>
        /// Gets the SHA512 as a 64 bytes read only list.
        /// </summary>
        /// <returns>The SHA512 bytes.</returns>
        public IReadOnlyList<byte> GetBytes() => _bytes ?? ZeroSHA512._bytes;

        /// <summary>
        /// Writes this SHA512 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Target binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( _bytes ?? ZeroSHA512._bytes );

        /// <summary>
        /// Overridden to test actual SHA512 equality.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>True if other is a SHA512Value with the same value, false otherwise.</returns>
        public override bool Equals( object? obj ) => obj is SHA512Value s && Equals( s );

        /// <summary>
        /// Gets the hash code of this SHA512.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _bytes == null ? 0 : (_bytes[0] << 24) | (_bytes[1] << 16) | (_bytes[2] << 8) | _bytes[3];

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA512 as a string.</returns>
        public override string ToString() => _string ?? ZeroSHA512._string;
        
        static string BuildString( byte[] b )
        { 
            Debug.Assert( b != null && b != ZeroSHA512._bytes );
            char[] a = new char[128];
            for( int i = 0; i < 128; )
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
        /// <param name="other">The other SHA512 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA512Value other )
        {
            if( _bytes == other._bytes ) return 0;
            if( _bytes == null || _bytes == ZeroSHA512._bytes )
                return other._bytes != null && other._bytes != ZeroSHA512._bytes
                        ? -1
                        : 0;
            if( other._bytes == null || other._bytes == ZeroSHA512._bytes ) return +1;
            for( int i = 0; i < _bytes.Length; ++i )
            {
                int cmp = _bytes[i] - other._bytes[i];
                if( cmp != 0 ) return cmp;
            }
            return 0;
        }
    }
}
