using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for collection &amp; list interfaces.
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">Collection items' type.</typeparam>
        /// <param name="this">This collection.</param>
        /// <param name="items">Multiple items to add. Can not be null.</param>
        public static void AddRange<T>( this ICollection<T> @this, IEnumerable<T> items )
        {
            if( items == null ) throw new ArgumentNullException( "items" );
            foreach( var i in items ) @this.Add( i );
        }

        /// <summary>
        /// Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">Collection items' type.</typeparam>
        /// <param name="this">This collection.</param>
        /// <param name="items">Items to add.</param>
        public static void AddRangeArray<T>( this ICollection<T> @this, params T[] items )
        {
            foreach( var i in items ) @this.Add( i );
        }

        /// <summary>
        /// Simple helper that removes elements in a <see cref="IList{T}"/> and returns them as an <see cref="IEnumerable{T}"/>.
        /// Makes the transfer of items from one list to another easy when combined with <see cref="AddRange"/>.
        /// The returned enumerable MUST be consumed to actually remove the items from the list (this is what AddRange do).
        /// Calling <see cref="System.Linq.Enumerable.Count{T}(IEnumerable{T})">IEnumerable&lt;T&gt;.Count()</see> for instance resolves the enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="this">This list.</param>
        /// <param name="removeCondition">Predicate that must return true for items that must be removed from this list.</param>
        /// <returns>Removed items (can be added into another one).</returns>
        public static IEnumerable<T> RemoveWhereAndReturnsRemoved<T>( this IList<T> @this, Func<T, bool> removeCondition )
        {
            for( int i = 0; i < @this.Count; ++i )
            {
                T x = @this[i];
                if( removeCondition( x ) )
                {
                    @this.RemoveAt( i-- );
                    yield return x;
                }
            }
        }

        /// <summary>
        /// Immutable reusable PropertyChangedEventArgs for "Item[]".
        /// </summary>
        public static readonly PropertyChangedEventArgs ItemArrayChangedEventArgs = new PropertyChangedEventArgs( "Item[]" );

        /// <summary>
        /// Immutable reusable PropertyChangedEventArgs for "Count".
        /// </summary>
        public static readonly PropertyChangedEventArgs CountChangedEventArgs = new PropertyChangedEventArgs( "Count" );
    }
}
