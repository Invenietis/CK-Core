using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Immutable SHA512 value. It is a wrapper around a 64 bytes array and its string representation.
    /// Default value is <see cref="Zero"/>.
    /// </summary>
    public readonly struct SHA512Value : IEquatable<SHA512Value>, IComparable<SHA512Value>
    {
        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// The "zero" SHA512 (64 bytes full of zeros).
        /// This is the default value of a new SHA512Value().
        /// </summary>
        public static readonly SHA512Value Zero;

        /// <summary>
        /// The empty SHA512 is the actual SHA512 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA512 algorithm).
        /// </summary>
        public static readonly SHA512Value Empty;

        /// <summary>
        /// Computes the SHA512 of a raw byte array.
        /// </summary>
        /// <param name="data">Bytes to compute. Can be null.</param>
        /// <returns>The SHA512 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA512Value ComputeHash( ReadOnlySpan<byte> data )
        {
            if( data.Length == 0 ) return Empty;
            return new SHA512Value( SHA512.HashData( data ) );
        }

        /// <summary>
        /// Computes the SHA512 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA512 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA512Value ComputeHash( string? data )
        {
            if( data == null || data.Length == 0 ) return Empty;
            var bytes = System.Runtime.InteropServices.MemoryMarshal.Cast<char, byte>( data.AsSpan() );
            return ComputeHash( bytes );
        }

        /// <summary>
        /// Computes the SHA512 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader. If not null, the hash is computed on its output.</param>
        /// <returns>The SHA512 of the file.</returns>
        public static async Task<SHA512Value> ComputeFileHashAsync( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA512 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return new SHA512Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// Computes the SHA512 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA512 of the file.</returns>
        public static SHA512Value ComputeFileHash( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA512 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return new SHA512Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// A SHA512 is 64 bytes long.
        /// </summary>
        /// <param name="sha512">The potential SHA512.</param>
        /// <returns>True when 64 bytes long, false otherwise.</returns>
        public static bool IsValid( ReadOnlySpan<byte> sha512 ) => sha512.Length == 64;

        /// <summary>
        /// Parses a 128 length hexadecimal string to a SHA512 value or throws a <see cref="FormatException"/>.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <returns>The value.</returns>
        public static SHA512Value Parse( ReadOnlySpan<char> text )
        {
            if( !TryParse( text, out var result ) ) throw new FormatException( "Invalid SHA512" );
            return result;
        }

        /// <summary>
        /// Tries to parse a 40 length hexadecimal string to a SHA512 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="value">The value on success, <see cref="Zero"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( ReadOnlySpan<char> text, out SHA512Value value )
        {
            value = Zero;
            if( text.Length < 128 ) return false;
            try
            {
                var bytes = Convert.FromHexString( text.Slice( 0, 128 ) );
                value = new SHA512Value( bytes );
                return true;
            }
            catch( FormatException )
            {
                return false;
            }
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
            Zero = new SHA512Value( true );
            var emptyBytes = new byte[] { 207, 131, 225, 53, 126, 239, 184, 189, 241, 84, 40, 80, 214, 109, 128, 7, 214, 32, 228, 5, 11, 87, 21, 220, 131, 244, 169, 33, 211, 108, 233, 206, 71, 208, 209, 60, 93, 133, 242, 176, 255, 131, 24, 210, 135, 126, 236, 47, 99, 185, 49, 189, 71, 65, 122, 129, 165, 56, 50, 122, 249, 39, 218, 62 };
            Empty = new SHA512Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            Debug.Assert( SHA512.HashData( ReadOnlySpan<byte>.Empty ).AsSpan().SequenceEqual( Empty._bytes ) );
#endif
        }

        /// <summary>
        /// Initializes a new <see cref="SHA512Value"/> from a read only 64 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA512Value( ReadOnlySpan<byte> twentyBytes )
        {
            if( twentyBytes.Length != 64 ) throw new ArgumentException( $"SHA512 is 64 bytes long, not {twentyBytes.Length}.", nameof( twentyBytes ) );
            if( twentyBytes.SequenceEqual( Zero._bytes.AsSpan() ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
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
            if( _bytes.SequenceEqual( Zero._bytes ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA512Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 64 );
            if( b.SequenceEqual( Zero._bytes ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
            }
            else
            {
                _bytes = b;
                _string = BuildString( b );
            }
        }

        SHA512Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 64 && !b.SequenceEqual( Zero._bytes ) && s != null && s.Length == 128 );
            _bytes = b;
            _string = s;
        }

        SHA512Value( bool forZeroSHA512Only )
        {
            _bytes = new byte[20];
            _string = new string( '0', 40 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="Zero"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == Zero._bytes;

        /// <summary>
        /// Tests whether this SHA512 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA512Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA512Value other )
        {
            return _bytes == other._bytes
                    || (_bytes == null && other._bytes == Zero._bytes)
                    || (_bytes == Zero._bytes && other._bytes == null)
                    || (_bytes != null && _bytes.SequenceEqual( other._bytes ));
        }

        /// <summary>
        /// Gets the SHA512 as a 64 bytes read only memory.
        /// </summary>
        /// <returns>The SHA512 bytes.</returns>
        public ReadOnlyMemory<byte> GetBytes() => _bytes ?? Zero._bytes;

        /// <summary>
        /// Writes this SHA512 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Target binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( GetBytes().Span );

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
        public override int GetHashCode() => HashCode.Combine( _bytes );

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA512 as a string.</returns>
        public override string ToString() => _string ?? Zero._string;

        static string BuildString( byte[] b )
        {
            Debug.Assert( !b.AsSpan().SequenceEqual( Zero._bytes.AsSpan() ) );
            return Convert.ToHexString( b ).ToLowerInvariant();
        }

        /// <summary>
        /// Compares this value to another one.
        /// </summary>
        /// <param name="other">The other SHA512 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA512Value other )
        {
            if( _bytes == other._bytes ) return 0;
            if( _bytes == null || _bytes == Zero._bytes )
                return other._bytes != null && other._bytes != Zero._bytes
                        ? -1
                        : 0;
            if( other._bytes == null || other._bytes == Zero._bytes ) return +1;
            for( int i = 0; i < _bytes.Length; ++i )
            {
                int cmp = _bytes[i] - other._bytes[i];
                if( cmp != 0 ) return cmp;
            }
            return 0;
        }
    }
}
