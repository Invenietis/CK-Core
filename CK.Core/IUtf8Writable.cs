using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Simple explicit contract for a <see cref="Write(Utf8JsonWriter)"/> method.
    /// </summary>
    public interface IUtf8Writable
    {
        /// <summary>
        /// Must write the content of this object to the writer.
        /// </summary>
        /// <param name="w">The writer.</param>
        void Write( Utf8JsonWriter w );
    }
}
