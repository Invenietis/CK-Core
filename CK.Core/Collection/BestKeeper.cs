using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CK.Core
{

    /// <summary>
    /// Implements a min heap: best items (depending on <see cref="Comparer"/>.
    /// It is important to note that kept items are not ordered inside the heap
    /// (hence this specializes IReadOnlyCollection instead of IReadOnlyList).
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class BestKeeper<T> : IReadOnlyCollection<T>
    {
        readonly T[] _items;
        int _count;

        class ComparerAdapter : IComparer<T>
        {
            readonly Func<T, T, int> _comparator;

            public ComparerAdapter( Func<T, T, int> comparator ) => _comparator = comparator;

            public int Compare( T x, T y ) => _comparator( x, y );
        }

        /// <summary>
        /// Initializes a new <see cref="BestKeeper{T}"/> on a comparator function.
        /// </summary>
        /// <param name="capacity">The fixed, maximal, number of items.</param>
        /// <param name="comparator">The comparator function.</param>
        public BestKeeper( int capacity, Func<T, T, int> comparator )
            : this( capacity, new ComparerAdapter( comparator ) )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BestKeeper{T}"/>.
        /// When no comparer is provided, <see cref="Comparer{T}.Default"/> is used.
        /// </summary>
        /// <param name="capacity">The fixed, maximal, number of items.</param>
        /// <param name="comparer">The optional comparer.</param>
        public BestKeeper( int capacity, IComparer<T> comparer = null )
        {
            if( capacity <= 0 ) throw new ArgumentException( "The max count must be greater than 0.", nameof( capacity ) );
            Comparer = comparer ?? Comparer<T>.Default;
            _items = new T[capacity];
        }

        /// <summary>
        /// Adds a item and signals whether it has been kept.
        /// </summary>
        /// <param name="candidate">The candidate item.</param>
        /// <param name="collector">The optional collector of items eliminated from the current <see cref="BestKeeper{T}"/>.</param>
        /// <returns>Whether the candidate has been kept.</returns>
        public bool Add( T candidate, Action<T> collector = null )
        {
            if( IsFull )
            {
                if( Comparer.Compare( candidate, _items[ 0 ] ) < 0 ) return false;
                AddFromTop( candidate, collector );
                return true;
            }

            AddFromBottom( candidate );
            return true;
        }

        /// <summary>
        /// Gets the comprarer that is used to determine the "best" to keep.
        /// </summary>
        public IComparer<T> Comparer { get; }

        /// <summary>
        /// Gets the maximum number of items.
        /// </summary>
        public int Capacity => _items.Length;

        /// <summary>
        /// Gets the current number of items.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the items that have been kept.
        /// Note that items are not ordered.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => _items.Take( _count ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IsFull => _count == _items.Length;

        void AddFromBottom( T item )
        {
            _items[ _count ] = item;
            int idx = _count;

            while( idx > 0 )
            {
                int fatherIdx = ( idx - 1 ) / 2;
                if( Comparer.Compare( item, _items[ fatherIdx ] ) > 0 ) break;
                Swap( idx, fatherIdx );
                idx = fatherIdx;
            }
            _count++;
        }

        void AddFromTop( T candidate, Action<T> collector )
        {
            int idx = 0;
            T removedItem = _items[ 0 ];
            _items[ 0 ] = candidate;

            while( true )
            {
                int leftIdx = idx * 2 + 1;
                int rightIdx = idx * 2 + 2;

                int smallestIdx;
                if( leftIdx < _count && Comparer.Compare( _items[ leftIdx ], candidate ) < 0 ) smallestIdx = leftIdx;
                else smallestIdx = idx;
                if( rightIdx < _count && Comparer.Compare( _items[ rightIdx ], _items[ smallestIdx ] ) < 0 ) smallestIdx = rightIdx;

                if( smallestIdx == idx )
                {
                    if( collector != null ) collector( removedItem );
                    return;
                }

                Swap( smallestIdx, idx );
                idx = smallestIdx;
            }
        }

        void Swap( int idx1, int idx2 )
        {
            T item = _items[ idx1 ];
            _items[ idx1 ] = _items[ idx2 ];
            _items[ idx2 ] = item;
        }
    }
}
