using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK.Core.Json
{
    /// <summary>
    /// Delegate for typed read from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <param name="r">The reader.</param>
    /// <param name="context">The context.</param>
    /// <returns>The read instance or null.</returns>
    public delegate void Utf8JsonReaderDelegate( ref Utf8JsonReader r, IUtf8JsonReaderContext context );

    /// <summary>
    /// Delegate for typed read from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <typeparam name="T">The type to read.</typeparam>
    /// <param name="r">The reader.</param>
    /// <param name="context">The context.</param>
    /// <returns>The read instance or null.</returns>
    public delegate T? Utf8JsonReaderDelegate<T>( ref Utf8JsonReader r, IUtf8JsonReaderContext context );
}
