#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKSortedArrayList.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Simple sorted array list implementation that supports covariance through <see cref="IReadOnlyList{T}"/> and contravariance 
    /// with <see cref="ICKWritableCollection{T}"/>.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IList{T}"/> both for performance (unfortunately Linq relies -too much- on it) and for interoperability reasons: this
    /// interface should NOT be used. Accessors of the <see cref="IList{T}"/> that defeats the invariant of this class (the fact that elements are sorted, such 
    /// as <see cref="IList{T}.Insert"/>) are explicitely implemented to hide them as much as possible.
    /// </remarks>
    [DebuggerTypeProxy( typeof( Impl.CKReadOnlyCollectionDebuggerView<> ) ), DebuggerDisplay( "Count = {Count}" )]
    public class CKSortedArrayList<T> : IList<T>, ICKReadOnlyList<T>, ICKWritableCollection<T>
    {
        const int _defaultCapacity = 4;

        static T[] _empty = new T[0];
        T[] _tab;
        int _count;
        int _version;

        /// <summary>
        /// Specialized implementation can use this comparison function if needed.
        /// </summary>
        protected readonly Comparison<T> Comparator;

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayList{T}"/> that rejects duplicates 
        /// and uses the <see cref="Comparer{T}.Default"/> comparer.
        /// </summary>
        /// <remarks>
        /// A default constructor is a parameterless constructor, it is not the same as a constructor with default parameter values.
        /// This is why it is explicitely defined.
        /// </remarks>
        public CKSortedArrayList()
            : this( Comparer<T>.Default.Compare, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayList{T}"/> that rejects or allows duplicates 
        /// and uses the <see cref="Comparer{T}.Default"/> comparer.
        /// </summary>
        /// <param name="allowDuplicates">True to allow duplicate elements.</param>
        public CKSortedArrayList( bool allowDuplicates )
            : this( Comparer<T>.Default.Compare, allowDuplicates )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayList{T}"/> that rejects or allows duplicates 
        /// and uses the given comparer.
        /// </summary>
        /// <param name="comparer">Comparer to use.</param>
        /// <param name="allowDuplicates">True to allow duplicate elements.</param>
        public CKSortedArrayList( IComparer<T> comparer, bool allowDuplicates = false )
            : this( comparer.Compare, allowDuplicates )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKSortedArrayList{T}"/> that rejects or allows duplicates 
        /// and uses the given comparison function.
        /// </summary>
        /// <param name="comparison">Comparison function to use.</param>
        /// <param name="allowDuplicates">True to allow duplicate elements.</param>
        public CKSortedArrayList( Comparison<T> comparison, bool allowDuplicates = false )
        {
            _tab = _empty;
            Comparator = comparison;
            if( allowDuplicates ) _version = 1;
        }

        /// <summary>
        /// Explicitely implemented since our <see cref="Add"/> method
        /// returns a boolean.
        /// </summary>
        /// <param name="item">Item to add.</param>
        void ICollection<T>.Add( T item )
        {
            Add( item );
        }

        /// <summary>
        /// Gets whether this list allows duplicated items.
        /// </summary>
        public bool AllowDuplicates 
        { 
            get { return (_version & 1) != 0; } 
        }

        /// <summary>
        /// Locates an element (one of the occurences when duplicates are allowed) in this list (logarithmic). 
        /// </summary>
        /// <param name="value">The element.</param>
        /// <returns>The result of the <see cref="Util.BinarySearch{T}"/> in the internal array.</returns>
        public virtual int IndexOf( T value )
        {
            if( value == null ) throw new ArgumentNullException();
            return Util.BinarySearch<T>( _tab, 0, _count, value, Comparator );
        }

        /// <summary>
        /// Binary search implementation that relies on an extended comparer: a function that knows how to 
        /// compare the elements of the array to its key. This function must work exactly like this <see cref="Comparator"/>
        /// but accepts a <typeparamref name="T"/> and the <typeparamref name="TKey"/> that is used to sort the items otherwise
        /// the result is undefined.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="key">The value of the key.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public int IndexOf<TKey>( TKey key, Func<T, TKey, int> comparison )
        {
            if( comparison == null ) throw new ArgumentNullException();
            return Util.BinarySearch( _tab, 0, _count, key, comparison );
        }

        /// <summary>
        /// Determines whether this <see cref="CKSortedArrayList{T}"/> contains a specific value (logarithmic).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the object is found; otherwise, false.</returns>
        public bool Contains( T value )
        {
            return IndexOf( value ) >= 0;
        }

        /// <summary>
        /// Covariant compatible overload of <see cref="IndexOf(T)"/>  (logarithmic).
        /// If the item is not <typeparamref name="T"/> compatible, the 
        /// value <see cref="Int32.MinValue"/> is returned. See <see cref="ICKReadOnlyList{T}.IndexOf"/>.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>
        /// Positive index when found, negative one when not found and <see cref="Int32.MinValue"/> 
        /// if the item can structurally NOT appear in this list.</returns>
        public virtual int IndexOf( object item )
        {
            return item is T ? IndexOf( (T)item ) : Int32.MinValue;
        }

        /// <summary>
        /// Covariant compatible overload of <see cref="Contains(T)"/>  (logarithmic).
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>True if the object is found; otherwise, false.</returns>
        public virtual bool Contains( object item )
        {
            return item is T ? Contains( (T)item ) : false;
        }

        /// <summary>
        /// Copy the content of the internal array into the given array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="arrayIndex">Index at which copying must start.</param>
        public void CopyTo( T[] array, int arrayIndex )
        {
            Array.Copy( _tab, 0, array, arrayIndex, _count );
        }

        /// <summary>
        /// Gets the number of elements in this sorted list.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Explicit implementation that always returns false.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes a value and returns true if found; otherwise returns false.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value has been found and removed, false otherwise.</returns>
        public bool Remove( T value )
        {
            int index = IndexOf( value );
            if( index < 0 ) return false;
            RemoveAt( index );
            return true;
        }

        /// <summary>
        /// Gets or sets the current capacity of the internal array.
        /// When setting it, if the new capacity is less than the current <see cref="Count"/>, 
        /// an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        public int Capacity
        {
            get { return _tab.Length; }
            set
            {
                if( value > 0 && value < _defaultCapacity ) value = _defaultCapacity;
                if( _tab.Length != value )
                {
                    if( value < _count ) throw new ArgumentException( "Capacity less than Count." );
                    if( value == 0 ) _tab = _empty;
                    else 
                    {
                        T[] tempValues = new T[value];
                        if( _count > 0 ) Array.Copy( _tab, 0, tempValues, 0, _count );
                        _tab = tempValues;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the object at the given index.
        /// </summary>
        /// <param name="index">Zero based position of the item in this list.</param>
        /// <returns>The item.</returns>
        public T this[int index]
        {
            get
            {
                if( index >= _count ) throw new IndexOutOfRangeException();
                return _tab[index];
            }
        }

        /// <summary>
        /// Explicit implementation to hide it as much as possible. 
        /// The setter calls the protected virtual <see cref="DoSet"/> method
        /// that does the job of actually setting the item at the given index... 
        /// even if this breaks the sort.
        /// </summary>
        /// <param name="index">Index of the item.</param>
        /// <returns>The item.</returns>
        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set { DoSet( index, value ); }
        }

        /// <summary>
        /// Explicit implementation to hide it as much as possible. 
        /// It calls the protected virtual <see cref="DoInsert"/> method
        /// that does the job of actually inserting the item at the given index... 
        /// even if this breaks the sort.
        /// </summary>
        /// <param name="index">Future index of the item.</param>
        /// <param name="value">Item to insert.</param>
        void IList<T>.Insert( int index, T value )
        {
            DoInsert( index, value );
        }

        /// <summary>
        /// Adds the item at its right position depending on the comparison function and returns true.
        /// May return false if, for any reason, the item has not been added. At this level (but this 
        /// may be overriden), if <see cref="AllowDuplicates"/> is false and the item already exists,
        /// false is returned and the item is not added.
        /// </summary>
        /// <param name="value">Item to add.</param>
        /// <returns>True if the item has actually been added; otherwise false.</returns>
        public bool Add( T value )
        {
            if( value == null ) throw new ArgumentNullException();
            int index = Util.BinarySearch<T>( _tab, 0, _count, value, Comparator );
            if( index >= 0 )
            {
                if( AllowDuplicates )
                {
                    DoInsert( index, value );
                    return true;
                }
                return false;
            }
            DoInsert( ~index, value );
            return true;
        }

        /// <summary>
        /// Removes the item at the given position.
        /// </summary>
        /// <param name="index">Index to remove.</param>
        public void RemoveAt( int index )
        {
            DoRemoveAt( index );
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            DoClear();
        }

        /// <summary>
        /// Checks that the item at the given index is between a lesser and a greater item and if not, 
        /// moves the item at its correct index.
        /// If the new index conflicts because <see cref="AllowDuplicates"/> is false (the default), a 
        /// negative value is returned, otherwise the new positive index is returned.
        /// </summary>
        /// <param name="index">Index of the element to check.</param>
        /// <returns>
        /// The new positive index if the position has been successfuly updated, or a negative value
        /// if a duplicate exists (and <see cref="AllowDuplicates"/> is false).
        /// </returns>
        public int CheckPosition( int index )
        {
            if( index >= _count ) throw new IndexOutOfRangeException();
            int other = index - 1;
            int cmp;
            if( other >= 0 && (cmp = Comparator( _tab[other], _tab[index] )) >= 0 )
            {
                if( cmp == 0 )
                {
                    return AllowDuplicates ? index : ~index;
                }
                if( other >= 0 )  other = Util.BinarySearch( _tab, 0, other, _tab[index], Comparator );
                if( other >= 0 )
                {
                    if( !AllowDuplicates ) return ~other;
                }
                else other = ~other; 
                index = DoMove( index, other );
            }
            else
            {
                other = index + 1;
                int lenAfter = _count - other;
                if( lenAfter > 0 && (cmp = Comparator( _tab[index], _tab[other] )) >= 0 )
                {
                    if( cmp == 0 )
                    {
                        return AllowDuplicates ? index : ~index;
                    }
                    if( lenAfter >= 1 ) other = Util.BinarySearch( _tab, other, lenAfter, _tab[index], Comparator );
                    if( other >= 0 )
                    {
                        if( !AllowDuplicates ) return ~other;
                    }
                    else other = ~other;
                    index = DoMove( index, other );
                }
            }
            return index;
        }

        /// <summary>
        /// Gives access to the internal array to specialized classes.
        /// </summary>
        protected T[] Store { get { return _tab; } }
        
        /// <summary>
        /// Sets a value at a given position.
        /// </summary>
        /// <param name="index">The position to set.</param>
        /// <param name="newValue">The new item to inject.</param>
        /// <returns>The previous item at the position.</returns>
        protected virtual T DoSet( int index, T newValue )
        {
            if( index >= _count ) throw new IndexOutOfRangeException();
            if( newValue == null ) throw new ArgumentNullException();
            T oldValue = _tab[index];
            _tab[index] = newValue;
            _version += 2;
            return oldValue;
        }

        /// <summary>
        /// Inserts a new item.
        /// </summary>
        /// <param name="index">Index to insert.</param>
        /// <param name="value">Item to insert.</param>
        protected virtual void DoInsert( int index, T value )
        {
            if( value == null ) throw new ArgumentNullException();
            if( index < 0 || index > _count ) throw new IndexOutOfRangeException();
            if( _count == _tab.Length )
            {
                Capacity = _count == 0 ? _defaultCapacity : _tab.Length * 2;
            }
            if( index < _count )
            {
                Array.Copy( _tab, index, _tab, index + 1, _count - index );
            }
            _tab[index] = value;
            _count++;
            _version += 2;
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        protected virtual void DoClear()
        {
            Array.Clear( _tab, 0, _count );
            _count = 0;
            _version += 2;
         }

        /// <summary>
        /// Removes the item at a given position.
        /// </summary>
        /// <param name="index">Index to remove.</param>
        protected virtual void DoRemoveAt( int index )
        {
            int newCount = _count - 1;
            int nbToCopy = newCount - index;
            if( index < 0 || nbToCopy < 0 ) throw new IndexOutOfRangeException();
            if( nbToCopy > 0 ) Array.Copy( _tab, index + 1, _tab, index, nbToCopy );
            _tab[(_count = newCount)] = default( T );
            _version += 2;
        }

        /// <summary>
        /// Moves an item from a position to another one.
        /// </summary>
        /// <param name="from">Old index of the item.</param>
        /// <param name="newIndex">New index.</param>
        /// <returns>The new index of the element.</returns>
        protected virtual int DoMove( int from, int newIndex )
        {
            if( from < 0 || from >= _count ) throw new IndexOutOfRangeException();
            if( newIndex < 0 || newIndex > _count ) throw new IndexOutOfRangeException();
            int lenToMove = newIndex - from;
            if( lenToMove != 0 )
            {
                T temp  = _tab[from];
                if( lenToMove > 0 )
                {
                    if( --lenToMove > 0 ) Array.Copy( _tab, from + 1, _tab, from, lenToMove );
                    _tab[--newIndex] = temp;
                }
                else 
                {
                    lenToMove = -lenToMove;
                    if( lenToMove > 0 ) Array.Copy( _tab, newIndex, _tab, newIndex + 1, lenToMove );
                    _tab[newIndex] = temp;
                }
            }
            // newIndex is equal to the original newIndex or to newIndex - 1.
            return newIndex;
        }

        #region Enumerable

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new E( this );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new E( this );
        }

        private sealed class E : IEnumerator<T>
        {
            readonly CKSortedArrayList<T> _list;
            readonly int _version;
            T _currentValue;
            int _index;

            // Methods
            internal E( CKSortedArrayList<T> l )
            {
                _list = l;
                _version = l._version;
            }

            public void Dispose()
            {
                this._index = 0;
                this._currentValue = default( T );
            }

            public bool MoveNext()
            {
                if( _version != _list._version ) throw new InvalidOperationException( "SortedList changed during enumeration." );
                if( _index < _list.Count )
                {
                    _currentValue = _list._tab[_index++];
                    return true;
                }
                _index = -1;
                _currentValue = default( T );
                return false;
            }

            void IEnumerator.Reset()
            {
                if( _version != _list._version ) throw new InvalidOperationException( "SortedList changed during enumeration." );
                _index = 0;
                _currentValue = default( T );
            }

            public T Current
            {
                get 
                {
                    if( _index <= 0 ) throw new InvalidOperationException();
                    return _currentValue; 
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        #endregion
    }
}
