using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [return: MaybeNull]
        static public T GetByKey<T, TKey>( this ICKReadOnlyUniqueKeyedCollection<T, TKey> @this, TKey key )
        {
            return @this.GetByKey( key, out _ );
        }

        /// <summary>
        /// Finds the index of a first item in a <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="this">This list.</param>
        /// <param name="predicate">Predicate function.</param>
        /// <returns>Index of the matching item or -1.</returns>
        static public int IndexOf<T>( this IReadOnlyList<T> @this, Func<T,bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            int i = 0;
            foreach( var x in @this )
            {
                if( predicate( x ) ) return i;
                ++i;
            }
            return -1;
        }

    }
}
