using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace CK.Core;

/// <summary>
/// Simple thread safe unique identifier generator with 64 bits (8 bytes) of entropy
/// that generates 11 characters long strings encoded in base 64 url.
/// </summary>
public sealed class FastUniqueIdGenerator
{
    long _next;

    /// <summary>
    /// Initializes a new generator with 8 bytes of entropy, generating 11 characters long
    /// strings encoded in base 64 url.
    /// </summary>
    public FastUniqueIdGenerator()
    {
        var bytes = MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref _next, 1 ) );
        System.Security.Cryptography.RandomNumberGenerator.Fill( bytes );
    }

    /// <summary>
    /// Initializes a new generator with an initial first value instead of using <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.
    /// </summary>
    /// <param name="initialValue">The explicit initial value.</param>
    public FastUniqueIdGenerator( long initialValue )
    {
        _next = initialValue;
    }

    /// <summary>
    /// Gets the next unique identifier.
    /// </summary>
    /// <returns></returns>
    public long GetNextLong() => Interlocked.Increment( ref _next );

    /// <summary>
    /// Fills the provided span with the UTF8 base64 url encoded next unique identifier.
    /// 11 bytes will be written to <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Target span. Must be at least 11 bytes long.</param>
    public void FillNextUtf8String( Span<byte> id )
    {
        Throw.CheckArgument( id.Length >= 11 );
        var x = Interlocked.Increment( ref _next );
        var sx = MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref x, 1 ) );
        if( BitConverter.IsLittleEndian ) sx.Reverse();
        Debug.Assert( Base64.GetMaxEncodedToUtf8Length( 8 ) == 12 );
        Span<byte> buffer = stackalloc byte[12];
        Base64.EncodeToUtf8( sx, buffer, out int _, out int bytesWritten );
        Debug.Assert( bytesWritten == 12 && buffer[11] == '=' );
        Base64UrlHelper.UncheckedBase64ToUrlBase64NoPadding( buffer, ref bytesWritten );
        Debug.Assert( bytesWritten == 11 );
        buffer.Slice( 0, 11 ).CopyTo( id );
    }

    /// <summary>
    /// Creates a base64 url string of 11 characters.
    /// </summary>
    /// <returns>A unique string.</returns>
    public string GetNextString() => CreateString( Interlocked.Increment( ref _next ) );

    /// <summary>
    /// Creates a base64 url string of 11 characters from a long.
    /// </summary>
    /// <returns>The base64 url string.</returns>
    public static string CreateString( long x )
    {
        var sx = MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref x, 1 ) );
        if( BitConverter.IsLittleEndian ) sx.Reverse();
        return Base64UrlHelper.ToBase64UrlString( sx );
    }

    /// <summary>
    /// Creates a base64 url string of 11 characters using <see cref="System.Security.Cryptography.RandomNumberGenerator.Fill(Span{byte})"/>.
    /// </summary>
    /// <returns>A random string.</returns>
    public static string GetRandomString()
    {
        Span<byte> buffer = stackalloc byte[8];
        System.Security.Cryptography.RandomNumberGenerator.Fill( buffer );
        return Base64UrlHelper.ToBase64UrlString( buffer );
    }

}
