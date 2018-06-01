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
    /// Immutable SHA1 value. It is a wrapper around a 20 bytes array and its string representation.
    /// Default value is <see cref="ZeroSHA1"/>.
    /// </summary>
    public struct SHA1Value : IEquatable<SHA1Value>, IComparable<SHA1Value>
    {
        /// <summary>
        /// The "zero" SHA1 (20 bytes full of zeroes).
        /// This is the default value of a new SHA1Value().
        /// </summary>
        public static readonly SHA1Value ZeroSHA1;

        /// <summary>
        /// The empty SHA1 is the actual SHA1 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA1 algorithm).
        /// </summary>
        public static readonly SHA1Value EmptySHA1;

        /// <summary>
        /// Computes the SHA1 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA1 of the file.</returns>
        public static SHA1Value ComputeFileSHA1( string fullPath, Func<Stream, Stream> wrapReader = null )
        {
            using( var shaCompute = new SHA1Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// Computes the SHA1 of a raw byte array.
        /// </summary>
        /// <param name="data">Byte array. Can be null.</param>
        /// <returns>The SHA1 of the data: <see cref="EmptySHA1"/> if data is null or empty.</returns>
        public static SHA1Value ComputeSHA1( byte[] data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA1;
            using( var n = new SHA1Managed() )
            {
                return new SHA1Value( n.ComputeHash( data ) );
            }
        }

        /// <summary>
        /// Computes the SHA1 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA1 of the data: <see cref="EmptySHA1"/> if data is null or empty.</returns>
        public static SHA1Value ComputeSHA1( string data )
        {
            if( data == null || data.Length == 0 ) return EmptySHA1;
            return ComputeSHA1( Encoding.Default.GetBytes( data ) );
        }

        /// <summary>
        /// Computes the SHA1 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA1 of the file.</returns>
        public static async Task<SHA1Value> ComputeFileSHA1Async( string fullPath, Func<Stream,Stream> wrapReader = null )
        {
            using( var shaCompute = new SHA1Stream() )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return shaCompute.GetFinalResult();
            }
        }

        /// <summary>
        /// A SHA1 is a non null 20 bytes long array.
        /// </summary>
        /// <param name="sha1">The potential sha1.</param>
        /// <returns>True when 20 bytes long array, false otherwise.</returns>
        public static bool IsValidSHA1( IReadOnlyList<byte> sha1 )
        {
            return sha1 != null && sha1.Count == 20;
        }

        /// <summary>
        /// Parse a 40 length hexadecimal string to a SHA1 value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <returns>The value.</returns>
        public static SHA1Value Parse( string s, int offset = 0 )
        {
            SHA1Value v;
            if( !TryParse( s, offset, out v ) ) throw new ArgumentException( "Invalid SHA1.", nameof(s) );
            return v;
        }

        /// <summary>
        /// Tries to parse a 40 length hexadecimal string to a SHA1 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="offset">The offset in the string.</param>
        /// <param name="value">The value on success, <see cref="ZeroSHA1"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( string s, int offset, out SHA1Value value )
        {
            value = ZeroSHA1;
            if( s == null || offset + 40 > s.Length ) return false;
            bool zero = true;
            byte[] b = new byte[20];
            for( int i = 0; i < 40; ++i )
            {
                int vH = s[offset + i].HexDigitValue();
                if( vH == -1 ) return false;
                if( vH != 0 ) zero = false;
                int vL = s[offset + ++i].HexDigitValue();
                if( vL == -1 ) return false;
                if( vL != 0 ) zero = false;
                b[i >> 1] = (byte)(vH << 4 | vL);
            }
            if( !zero ) value = new SHA1Value( b, BuildString( b ) );
            return true;
        }

        /// <summary>
        /// Defines equality operator.
        /// </summary>
        /// <param name="x">First sha1.</param>
        /// <param name="y">Second sha1.</param>
        /// <returns>True if x equals y, otherwise false.</returns>
        static public bool operator ==( SHA1Value x, SHA1Value y ) => x.Equals( y );

        /// <summary>
        /// Defines unequality operator.
        /// </summary>
        /// <param name="x">First sha1.</param>
        /// <param name="y">Second sha1.</param>
        /// <returns>True if x is not equal to y, otherwise false.</returns>
        static public bool operator !=( SHA1Value x, SHA1Value y ) => !x.Equals( y );

        static SHA1Value()
        {
            ZeroSHA1 = new SHA1Value( true );
            var emptyBytes = new byte[] { 0xDA, 0x39, 0xA3, 0xEE, 0x5E, 0x6B, 0x4B, 0x0D, 0x32, 0x55, 0xBF, 0xEF, 0x95, 0x60, 0x18, 0x90, 0xAF, 0xD8, 0x07, 0x09 };
            EmptySHA1 = new SHA1Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            using( var h = new SHA1Managed() )
            {
                Debug.Assert( h.ComputeHash( Array.Empty<byte>() ).SequenceEqual( EmptySHA1._bytes ) );
            }
#endif
        }


        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// Initializes a new <see cref="SHA1Value"/> from its 20 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA1Value( IReadOnlyList<byte> twentyBytes )
        {
            if( !IsValidSHA1( twentyBytes ) ) throw new ArgumentException( "Invalid SHA1.", nameof( twentyBytes ) );
            if( twentyBytes.SequenceEqual( ZeroSHA1._bytes ) )
            {
                _bytes = ZeroSHA1._bytes;
                _string = ZeroSHA1._string;
            }
            else
            {
                _bytes = twentyBytes.ToArray();
                _string = BuildString( _bytes );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SHA1Value"/> from a binary reader.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        public SHA1Value( BinaryReader reader )
        {
            _bytes = reader.ReadBytes( 20 );
            if( _bytes.Length < 20 ) throw new EndOfStreamException( $"Expected SHA1 (20 bytes). Got only {_bytes.Length} bytes." );
            if( _bytes.SequenceEqual( ZeroSHA1._bytes ) )
            {
                _bytes = ZeroSHA1._bytes;
                _string = ZeroSHA1._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA1Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 20 );
            if( b.SequenceEqual( ZeroSHA1._bytes ) )
            {
                _bytes = ZeroSHA1._bytes;
                _string = ZeroSHA1._string;
            }
            else
            {
                _bytes = b;
                _string = BuildString( b );
            }
        }

        SHA1Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 20 && !b.SequenceEqual( ZeroSHA1._bytes ) && s != null && s.Length == 40 );
            _bytes = b;
            _string = s;
        }

        SHA1Value( bool forZeroSha1Only )
        {
            _bytes = new byte[20];
            _string = new string( '0', 40 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="ZeroSHA1"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == ZeroSHA1._bytes;

        /// <summary>
        /// Tests whether this SHA1 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA1Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA1Value other )
        {
            return _bytes == other._bytes 
                    || (_bytes == null && other._bytes == ZeroSHA1._bytes)
                    || (_bytes == ZeroSHA1._bytes && other._bytes == null)
                    || _bytes.SequenceEqual( other._bytes );
        }

        /// <summary>
        /// Gets the SHA1 as a 20 bytes readonly list.
        /// </summary>
        /// <returns>The sha1 bytes.</returns>
        public IReadOnlyList<byte> GetBytes() => _bytes ?? ZeroSHA1._bytes;

        /// <summary>
        /// Writes this SHA1 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Targtet binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( _bytes ?? ZeroSHA1._bytes );

        /// <summary>
        /// Overridden to test actual SHA1 equality.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>True if other is a SHA1Value with the same value, false otherwise.</returns>
        public override bool Equals( object obj )
        {
            if( obj is SHA1Value ) return Equals( (SHA1Value)obj );
            return false;
        }

        /// <summary>
        /// Gets the hash code of this SHA1.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _bytes == null ? 0 : (_bytes[0] << 24) | (_bytes[1] << 16) | (_bytes[2] << 8) | _bytes[3];

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA1 as a string.</returns>
        public override string ToString() => _string ?? ZeroSHA1._string;
        
        static string BuildString( byte[] b )
        { 
            Debug.Assert( b != null && b != ZeroSHA1._bytes );
            char[] a = new char[40];
            for( int i = 0; i < 40; )
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
        /// <param name="other">The other SHA1 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA1Value other )
        {
            if( _bytes == other._bytes ) return 0;
            if( _bytes == null || _bytes == ZeroSHA1._bytes )
                return other._bytes != null && other._bytes != ZeroSHA1._bytes
                        ? -1
                        : 0;
            if( other._bytes == null || other._bytes == ZeroSHA1._bytes ) return +1;
            for( int i = 0; i < _bytes.Length; ++i )
            {
                int cmp = _bytes[i] - other._bytes[i];
                if( cmp != 0 ) return cmp;
            }
            return 0;
        }
    }
}
