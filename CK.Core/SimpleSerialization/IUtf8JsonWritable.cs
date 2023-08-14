using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Simple explicit contract for a <see cref="Write(Utf8JsonWriter)"/> method.
    /// </summary>
    public interface IUtf8JsonWritable
    {
        /// <summary>
        /// Must write the content of this object to the writer.
        /// </summary>
        /// <param name="w">The writer.</param>
        void Write( Utf8JsonWriter w );
    }

    /// <summary>
    /// Useful delegate for typed read from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <typeparam name="T">The type to read.</typeparam>
    /// <param name="r">The reader.</param>
    /// <returns>The read instance or null.</returns>
    public delegate T? Utf8JsonReaderDelegate<T>( ref Utf8JsonReader r );

}
