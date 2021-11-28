using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Represents a generic read only keyed collections of covariant items with
    /// a contravariant key. This interface can be supported by collections that 
    /// support duplicated items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key associated to the elements.</typeparam>
    public interface ICKReadOnlyUniqueKeyedCollection<out T, in TKey> : ICKReadOnlyCollection<T>
    {
        /// <summary>
        /// Checks whether any item in this keyed collection is associated to the
        /// given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        bool Contains( TKey key );

        /// <summary>
        /// Gets the item with the associated key.
        /// When duplicated item exists, any item with the given key can be returned.
        /// </summary>
        /// <param name="key">The item key.</param>
        /// <param name="exists">True if the key has been found, false otherwise (see remarks).</param>
        /// <returns>The item that matches the key, default(T) if the key can not be found.</returns>
        /// <remarks>
        /// Due to current CLI limitation (out parameters are actually ref parameters), it is not possible 
        /// to define a method with an ( out T ) parameter where T is covariant: we can not define 
        /// the standard TryGetValue method but this "opposite" form.
        /// </remarks>
        [return: MaybeNull]
        T GetByKey( TKey key, out bool exists );

    }
}
