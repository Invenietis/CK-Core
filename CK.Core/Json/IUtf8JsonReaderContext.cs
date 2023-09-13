using System;
using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Enables <see cref="Utf8JsonReader"/> to be refilled with data.
    /// This interface is not <see cref="IDisposable"/> but its implementations
    /// like <see cref="Utf8JsonStreamReader"/> often needs to be.
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
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader#read-from-a-stream-using-utf8jsonreader
    /// </para>
    /// </summary>
    public interface IUtf8JsonReaderContext
    {
        sealed class EmptyImpl : IUtf8JsonReaderContext
        {
            public void ReadMoreData( ref Utf8JsonReader reader ) {}
            public void SkipMoreData( ref Utf8JsonReader reader ) {}
        }

        /// <summary>
        /// Gets an empty context that doesn't provide any data.
        /// </summary>
        public static readonly IUtf8JsonReaderContext Empty = new EmptyImpl();

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
}
