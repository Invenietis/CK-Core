using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CK.Core
{
    /// <summary>
    /// Extends <see cref="IncrementalHash"/>.
    /// </summary>
    public static class IncrementalHashExtensions
    {
        /// <summary>
        /// Appends a string.
        /// </summary>
        /// <param name="hasher">This Incremental hasher.</param>
        /// <param name="value">The string to append.</param>
        /// <returns>This IncrementalHash.</returns>
        public static IncrementalHash Append( this IncrementalHash hasher, string value )
        {
            hasher.AppendData( MemoryMarshal.Cast<char, byte>( value.AsSpan() ) );
            return hasher;
        }

        /// <summary>
        /// Appends a basic value type: the <typeparamref name="T"/> must not contain references or pointers
        /// otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">Value's type.</typeparam>
        /// <param name="hasher">This Incremental hasher.</param>
        /// <param name="value">The basic value (no pointers, no references).</param>
        /// <returns>This IncrementalHash.</returns>
        public static IncrementalHash Append<T>( this IncrementalHash hasher, T value ) where T : struct
        {
            hasher.AppendData( MemoryMarshal.AsBytes( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) ) );
            return hasher;
        }
    }
}
