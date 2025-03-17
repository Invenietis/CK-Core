using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace CK.Core;

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
        Throw.CheckState( s == OperationStatus.Done );
        UncheckedBase64ToUrlBase64NoPadding( buffer, ref bytesWritten );
    }

    const int MaxStackSize = 256;

    /// <summary>
    /// Creates a string in Base64Url from bytes.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToBase64UrlString( ReadOnlySpan<byte> bytes )
    {

        int size = Base64.GetMaxEncodedToUtf8Length( bytes.Length );
        byte[]? fromPool = null;
        Span<byte> buffer = size > MaxStackSize
                            ? (fromPool = ArrayPool<byte>.Shared.Rent( size )).AsSpan( 0, size )
                            : stackalloc byte[size];
        try
        {
            Base64.EncodeToUtf8( bytes, buffer, out int _, out int bytesWritten );
            UncheckedBase64ToUrlBase64NoPadding( buffer, ref bytesWritten );
            return Encoding.ASCII.GetString( buffer.Slice( 0, bytesWritten ) );
        }
        finally
        {
            if( fromPool != null ) ArrayPool<byte>.Shared.Return( fromPool );
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
        if( !TryFromBase64UrlString( base64UrlString, out Memory<byte> result ) )
        {
            Throw.ArgumentException( nameof( base64UrlString ), "Invalid Base64Url data." );
        }
        return result;
    }

    /// <summary>
    /// Tries to convert a base64Url string to its bytes value.
    /// </summary>
    /// <param name="base64UrlString">The string.</param>
    /// <param name="value">The value on success, empty otherwise.</param>
    /// <returns>Whether the decoding succeeded.</returns>
    public static bool TryFromBase64UrlString( string base64UrlString, out Memory<byte> value )
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
                    value = Memory<byte>.Empty;
                    return false;
            }
        }
        while( written < a.Length ) a[written++] = (byte)'=';

        if( Base64.DecodeFromUtf8InPlace( a.AsSpan(), out int final ) != OperationStatus.Done )
        {
            value = Memory<byte>.Empty;
            return false;
        }
        value = a.AsMemory( 0, final );
        return true;
    }

    /// <summary>
    /// Process the buffer by mapping '+' to '-' and '/' to '_' up to the end or to
    /// the first '=' (padding). When padding is met, <paramref name="bytesWritten"/> is updated.
    /// </summary>
    /// <param name="buffer">The buffer that must be in Base64. No checks are done.</param>
    /// <param name="bytesWritten">Updated buffer length: padding is skipped.</param>
    public static void UncheckedBase64ToUrlBase64NoPadding( Span<byte> buffer, ref int bytesWritten )
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

    /// <summary>
    /// Gets the allowed characters in base64Url. 
    /// </summary>
    public static readonly string Base64UrlCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

    /// <summary>
    /// Gets whether the string is composed only of <see cref="Base64UrlCharacters"/>.
    /// This doesn't check that the string is a valid base 64 url encoding: this just check the set of characters
    /// (for instance "A" is not a valid string even if it contains valid characters).
    /// <para>
    /// Use <see cref="TryFromBase64UrlString"/> to check a valid base 64 url value.
    /// </para>
    /// </summary>
    /// <param name="s">The string to test.</param>
    /// <returns>True if the string is composed only of <see cref="Base64UrlCharacters"/>.</returns>
    public static bool IsBase64UrlCharacters( ReadOnlySpan<char> s ) => s.TrimStart( Base64UrlCharacters.AsSpan() ).IsEmpty;

}
