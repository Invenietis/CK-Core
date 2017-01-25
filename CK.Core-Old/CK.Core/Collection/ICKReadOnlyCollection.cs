using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Represents a generic read only collections of objects with a contravariant <see cref="Contains"/> method.
    /// This enables collection implementing this interface to support better lookup complexity than O(n) if possible. 
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public interface ICKReadOnlyCollection<out T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Determines whether collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to find in the collecion.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        bool Contains( object item );

    }
}
