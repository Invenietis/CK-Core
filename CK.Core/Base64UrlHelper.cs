using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Helper methods that uses <see cref="Base64"/> methods and maps the characters and remove padding.
    /// This should disappear sooner or later: https://github.com/dotnet/runtime/issues/1658
    /// </summary>
    public static class Base64UrlHelper
    {
        /// <summary>
        /// Encodes the span of binary data (in-place) into UTF-8 encoded text represented
        /// as Base64Url. The encoded text output is larger than the binary data contained
        /// in the input (the operation inflates the data).
        /// <para>
        /// This throws an <see cref="InvalidOperationException"/> if the buffer is too small.
        /// Use <see cref="Base64.GetMaxEncodedToUtf8Length(int)"/> to compute the output length.
        /// </para>
        /// </summary>
        /// <param name="buffer">
        /// The input span that contains binary data that needs to be encoded.
        /// Because the method performs an in-place conversion, it needs to be large enough to store the result of the operation.
        /// </param>
        /// <param name="dataLength">
        /// The number of bytes of binary data contained within the buffer that needs to be encoded.
        /// This value must be smaller than the buffer length.</param>
        /// <param name="bytesWritten">The number of bytes written into the buffer.</param>
        public static void Base64UrlEncodeToUtf8InPlaceNoPadding( Span<byte> buffer, int dataLength, out int bytesWritten )
        {
            var s = Base64.EncodeToUtf8InPlace( buffer, dataLength, out bytesWritten );
            if( s != OperationStatus.Done )
            {
                throw new InvalidOperationException( s.ToString() );
            }
            Base64ToUrlBase64NoPadding( buffer, ref bytesWritten );
        }

        /// <summary>
        /// Creates a string in Base64Url from bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase64UrlString( ReadOnlySpan<byte> bytes )
        {
            int size = Base64.GetMaxEncodedToUtf8Length( bytes.Length );
            var buffer = ArrayPool<byte>.Shared.Rent( size );
            try
            {
                Base64.EncodeToUtf8( bytes, buffer, out int _, out int bytesWritten );
                Base64ToUrlBase64NoPadding( buffer, ref bytesWritten );
                return Encoding.ASCII.GetString( buffer.AsSpan( 0, bytesWritten ) );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( buffer );
            }
        }

        /// <summary>
        /// Converts a base64Url string (that must be valid base64Url otherwise an <see cref="ArgumentException"/> is thrown)
        /// to bytes.
        /// </summary>
        /// <param name="base64UrlString">The string.</param>
        /// <returns>The bytes.</returns>
        public static Memory<byte> FromBase64UrlString( string base64UrlString )
        {
            Throw.CheckNotNullArgument( base64UrlString );
            var a = new byte[((base64UrlString.Length + 3) >> 2) << 2];
            int written = Encoding.ASCII.GetBytes( base64UrlString.AsSpan(), a );
            for( int i = 0; i < written; i++ )
            {
                switch( a[i] )
                {
                    case (byte)'-': a[i] = (byte)'+'; break;
                    case (byte)'_': a[i] = (byte)'/'; break;
                    case (byte)'+':
                    case (byte)'/':
                    case (byte)'=':
                        Throw.ArgumentException( nameof( base64UrlString ), "Invalid Base64Url data." );
                        break;
                }
            }
            while( written < a.Length ) a[written++] = (byte)'=';

            if( Base64.DecodeFromUtf8InPlace( a.AsSpan(), out int final ) != OperationStatus.Done )
            {
                Throw.ArgumentException( nameof( base64UrlString ), "Invalid Base64Url data." );
            }
            return a.AsMemory( 0, final );
        }

        static void Base64ToUrlBase64NoPadding( Span<byte> buffer, ref int bytesWritten )
        {
            for( int i = 0; i < bytesWritten; ++i )
            {
                switch( buffer[i] )
                {
                    case (byte)'=': bytesWritten = i; return;
                    case (byte)'+': buffer[i] = (byte)'-'; break;
                    case (byte)'/': buffer[i] = (byte)'_'; break;
                }
            }
        }

    }
}
