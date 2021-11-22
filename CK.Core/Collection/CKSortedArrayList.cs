using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Simple sorted array list implementation that supports covariance through <see cref="ICKReadOnlyList{T}"/> and contra-variance 
    /// with <see cref="ICKWritableCollection{T}"/>. This is a "dangerous" class since to keep the correct ordering, <see cref="CheckPosition(int)"/> 
    /// must be explicitly called whenever something changes on any item that impacts the <see cref="Comparator"/> result.
    /// See the remarks for other caveats.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements <see cref="IList{T}"/> both for performance (unfortunately Linq relies -too much- on it) and for interoperability reasons: this
    /// interface should NOT be used. Accessors of the <see cref="IList{T}"/> that defeats the invariant of this class (the fact that elements are sorted, such 
    /// as <see cref="IList{T}.Insert"/>) are explicitly implemented to hide them as much as possible.
    /// </para>
    /// <para>
    /// This is the base class for <see cref="CKSortedArrayKeyList{T,TKey}"/>.
    /// </para>
    /// <para>
    /// Specialized classes may use protected <see cref="Store"/>, <see cref="StoreCount"/> and <see cref="StoreVersion"/> to have a direct, uncontrolled, access
    /// to the whole state of this object.
    /// </para>
    /// </remarks>
    public class CKSortedArrayList<T> : IList<T>, IReadOnlyList<T>, ICKWritableCollection<T>
    {
        const int _defaultCapacity = 4;

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
        /// This is why it is explicitly defined.
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
            _tab = Array.Empty<T>();
            Comparator = comparison;
            if( allowDuplicates ) _version = 1;
        }

        /// <summary>
        /// Explicitly implemented since our <see cref="Add"/> method
        /// returns a boolean.
        /// </summary>
        /// <param name="item">Item to add.</param>
        void ICollection<T>.Add( T item ) => Add( item );

        /// <summary>
        /// Gets whether this list allows duplicated items.
        /// </summary>
        public bool AllowDuplicates  => (_version & 1) != 0; 

        /// <summary>
        /// Locates an element (one of the occurrences when duplicates are allowed) in this list (logarithmic). 
        /// </summary>
        /// <param name="value">The element.</param>
        /// <returns>The result of the <see cref="Util.BinarySearch{T}(IReadOnlyList{T}, int, int, T, Comparison{T})"/> in the internal array.</returns>
        public virtual int IndexOf( T value )
        {
            if( value is null ) ThrowHelper.ThrowArgumentNullException( nameof( value ) );
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
            if( comparison == null ) throw new ArgumentNullException( nameof( comparison ) );
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
            return item is T i ? IndexOf( i ) : Int32.MinValue;
        }

        /// <summary>
        /// Covariant compatible overload of <see cref="Contains(T)"/>  (logarithmic).
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>True if the object is found; otherwise, false.</returns>
        public virtual bool Contains( object item )
        {
            return item is T i && Contains( i );
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
        public int Count => _count; 

        /// <summary>
        /// Explicit implementation that always returns false.
        /// </summary>
        bool ICollection<T>.IsReadOnly => false; 

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
                    if( value == 0 ) _tab = Array.Empty<T>();
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
        /// may be overridden), if <see cref="AllowDuplicates"/> is false and the item already exists,
        /// false is returned and the item is not added.
        /// </summary>
        /// <param name="value">Item to add.</param>
        /// <returns>True if the item has actually been added; otherwise false.</returns>
        public virtual bool Add( T value )
        {
            if( value == null ) throw new ArgumentNullException( nameof( value ) );
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
        /// This is not virtual since it directly calls protected virtual <see cref="DoRemoveAt(int)"/>
        /// </summary>
        /// <param name="index">Index to remove.</param>
        public void RemoveAt( int index )
        {
            DoRemoveAt( index );
        }

        /// <summary>
        /// Clears the list.
        /// This is not virtual since it directly calls protected virtual <see cref="DoClear()"/>
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
        /// The new positive index if the position has been successfully updated, or a negative value
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
        /// Direct access to the internal array to specialized classes.
        /// This must be used with care.
        /// </summary>
        protected T[] Store 
        {
            get { return _tab; }
            set { _tab = value; } 
        }

        /// <summary>
        /// Direct access to the <see cref="Count"/> to specialized classes.
        /// This must be used with care.
        /// </summary>
        protected int StoreCount
        {
            get { return _count; }
            set { _count = value; }
        }

        /// <summary>
        /// Direct access to the internal version to specialized classes.
        /// LSB (StoreVersion &amp;  1) is <see cref="AllowDuplicates"/>: the version 
        /// is incremented by two whenever the content change.
        /// This must be used with care.
        /// </summary>
        protected int StoreVersion
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Sets a value at a given position.
        /// </summary>
        /// <param name="index">The position to set.</param>
        /// <param name="newValue">The new item to inject.</param>
        /// <returns>The previous item at the position.</returns>
        protected virtual T DoSet( int index, T newValue )
        {
            if( index >= _count ) throw new IndexOutOfRangeException();
            if( newValue == null ) throw new ArgumentNullException( nameof( newValue ) );
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
            if( value == null ) throw new ArgumentNullException( nameof( value ) );
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
            _count = newCount;
#if NETSTANDARD2_1
            if( System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
            {
                _tab[newCount] = default!;
            }
#else
            _tab[newCount] = default!;
#endif
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
            return new Enumerator( this );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator( this );
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly CKSortedArrayList<T> _list;
            private int _index;
            private readonly int _version;
            private T? _current;

            internal Enumerator( CKSortedArrayList<T> list )
            {
                _list = list;
                _index = 0;
                _version = list._version;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var localList = _list;

                if( _version == localList._version && ((uint)_index < (uint)localList._count) )
                {
                    _current = localList._tab[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            bool MoveNextRare()
            {
                if( _version != _list._version )
                {
                    throw new InvalidOperationException( "Collection was modified; enumeration operation may not execute." );
                }
                _index = _list._count + 1;
                _current = default;
                return false;
            }

            public T Current => _current!;

            object? IEnumerator.Current
            {
                get
                {
                    if( _index == 0 || _index == _list._count + 1 )
                    {
                        ThrowEnumNotpossible();
                    }
                    return Current;
                }
            }

            [DoesNotReturn]
            static void ThrowEnumNotpossible()
            {
                throw new InvalidOperationException( "Enumeration has either not started or has already finished." );
            }

            void IEnumerator.Reset()
            {
                if( _version != _list._version )
                {
                    throw new InvalidOperationException( "Collection was modified; enumeration operation may not execute." );
                }
                _index = 0;
                _current = default;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    #endregion
}

