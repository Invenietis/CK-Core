#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\EnumerableExtension.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Checks whether the enumerable is in strict (no duplicates) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <returns>True if the enumerable is empty or is in strict ascending order.</returns>
        public static bool IsSortedStrict<T>( this IEnumerable<T> source )
        {
            return IsSortedStrict( source, Comparer<T>.Default.Compare );
        }

        /// <summary>
        /// Checks whether the enumerable is in large (duplicates allowed) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <returns>True if the enumerable is empty or is in large ascending order.</returns>
        public static bool IsSortedLarge<T>( this IEnumerable<T> source )
        {
            return IsSortedLarge( source, Comparer<T>.Default.Compare );
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
            if( @this == null ) throw new ArgumentNullException( "@this" );
            if( comparison == null )
                throw new ArgumentNullException( "comparison" );
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
            if( @this == null ) throw new ArgumentNullException( "@this" );
            if( comparison == null ) throw new ArgumentNullException( "comparison" );
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
        /// Returns the maximal element of the given sequence, based on a projection (typically
        /// one of the object property). The sequence MUST NOT 
        /// be empty otherwise an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence.</typeparam>
        /// <typeparam name="TKey">Type of the projected element.</typeparam>
        /// <param name="this">Source sequence.</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="this"/> or <paramref name="selector"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="this"/> is empty</exception>
        public static TSource MaxBy<TSource, TKey>( this IEnumerable<TSource> @this, Func<TSource, TKey> selector )
        {
            return MaxBy( @this, selector, Comparer<TKey>.Default.Compare );
        }

        /// <summary>
        /// Returns the maximal element of the given sequence based on
        /// a projection and a <see cref="Comparison{T}"/>. The sequence MUST NOT 
        /// be empty otherwise an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence.</typeparam>
        /// <typeparam name="TKey">Type of the projected element.</typeparam>
        /// <param name="this">Source sequence.</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparison">Comparison function  to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="this"/>, <paramref name="selector"/> 
        /// or <paramref name="comparison"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="this"/> is empty</exception>       
        public static TSource MaxBy<TSource, TKey>( this IEnumerable<TSource> @this, Func<TSource, TKey> selector, Comparison<TKey> comparison )
        {
            if( @this == null ) throw new ArgumentNullException( "@this" );
            if( selector == null ) throw new ArgumentNullException( "selector" );
            if( comparison == null ) throw new ArgumentNullException( "comparer" );
            using( IEnumerator<TSource> sourceIterator = @this.GetEnumerator() )
            {
                if( !sourceIterator.MoveNext() )
                {
                    throw new InvalidOperationException( "Sequence was empty" );
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
            if( @this == null ) throw new ArgumentNullException( "@this" );
            if( predicate == null ) throw new ArgumentNullException( "predicate" );
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
            if( @this == null ) throw new ArgumentNullException( "@this" );
            if( predicate == null ) throw new ArgumentNullException( "predicate" );
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

        /// <summary>
        /// Internal implementation of Append extension method.
        /// </summary>
        /// <typeparam name="T">Type of source sequence.</typeparam>
        class EAppend<T> : IEnumerable<T>
        {
            readonly IEnumerable<T> _source;
            readonly T _item;

            class E : IEnumerator<T>
            {
                readonly EAppend<T> _a;
                T _current;
                IEnumerator<T> _first;
                int _status;

                public E( EAppend<T> a ) 
                { 
                    _a = a;
                    _first = _a._source.GetEnumerator();
                }

                public T Current
                {
                    get 
                    {
                        if( _status <= 0 ) throw new InvalidOperationException();
                        return _current; 
                    }
                }

                public void Dispose()
                {
                    if( _first != null )
                    {
                        _first.Dispose();
                        _first = null;
                        _status = -1;
                    }
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }

                public bool MoveNext()
                {
                    if( _status < 0 ) throw new InvalidOperationException();
                    if( _status == 2 )
                    {
                        Dispose();
                        return false;
                    }
                    if( _first.MoveNext() )
                    {
                        _status = 1;
                        _current = _first.Current;
                    }
                    else
                    {
                        _current = _a._item;
                        _status = 2;
                    }
                    return true;
                }

                public void Reset()
                {
                    _first = _a._source.GetEnumerator();
                    _status = 0;
                }
            }

            public EAppend( IEnumerable<T> s, T item )
            {
                _source = s;
                _item = item;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new E( this );
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> that appends one item to an existing enumerable.
        /// </summary>
        /// <typeparam name="TSource">Type of source sequence.</typeparam>
        /// <param name="this">Source sequence.</param>
        /// <param name="item">Item to append.</param>
        /// <returns>An enumerable that appends the item to trhe sequence.</returns>
        public static IEnumerable<TSource> Append<TSource>( this IEnumerable<TSource> @this, TSource item )
        {
            if( @this == null ) throw new ArgumentNullException( "@this" );
            return new EAppend<TSource>( @this, item );
        }
    }
}
