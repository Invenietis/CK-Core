using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="ICKReadOnlyCollection{T}"/>, <see cref="IReadOnlyList{T}"/> and <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}"/>.
    /// </summary>
    public static class CKReadOnlyExtension
    {
        /// <summary>
        /// Gets the item with the associated key, forgetting the exists out parameter in <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}.GetByKey(TKey,out bool)"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="this">Keyed collection of elements.</param>
        /// <param name="key">The item key.</param>
        /// <returns>The item that matches the key, default(T) if the key can not be found.</returns>
        static public T GetByKey<T, TKey>( this ICKReadOnlyUniqueKeyedCollection<T, TKey> @this, TKey key )
        {
            bool exists;
            return @this.GetByKey( key, out exists );
        }

        /// <summary>
        /// Creates an array from a read only collection.
        /// This is a much more efficient version than the IEnumerable ToArray extension method
        /// since this implementation allocates one and only one array. 
        /// </summary>
        /// <typeparam name="T">Type of the array and lists elements.</typeparam>
        /// <param name="this">Read only collection of elements.</param>
        /// <returns>A new array that contains the same element as the collection.</returns>
        static public T[] ToArray<T>( this IReadOnlyCollection<T> @this )
        {
            T[] r = new T[@this.Count];
            int i = 0;
            foreach( T item in @this ) r[i++] = item;
            return r;
        }

    }
}
