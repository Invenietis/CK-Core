using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core;

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
        Throw.CheckNotNullArgument( items );
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
    /// <returns>Removed items (must be added into another one or counted for the remove to work).</returns>
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


    sealed class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>, IEquatable<IReadOnlyCollection<T>>
    {
        readonly ICollection<T> _values;

        public ReadOnlyCollectionWrapper( ICollection<T> values ) => _values = values;

        public ICollection<T> Values => _values;

        public int Count => _values.Count;

        public bool Equals( IReadOnlyCollection<T>? other ) => other == _values;

        public override bool Equals( [NotNullWhen( true )] object? obj ) => obj == _values || (obj is ReadOnlyCollectionWrapper<T> w && w._values == _values);

        public override int GetHashCode() => _values.GetHashCode();

        public IEnumerator<T> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Returns this collection if the implementation supports <see cref="IReadOnlyCollection{T}"/> 
    /// or a simple wrapper that adapts the interface.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <param name="this">This collection.</param>
    /// <returns>This collection or a simple wrapper that adapts the interface.</returns>
    public static IReadOnlyCollection<T> AsIReadOnlyCollection<T>( this ICollection<T> @this )
    {
        if( @this is not IReadOnlyCollection<T> c )
        {
            c = CrateWrapper( @this );
        }
        return c;

        static IReadOnlyCollection<T> CrateWrapper( ICollection<T> @this )
        {
            if( @this == null ) throw new NullReferenceException();
            return new ReadOnlyCollectionWrapper<T>( @this );
        }
    }
}
