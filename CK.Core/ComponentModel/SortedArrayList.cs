using System;
using System.Collections;
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Simple sorted array list implementation that supports covariance through <see cref="IReadOnlyList{T}"/> and contravariance 
    /// with <see cref="IWritableCollection{T}"/>.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IList{T}"/> both for performance (unfortunately Linq relies -too much- on it) and for interoperability reasons: this
    /// interface should NOT be used. Accessors of the <see cref="IList{T}"/> that defeats the invariant of this class (the fact that elements are sorted, such 
    /// as <see cref="IList{T}.Insert"/>) are explicitely implemented to hide them as much as possible.
    /// </remarks>
    public class SortedArrayList<T> : IList<T>, IReadOnlyList<T>, IWritableCollection<T>
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
        /// Initializes a new <see cref="SortedArrayList{T}"/> that rejects duplicates 
        /// and uses the <see cref="Comparer<T>.Default"/> comparer.
        /// </summary>
        /// <remarks>
        /// A default constructor is a parameterless constructor, it is not the same as a constructor with default parameter values.
        /// This is why it is explicitely defined.
        /// </remarks>
        public SortedArrayList()
            : this( Comparer<T>.Default.Compare, false )
        {
        }

        public SortedArrayList( bool allowDuplicates )
            : this( Comparer<T>.Default.Compare, allowDuplicates )
        {
        }

        public SortedArrayList( IComparer<T> comparer, bool allowDuplicates = false )
            : this( comparer.Compare, allowDuplicates )
        {
        }

        public SortedArrayList( Comparison<T> comparison, bool allowDuplicates = false )
        {
            _tab = _empty;
            Comparator = comparison;
            if( allowDuplicates ) _version = 1;
        }

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

        public virtual int IndexOf( T value )
        {
            if( value == null ) throw new ArgumentNullException();
            return Util.BinarySearch<T>( _tab, 0, _count, value, Comparator );
        }

        public bool Contains( T value )
        {
            return IndexOf( value ) >= 0;
        }

        public virtual int IndexOf( object item )
        {
            return item is T ? IndexOf( (T)item ) : Int32.MinValue;
        }

        public virtual bool Contains( object item )
        {
            return item is T ? Contains( (T)item ) : false;
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            Array.Copy( _tab, 0, array, arrayIndex, _count );
        }

        public int Count
        {
            get { return _count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove( T value )
        {
            int index = IndexOf( value );
            if( index < 0 ) return false;
            RemoveAt( index );
            return true;
        }

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

        public T this[int index]
        {
            get
            {
                if( index >= _count ) throw new IndexOutOfRangeException();
                return _tab[index];
            }
        }

        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set { DoSet( index, value ); }
        }

        void IList<T>.Insert( int index, T value )
        {
            DoInsert( index, value );
        }

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

        public void RemoveAt( int index )
        {
            DoRemoveAt( index );
        }

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
                if( other > 0 ) other = Util.BinarySearch( _tab, 0, other, _tab[index], Comparator );
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
                    if( lenAfter > 1 ) other = Util.BinarySearch( _tab, other, lenAfter, _tab[index], Comparator );
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

        protected T[] Store { get { return _tab; } }
        
        protected virtual T DoSet( int index, T newValue )
        {
            if( index >= _count ) throw new IndexOutOfRangeException();
            if( newValue == null ) throw new ArgumentNullException();
            T oldValue = _tab[index];
            _tab[index] = newValue;
            _version += 2;
            return oldValue;
        }

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

        protected virtual void DoClear()
        {
            Array.Clear( _tab, 0, _count );
            _count = 0;
            _version += 2;
         }

        protected virtual void DoRemoveAt( int index )
        {
            int newCount = _count - 1;
            int nbToCopy = newCount - index;
            if( index < 0 || nbToCopy < 0 ) throw new IndexOutOfRangeException();
            if( nbToCopy > 0 ) Array.Copy( _tab, index + 1, _tab, index, nbToCopy );
            _tab[(_count = newCount)] = default( T );
            _version += 2;
        }

        protected virtual int DoMove( int from, int newIndex )
        {
            if( from < 0 || newIndex < 0 ) throw new IndexOutOfRangeException();
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
            return newIndex;
        }

        #region Enumerable

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
            readonly SortedArrayList<T> _list;
            readonly int _version;
            T _currentValue;
            int _index;

            // Methods
            internal E( SortedArrayList<T> l )
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
