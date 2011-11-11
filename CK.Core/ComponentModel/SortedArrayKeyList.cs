using System;
using System.Collections;
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SortedArrayKeyList<T,TKey> : SortedArrayList<T>
    {
        Func<T,TKey> _keySelector;
        Comparison<TKey> _keyComparison;

        public SortedArrayKeyList( Func<T, TKey> keySelector, bool allowDuplicates = false )
            : this( keySelector, Comparer<TKey>.Default.Compare, allowDuplicates )
        {
        }

        public SortedArrayKeyList( Func<T, TKey> keySelector, Comparison<TKey> keyComparison, bool allowDuplicates = false )
            : base( ( e1, e2 ) => keyComparison( keySelector( e1 ), keySelector( e2 ) ), allowDuplicates )
        {
            _keySelector = keySelector;
            _keyComparison = keyComparison;
        }

        public int IndexOf( TKey key )
        {
            return Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
        }

        /// <summary>
        /// Gets the index of the element thanks to a linear search into the 
        /// internal array.
        /// If the key did not change, it is more efficient to find an element with <see cref="IndexOf(TKey)"/> that 
        /// uses a dichotomic search.
        /// </summary>
        /// <param name="value">The element to locate.</param>
        /// <returns>The index in array that, if found; otherwise, –1.</returns>
        public override int IndexOf( T value )
        {
            if( value == null ) throw new ArgumentNullException();
            return Array.IndexOf<T>( Store, value, 0, Count );
        }

        public override int IndexOf( object item )
        {
            if( item is T ) return IndexOf( (T)item );
            if( item is TKey ) return IndexOf( (TKey)item );
            return Int32.MinValue;
        }

        public override bool Contains( object item )
        {
            if( item is T ) return Contains( (T)item );
            if( item is TKey ) return Contains( (TKey)item );
            return false;
        }

        public bool TryGetValue( TKey key, out T value )
        {
            int idx = Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
            if( idx < 0 )
            {
                value = default( T );
                return false;
            }
            value = Store[idx];
            return true;
        }

        public bool Remove( TKey key )
        {
            int index = IndexOf( key );
            if( index < 0 ) return false;
            RemoveAt( index );
            return true;
        }

        int ComparisonKey( T e, TKey key )
        {
            return _keyComparison( _keySelector( e ), key );
        }

    }
}
