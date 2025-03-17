using Microsoft.IO;
using System;
using System.Buffers;
using System.Text.Json;

namespace CK.Core;

/// <summary>
/// Enables <see cref="Utf8JsonReader"/> to be refilled with data.
/// This interface is not <see cref="IDisposable"/> but its implementations
/// like <see cref="Utf8JsonStreamReaderContext"/> often needs to be.
/// <para>
/// The pattern to use it is to replace all calls to <c>reader.Read()</c> with:
/// <code>
/// if( !reader.Read() ) context.ReadMoreData( ref reader );
/// </code>
/// And all <c>reader.Skip()</c> calls with:
/// <code>
/// if( !reader.TrySkip() ) context.SkipMoreData( ref reader );
/// </code>
/// </para>
/// Or use the extension methods <see cref="Utf8JsonExtensions.ReadWithMoreData"/> and <see cref="Utf8JsonExtensions.SkipWithMoreData"/>
/// that encapsulate these.
/// <para>
/// This implements the pattern described here:
/// <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader#read-from-a-stream-using-utf8jsonreader"/>
/// </para>
/// </summary>
public interface IUtf8JsonReaderContext
{
    /// <summary>
    /// Empty object pattern. Can be used when data is available in memory, typically
    /// in a <see cref="ReadOnlySequence{T}"/> of bytes.
    /// </summary>
    public sealed class EmptyContext : IDisposableUtf8JsonReaderContext
    {
        void IUtf8JsonReaderContext.ReadMoreData( ref Utf8JsonReader reader ) { }
        void IUtf8JsonReaderContext.SkipMoreData( ref Utf8JsonReader reader ) { }
        void IDisposable.Dispose() { }
    }

    /// <summary>
    /// Gets an empty context that doesn't provide any data.
    /// </summary>
    public static readonly EmptyContext Empty = new EmptyContext();

    /// <summary>
    /// Method to call whenever <see cref="Utf8JsonReader.Read()"/> returns false.
    /// </summary>
    /// <param name="reader">The reader for which more data is needed.</param>
    void ReadMoreData( ref Utf8JsonReader reader );

    /// <summary>
    /// Method to call whenever <see cref="Utf8JsonReader.TrySkip()"/> returns false.
    /// </summary>
    /// <param name="reader">The reader for which more data is needed.</param>
    void SkipMoreData( ref Utf8JsonReader reader );
}
