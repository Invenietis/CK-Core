using Microsoft.Toolkit.Diagnostics;
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
    /// Immutable SHA256 value. It is a wrapper around a 32 bytes array and its string representation.
    /// Default value is <see cref="Zero"/>.
    /// </summary>
    public readonly struct SHA256Value : IEquatable<SHA256Value>, IComparable<SHA256Value>
    {
        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// The "zero" SHA256 (32 bytes full of zeros).
        /// This is the default value of a new SHA256Value().
        /// </summary>
        public static readonly SHA256Value Zero;

        /// <summary>
        /// The empty SHA256 is the actual SHA256 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA256 algorithm).
        /// </summary>
        public static readonly SHA256Value Empty;

        /// <summary>
        /// Computes the SHA256 of a raw byte array.
        /// </summary>
        /// <param name="data">Bytes to compute. Can be null.</param>
        /// <returns>The SHA256 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA256Value ComputeHash( ReadOnlySpan<byte> data )
        {
            if( data.Length == 0 ) return Empty;
            return new SHA256Value( SHA256.HashData( data ) );
        }

        /// <summary>
        /// Computes the SHA256 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA256 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA256Value ComputeHash( string? data )
        {
            if( data == null || data.Length == 0 ) return Empty;
            var bytes = System.Runtime.InteropServices.MemoryMarshal.Cast<char, byte>( data.AsSpan() );
            return ComputeHash( bytes );
        }

        /// <summary>
        /// Computes the SHA256 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader. If not null, the hash is computed on its output.</param>
        /// <returns>The SHA256 of the file.</returns>
        public static async Task<SHA256Value> ComputeFileHashAsync( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA256 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return new SHA256Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// Computes the SHA256 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA256 of the file.</returns>
        public static SHA256Value ComputeFileHash( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA256 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return new SHA256Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// A SHA256 is 32 bytes long.
        /// </summary>
        /// <param name="sha256">The potential SHA256.</param>
        /// <returns>True when 32 bytes long, false otherwise.</returns>
        public static bool IsValid( ReadOnlySpan<byte> sha256 ) => sha256.Length == 32;

        /// <summary>
        /// Parses a 64 length hexadecimal string to a SHA256 value or throws a <see cref="FormatException"/>.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <returns>The value.</returns>
        public static SHA256Value Parse( ReadOnlySpan<char> text )
        {
            if( !TryParse( text, out var result ) ) ThrowHelper.ThrowArgumentException( nameof( text ), "Invalid SHA256." );
            return result;
        }

        /// <summary>
        /// Tries to parse a 64 length hexadecimal string to a SHA256 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="value">The value on success, <see cref="Zero"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( ReadOnlySpan<char> text, out SHA256Value value )
        {
            value = Zero;
            if( text.Length < 64 ) return false;
            try
            {
                var bytes = Convert.FromHexString( text.Slice( 0, 64 ) );
                value = new SHA256Value( bytes );
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
            Zero = new SHA256Value( true );
            var emptyBytes = new byte[] { 227, 176, 196, 66, 152, 252, 28, 20, 154, 251, 244, 200, 153, 111, 185, 36, 39, 174, 65, 228, 100, 155, 147, 76, 164, 149, 153, 27, 120, 82, 184, 85 };
            Empty = new SHA256Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            Debug.Assert( SHA256.HashData( ReadOnlySpan<byte>.Empty ).AsSpan().SequenceEqual( Empty._bytes ) );
#endif
        }

        /// <summary>
        /// Initializes a new <see cref="SHA256Value"/> from a read only 32 bytes value.
        /// </summary>
        /// <param name="thirtyTwoBytes">Binary values.</param>
        public SHA256Value( ReadOnlySpan<byte> thirtyTwoBytes )
        {
            if( thirtyTwoBytes.Length != 32 ) ThrowHelper.ThrowArgumentException( nameof( thirtyTwoBytes ), $"SHA256 is 32 bytes long, not {thirtyTwoBytes.Length}." );
            if( thirtyTwoBytes.SequenceEqual( Zero._bytes.AsSpan() ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
            }
            else
            {
                _bytes = thirtyTwoBytes.ToArray();
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
            if( _bytes.SequenceEqual( Zero._bytes ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA256Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 32 );
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

        SHA256Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 32 && !b.SequenceEqual( Zero._bytes ) && s != null && s.Length == 64 );
            _bytes = b;
            _string = s;
        }

        SHA256Value( bool forZeroSHA256Only )
        {
            _bytes = new byte[20];
            _string = new string( '0', 40 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="Zero"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == Zero._bytes;

        /// <summary>
        /// Tests whether this SHA256 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA256Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA256Value other )
        {
            return _bytes == other._bytes
                    || (_bytes == null && other._bytes == Zero._bytes)
                    || (_bytes == Zero._bytes && other._bytes == null)
                    || (_bytes != null && _bytes.SequenceEqual( other._bytes ));
        }

        /// <summary>
        /// Gets the SHA256 as a 32 bytes read only memory.
        /// </summary>
        /// <returns>The SHA256 bytes.</returns>
        public ReadOnlyMemory<byte> GetBytes() => _bytes ?? Zero._bytes;

        /// <summary>
        /// Writes this SHA256 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Target binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( GetBytes().Span );

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
        public override int GetHashCode() => HashCode.Combine( _bytes );

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA256 as a string.</returns>
        public override string ToString() => _string ?? Zero._string;

        static string BuildString( byte[] b )
        {
            Debug.Assert( !b.AsSpan().SequenceEqual( Zero._bytes.AsSpan() ) );
            return Convert.ToHexString( b ).ToLowerInvariant();
        }

        /// <summary>
        /// Compares this value to another one.
        /// </summary>
        /// <param name="other">The other SHA256 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA256Value other )
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
