#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKSortedArrayKeyList.cs) is part of CiviKey. 
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Sorted list of items where the sort order relies on an external key, not the item itself.
    /// </summary>.
    [DebuggerTypeProxy( typeof( CKSortedArrayKeyList<,>.DebuggerView ) ), DebuggerDisplay( "Count = {Count}" )]
    public class CKSortedArrayKeyList<T, TKey> : CKSortedArrayList<T>, ICKReadOnlyMultiKeyedCollection<T, TKey>
    {
        Func<T,TKey> _keySelector;
        Comparison<TKey> _keyComparison;

        [ExcludeFromCodeCoverage]
        class DebuggerView
        {
            CKSortedArrayKeyList<T, TKey> _c;

            public DebuggerView( CKSortedArrayKeyList<T, TKey> c )
            {
                _c = c;
            }

            /// <summary>
            /// Gets the items as a flattened array view.
            /// </summary>
            [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
            public KeyValuePair<TKey,T>[] Items
            {
                get
                {
                    var a = new List<KeyValuePair<TKey,T>>();
                    foreach( var e in _c )
                    {
                        a.Add( new KeyValuePair<TKey,T>( _c._keySelector( e ), e ) );
                    }
                    return a.ToArray();
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayKeyList{T,TKey}"/>.
        /// </summary>
        /// <param name="keySelector">Function that associates a key to an item.</param>
        /// <param name="allowDuplicates">True to allow duplicates.</param>
        public CKSortedArrayKeyList( Func<T, TKey> keySelector, bool allowDuplicates = false )
            : this( keySelector, Comparer<TKey>.Default.Compare, allowDuplicates )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayKeyList{T,TKey}"/> where a <see cref="Comparison{T}"/> function
        /// is used to compare keys.
        /// </summary>
        /// <param name="keySelector">Function that associates a key to an item.</param>
        /// <param name="keyComparison">Function used to compare keys.</param>
        /// <param name="allowDuplicates">True to allow duplicates.</param>
        public CKSortedArrayKeyList( Func<T, TKey> keySelector, Comparison<TKey> keyComparison, bool allowDuplicates = false )
            : base( ( e1, e2 ) => keyComparison( keySelector( e1 ), keySelector( e2 ) ), allowDuplicates )
        {
            _keySelector = keySelector;
            _keyComparison = keyComparison;
        }

        /// <summary>
        /// Gets the zero based position of on of the items that is associated to this key.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>The index or a negative value like <see cref="Util.BinarySearch{T,TKey}"/>.</returns>
        public int IndexOf( TKey key )
        {
            return Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
        }

        /// <summary>
        /// True if this list contains at least one item with the given key.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>True if an item is foud, false otherwise.</returns>
        public bool Contains( TKey key )
        {
            return Util.BinarySearch( Store, 0, Count, key, ComparisonKey ) >= 0;
        }

        /// <summary>
        /// Gets the first item with a given key or the default value if no such item exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="exists">True if the key has been found, otherwise false.</param>
        /// <returns>The item or default(T) if not found.</returns>
        public T GetByKey( TKey key, out bool exists )
        {
            int idx = Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
            return (exists = idx >= 0) ? Store[idx] : default( T );
        }

        /// <summary>
        /// Gets the number of items with a given key. It can be greater than 1 only if <see cref="CKSortedArrayList{T}.AllowDuplicates">AllowDuplicates</see> is true.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>The number of item with the <paramref name="key"/>.</returns>
        public int KeyCount( TKey key )
        {
            int idx = Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
            if( idx < 0 ) return 0;
            if( !AllowDuplicates ) return 1;
            int min = idx - 1;
            while( min >= 0 && ComparisonKey( Store[min], key ) == 0 ) --min;
            int max = idx + 1;
            while( max < Store.Length && ComparisonKey( Store[max], key ) == 0 ) ++max;
            return max - min - 1;
        }

        /// <summary>
        /// Gets an independant collection of the items that 
        /// are associated to the given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>An independant collection of <typeparamref name="T"/>.</returns>
        public IReadOnlyCollection<T> GetAllByKey( TKey key )
        {
            int idx = Util.BinarySearch( Store, 0, Count, key, ComparisonKey );
            if( idx < 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( !AllowDuplicates ) return new CKReadOnlyListMono<T>( Store[idx] );
            int min = idx - 1;
            while( min >= 0 && ComparisonKey( Store[min], key ) == 0 ) --min;
            int max = idx + 1;
            while( max < Store.Length && ComparisonKey( Store[max], key ) == 0 ) ++max;           
            int count = max - (++min);
            Debug.Assert( count > 0 );
            return Store.ToReadOnlyList( min, count );
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

        /// <summary>
        /// Covariant IndexOf method: if <paramref name="item"/> is of type <typeparamref name="T"/>
        /// the linear <see cref="IndexOf(T)"/> is used but if <paramref name="item"/> is of type <typeparamref name="TKey"/>,
        /// the logarithmic <see cref="IndexOf(TKey)"/> is used.
        /// </summary>
        /// <param name="item">Can be a <typeparamref name="T"/> or a <typeparamref name="TKey"/>.</param>
        /// <returns>The index of the item in the collection.</returns>
        public override int IndexOf( object item )
        {
            if( item is T ) return IndexOf( (T)item );
            if( item is TKey ) return IndexOf( (TKey)item );
            return Int32.MinValue;
        }

        /// <summary>
        /// Covariant version of the contains predicate. 
        /// If <paramref name="item"/> is of type <typeparamref name="T"/> the <see cref="CKSortedArrayList{T}.Contains(T)"/> is used 
        /// but if <paramref name="item"/> is of type <typeparamref name="TKey"/>, the <see cref="Contains(TKey)"/> is used.
        /// </summary>
        /// <param name="item">Can be a <typeparamref name="T"/> or a <typeparamref name="TKey"/>.</param>
        /// <returns>True if a corresponding element in this list can be found.</returns>
        public override bool Contains( object item )
        {
            if( item is T ) return Contains( (T)item );
            if( item is TKey ) return Contains( (TKey)item );
            return false;
        }

        /// <summary>
        /// Removes one item given a key: only one item is removed when <see cref="CKSortedArrayList{T}.AllowDuplicates"/> is 
        /// true and more than one item are associated to this key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if an item has been removed, false otherwise.</returns>
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
