using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Toolkit.Diagnostics;

namespace CK.Core
{

    /// <summary>
    /// Immutable SHA1 value. It is a wrapper around a 20 bytes array and its string representation.
    /// Default value is <see cref="Zero"/>.
    /// </summary>
    public readonly struct SHA1Value : IEquatable<SHA1Value>, IComparable<SHA1Value>
    {
        readonly byte[] _bytes;
        readonly string _string;

        /// <summary>
        /// The "zero" SHA1 (20 bytes full of zeros).
        /// This is the default value of a new SHA1Value().
        /// </summary>
        public static readonly SHA1Value Zero;

        /// <summary>
        /// The empty SHA1 is the actual SHA1 of no bytes at all (it corresponds 
        /// to the internal initial values of the SHA1 algorithm).
        /// </summary>
        public static readonly SHA1Value Empty;

        /// <summary>
        /// Computes the SHA1 of a raw byte array.
        /// </summary>
        /// <param name="data">Bytes to compute. Can be null.</param>
        /// <returns>The SHA1 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA1Value ComputeHash( ReadOnlySpan<byte> data )
        {
            if( data.Length == 0 ) return Empty;
            return new SHA1Value( SHA1.HashData( data ) );
        }

        /// <summary>
        /// Computes the SHA1 of a string (using <see cref="Encoding.Default"/>).
        /// </summary>
        /// <param name="data">String data. Can be null.</param>
        /// <returns>The SHA1 of the data: <see cref="Empty"/> if data is null or empty.</returns>
        public static SHA1Value ComputeHash( string? data )
        {
            if( data == null || data.Length == 0 ) return Empty;
            var bytes = System.Runtime.InteropServices.MemoryMarshal.Cast<char, byte>( data.AsSpan() );
            return ComputeHash( bytes );
        }

        /// <summary>
        /// Computes the SHA1 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader. If not null, the hash is computed on its output.</param>
        /// <returns>The SHA1 of the file.</returns>
        public static async Task<SHA1Value> ComputeFileHashAsync( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA1 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                await wrap.CopyToAsync( shaCompute );
                return new SHA1Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// Computes the SHA1 of a local file by reading its content.
        /// </summary>
        /// <param name="fullPath">The file full path.</param>
        /// <param name="wrapReader">Optional stream wrapper reader.</param>
        /// <returns>The SHA1 of the file.</returns>
        public static SHA1Value ComputeFileHash( string fullPath, Func<Stream, Stream>? wrapReader = null )
        {
            using( var shaCompute = new HashStream( HashAlgorithmName.SHA1 ) )
            using( var file = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var wrap = wrapReader != null ? wrapReader( file ) : file )
            {
                wrap.CopyTo( shaCompute );
                return new SHA1Value( shaCompute.GetFinalResult() );
            }
        }

        /// <summary>
        /// A SHA1 is 20 bytes long.
        /// </summary>
        /// <param name="sha1">The potential sha1.</param>
        /// <returns>True when 20 bytes long, false otherwise.</returns>
        public static bool IsValid( ReadOnlySpan<byte> sha1 ) => sha1.Length == 20;

        /// <summary>
        /// Parses a 40 length hexadecimal string to a SHA1 value or throws a <see cref="FormatException"/>.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <returns>The value.</returns>
        public static SHA1Value Parse( ReadOnlySpan<char> text )
        {
            if( !TryParse( text, out var result ) ) ThrowHelper.ThrowArgumentException( nameof( text ), "Invalid SHA1." );
            return result;
        }

        /// <summary>
        /// Tries to parse a 40 length hexadecimal string to a SHA1 value.
        /// The string can be longer, suffix is ignored.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="value">The value on success, <see cref="Zero"/> on error.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool TryParse( ReadOnlySpan<char> text, out SHA1Value value )
        {
            value = Zero;
            if( text.Length < 40 ) return false;
            try
            {
                var bytes = Convert.FromHexString( text.Slice( 0, 40 ) );
                value = new SHA1Value( bytes );
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
        /// <param name="x">First sha1.</param>
        /// <param name="y">Second sha1.</param>
        /// <returns>True if x equals y, otherwise false.</returns>
        static public bool operator ==( SHA1Value x, SHA1Value y ) => x.Equals( y );

        /// <summary>
        /// Defines inequality operator.
        /// </summary>
        /// <param name="x">First sha1.</param>
        /// <param name="y">Second sha1.</param>
        /// <returns>True if x is not equal to y, otherwise false.</returns>
        static public bool operator !=( SHA1Value x, SHA1Value y ) => !x.Equals( y );

        static SHA1Value()
        {
            Zero = new SHA1Value( true );
            var emptyBytes = new byte[] { 0xDA, 0x39, 0xA3, 0xEE, 0x5E, 0x6B, 0x4B, 0x0D, 0x32, 0x55, 0xBF, 0xEF, 0x95, 0x60, 0x18, 0x90, 0xAF, 0xD8, 0x07, 0x09 };
            Empty = new SHA1Value( emptyBytes, BuildString( emptyBytes ) );
#if DEBUG
            Debug.Assert( SHA1.HashData( ReadOnlySpan<byte>.Empty ).AsSpan().SequenceEqual( Empty._bytes ) );
#endif
        }

        /// <summary>
        /// Initializes a new <see cref="SHA1Value"/> from a read only 20 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA1Value( ReadOnlySpan<byte> twentyBytes )
        {
            if( twentyBytes.Length != 20 ) ThrowHelper.ThrowArgumentException( nameof( twentyBytes ), $"SHA1 is 20 bytes long, not {twentyBytes.Length}." );
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
        /// Initializes a new <see cref="SHA1Value"/> from a read only 20 bytes value.
        /// </summary>
        /// <param name="twentyBytes">Binary values.</param>
        public SHA1Value( ReadOnlyMemory<byte> twentyBytes )
        {
            if( twentyBytes.Length != 20 ) throw new ArgumentException( $"SHA1 is 20 bytes long, not {twentyBytes.Length}.", nameof( twentyBytes ) );
            if( twentyBytes.Span.SequenceEqual( Zero._bytes.AsSpan() ) )
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
        /// Initializes a new <see cref="SHA1Value"/> from a binary reader.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        public SHA1Value( BinaryReader reader )
        {
            _bytes = reader.ReadBytes( 20 );
            if( _bytes.Length < 20 ) throw new EndOfStreamException( $"Expected SHA1 (20 bytes). Got only {_bytes.Length} bytes." );
            if( _bytes.SequenceEqual( Zero._bytes ) )
            {
                _bytes = Zero._bytes;
                _string = Zero._string;
            }
            else _string = BuildString( _bytes );
        }

        internal SHA1Value( byte[] b )
        {
            Debug.Assert( b != null && b.Length == 20 );
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

        SHA1Value( byte[] b, string s )
        {
            Debug.Assert( b.Length == 20 && !b.SequenceEqual( Zero._bytes ) && s != null && s.Length == 40 );
            _bytes = b;
            _string = s;
        }

        SHA1Value( bool forZeroSha1Only )
        {
            _bytes = new byte[20];
            _string = new string( '0', 40 );
        }

        /// <summary>
        /// Gets whether this is a <see cref="Zero"/>.
        /// </summary>
        public bool IsZero => _bytes == null || _bytes == Zero._bytes;

        /// <summary>
        /// Tests whether this SHA1 is the same as the other one.
        /// </summary>
        /// <param name="other">Other SHA1Value.</param>
        /// <returns>True if other has the same value, false otherwise.</returns>
        public bool Equals( SHA1Value other )
        {
            return _bytes == other._bytes
                    || (_bytes == null && other._bytes == Zero._bytes)
                    || (_bytes == Zero._bytes && other._bytes == null)
                    || (_bytes != null && _bytes.SequenceEqual( other._bytes ));
        }

        /// <summary>
        /// Gets the SHA1 as a 20 bytes read only memory.
        /// </summary>
        /// <returns>The sha1 bytes.</returns>
        public ReadOnlyMemory<byte> GetBytes() => _bytes ?? Zero._bytes;

        /// <summary>
        /// Writes this SHA1 value in a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">Target binary writer.</param>
        public void Write( BinaryWriter w ) => w.Write( GetBytes().Span );

        /// <summary>
        /// Overridden to test actual SHA1 equality.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>True if other is a SHA1Value with the same value, false otherwise.</returns>
        public override bool Equals( object? obj ) => obj is SHA1Value s && Equals( s );

        /// <summary>
        /// Gets the hash code of this SHA1.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine( _bytes );

        /// <summary>
        /// Returns the 40 hexadecimal characters string.
        /// </summary>
        /// <returns>The SHA1 as a string.</returns>
        public override string ToString() => _string ?? Zero._string;
        
        static string BuildString( byte[] b )
        { 
            Debug.Assert( !b.AsSpan().SequenceEqual( Zero._bytes.AsSpan() ) );
            return Convert.ToHexString( b ).ToLowerInvariant();
        }

        /// <summary>
        /// Compares this value to another one.
        /// </summary>
        /// <param name="other">The other SHA1 to compare.</param>
        /// <returns>The standard positive value if this is greater than other, 0 if they are equal and a negative value otherwise.</returns>
        public int CompareTo( SHA1Value other )
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
