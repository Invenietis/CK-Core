using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Represents a generic read only keyed collections of covariant items with
    /// a contravariant key that can support duplicate items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key associated to the elements.</typeparam>
    public interface ICKReadOnlyMultiKeyedCollection<out T, in TKey> : ICKReadOnlyUniqueKeyedCollection<T,TKey>
    {
        /// <summary>
        /// Gets whether this collection supports duplicates.
        /// </summary>
        bool AllowDuplicates { get; }

        /// <summary>
        /// Gets the number of items in this keyed collection that are associated to the
        /// given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>Number of items with the <paramref name="key"/>.</returns>
        int KeyCount( TKey key );

        /// <summary>
        /// Gets an independent collection of the items that 
        /// are associated to the given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>An independent collection of <typeparamref name="T"/>.</returns>
        IReadOnlyCollection<T> GetAllByKey( TKey key );
    }
}
