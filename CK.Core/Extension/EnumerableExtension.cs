using System;
using System.Collections.Generic;

namespace CK.Core;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// Checks whether the enumerable is in strict (no duplicates) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable.</typeparam>
    /// <param name="this">This enumerable.</param>
    /// <returns>True if the enumerable is empty or is in strict ascending order.</returns>
    public static bool IsSortedStrict<T>( this IEnumerable<T> @this )
    {
        return IsSortedStrict( @this, Comparer<T>.Default.Compare );
    }

    /// <summary>
    /// Checks whether the enumerable is in large (duplicates allowed) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable.</typeparam>
    /// <param name="this">This enumerable.</param>
    /// <returns>True if the enumerable is empty or is in large ascending order.</returns>
    public static bool IsSortedLarge<T>( this IEnumerable<T> @this )
    {
        return IsSortedLarge( @this, Comparer<T>.Default.Compare );
    }

    /// <summary>
    /// Checks whether the enumerable is in strict (no duplicates) ascending order based on a comparison function.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable.</typeparam>
    /// <param name="this">This enumerable.</param>
    /// <param name="comparison">The delegate used to compare elements.</param>
    /// <returns>True if the enumerable is empty or is in strict ascending order.</returns>
    public static bool IsSortedStrict<T>( this IEnumerable<T> @this, Comparison<T> comparison )
    {
        Throw.CheckNotNullArgument( comparison );
        using( IEnumerator<T> e = @this.GetEnumerator() )
        {
            if( !e.MoveNext() ) return true;
            T prev = e.Current;
            while( e.MoveNext() )
            {
                T current = e.Current;
                if( comparison( prev, current ) >= 0 ) return false;
                prev = current;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks whether the enumerable is in large (duplicates allowed) ascending order based on a comparison function.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable.</typeparam>
    /// <param name="this">This enumerable.</param>
    /// <param name="comparison">The delegate used to compare elements.</param>
    /// <returns>True if the enumerable is empty or is in large ascending order.</returns>
    public static bool IsSortedLarge<T>( this IEnumerable<T> @this, Comparison<T> comparison )
    {
        Throw.CheckNotNullArgument( comparison );
        using( IEnumerator<T> e = @this.GetEnumerator() )
        {
            if( !e.MoveNext() ) return true;
            T prev = e.Current;
            while( e.MoveNext() )
            {
                T current = e.Current;
                if( comparison( prev, current ) > 0 ) return false;
                prev = current;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the maximal element of the given sequence based on
    /// a projection and a <see cref="Comparison{T}"/> (that defaults to <c>Comparer&lt;TKey&gt;.Default.Compare</c>.
    /// The sequence MUST NOT be empty otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence.</typeparam>
    /// <typeparam name="TKey">Type of the projected element.</typeparam>
    /// <param name="this">Source sequence.</param>
    /// <param name="selector">Selector to use to pick the results to compare.</param>
    /// <param name="comparison">Comparison function  to use to compare projected values. When null, <c>Comparer&lt;TKey&gt;.Default.Compare</c> is used.</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="this"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="this"/> is empty</exception>       
    public static TSource MaxBy<TSource, TKey>( this IEnumerable<TSource> @this, Func<TSource, TKey> selector, Comparison<TKey>? comparison = null )
    {
        Throw.CheckNotNullArgument( selector );
        if( comparison == null ) comparison = Comparer<TKey>.Default.Compare;
        using( IEnumerator<TSource> sourceIterator = @this.GetEnumerator() )
        {
            if( !sourceIterator.MoveNext() )
            {
                Throw.InvalidOperationException( "Sequence was empty" );
            }
            TSource max = sourceIterator.Current;
            TKey maxKey = selector( max );
            while( sourceIterator.MoveNext() )
            {
                TSource candidate = sourceIterator.Current;
                TKey candidateProjected = selector( candidate );
                if( comparison( candidateProjected, maxKey ) > 0 )
                {
                    max = candidate;
                    maxKey = candidateProjected;
                }
            }
            return max;
        }
    }

    /// <summary>
    /// Gets the first index in the enumerable where the predicate evaluates to true.
    /// Returns -1 when not found.
    /// </summary>
    /// <typeparam name="TSource">Type of source sequence.</typeparam>
    /// <param name="this">Source sequence.</param>
    /// <param name="predicate">Predicate function.</param>
    /// <returns>Index where predicate is true. -1 if not found.</returns>
    public static int IndexOf<TSource>( this IEnumerable<TSource> @this, Func<TSource, bool> predicate )
    {
        Throw.CheckNotNullArgument( predicate );
        int i = 0;
        using( var e = @this.GetEnumerator() )
        {
            while( e.MoveNext() )
            {
                if( predicate( e.Current ) ) return i;
                ++i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the first index in the enumerable where the predicate evaluates to true, the index of the element is available to the predicate.
    /// Returns -1 when not found.
    /// </summary>
    /// <typeparam name="TSource">Type of source sequence.</typeparam>
    /// <param name="this">Source sequence.</param>
    /// <param name="predicate">Predicate function that accepts the element and its index.</param>
    /// <returns>Index where predicate is true, or -1 if not found.</returns>
    public static int IndexOf<TSource>( this IEnumerable<TSource> @this, Func<TSource, int, bool> predicate )
    {
        Throw.CheckNotNullArgument( predicate );
        int i = 0;
        using( var e = @this.GetEnumerator() )
        {
            while( e.MoveNext() )
            {
                if( predicate( e.Current, i ) ) return i;
                ++i;
            }
        }
        return -1;
    }

}
