#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyListOnIList.cs) is part of CiviKey. 
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

using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Adapts a <see cref="IList{T}"/> object to the <see cref="IReadOnlyList{T}"/> interface.
    /// The other <see cref="ReadOnlyListOnIList{T,TInner}"/> generic can expose (wrap) a list of TInner 
    /// as a readonly list of T where TInner is a T.
    /// </summary>
    /// <typeparam name="T">Type of the element.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.CKReadOnlyCollectionDebuggerView<> ) ), DebuggerDisplay( "Count = {Count}" )]
    public sealed class CKReadOnlyListOnIList<T> : ICKReadOnlyList<T>, IList<T>
    {
		IList<T> _inner;

		/// <summary>
		/// Initializes a new <see cref="CKReadOnlyListOnIList{T}"/> around a <see cref="IList{T}"/>.
		/// </summary>
        /// <param name="inner">List to wrap. Must not be null.</param>
		public CKReadOnlyListOnIList( IList<T> inner )
        {
            if( inner == null ) throw new ArgumentNullException( "inner" ); 
            _inner = inner;
        }

		/// <summary>
		/// Gets or sets the wrapped list. Must not be null (nor itself).
		/// </summary>
        public IList<T> Inner
        {
            get { return _inner; }
            set 
            {
                if( value == null ) throw new ArgumentNullException( "value" );
                if( value == this ) throw new ArgumentException( "Auto reference in adapter.", "value" );
                _inner = value; 
            }
        }

		/// <summary>
		/// Determines the index of a specific item in list.
		/// </summary>
		/// <param name="item">The item to locate in the list.</param>
		/// <returns>The index of item if found in the list; otherwise a negative value (see <see cref="ICKReadOnlyList{T}.IndexOf"/>).</returns>
		public int IndexOf( object item )
        {
            return item is T ? _inner.IndexOf( (T)item ) : Int32.MinValue;
        }

		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <param name="i">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="i"/> is not a valid index in the list.</exception>
        public T this[ int i ]
        {
            get { return _inner[i]; }
        }

		/// <summary>
		/// Whether an item is contained or not.
		/// </summary>
		/// <param name="item">Item to challenge.</param>
		/// <returns>True if the item is contained in the collection.</returns>
		public bool Contains( object item )
		{
			return item is T ? _inner.Contains( (T)item ) : false;
		}

		/// <summary>
		/// Gets the number of items of the collection.
		/// </summary>
		public int Count
		{
			get { return _inner.Count; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _inner.GetEnumerator();
		}


        #region IList<T> Members

        int IList<T>.IndexOf( T item )
        {
            return _inner.IndexOf( item );
        }

        void IList<T>.Insert( int index, T item )
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt( int index )
        {
            throw new NotSupportedException();
        }

        T IList<T>.this[int index]
        {
            get
            {
                return _inner[ index ];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add( T item )
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains( T item )
        {
            return _inner.Contains( item );
        }

        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
        {
            _inner.CopyTo( array, arrayIndex );
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove( T item )
        {
            throw new NotSupportedException();
        }

        #endregion

    }

    /// <summary>
	/// Adapts a <see cref="IList{TInner}"/> object to the <see cref="IReadOnlyList{T}"/> interface
    /// where TInner is a T.
	/// </summary>
    /// <typeparam name="T">Type of the exposed element.</typeparam>
    /// <typeparam name="TInner">Type of the list element.</typeparam>
    /// <remarks>
    /// There is no way to define a beast like <c>ReadOnlyListOnIList&lt;T, TInner&gt; where TInner : T</c> that would 
    /// extend <see cref="IList{TInner}"/> because of the GetEnumerator support.
    /// <para>
    /// The adapter object would have to implement both GetEnumerator() methods (for TInner and T), and even if the constraint states that
    /// TInner is T and the IEnumerator is covariant, this is rejected with the following error: cannot implement 
    /// both 'IEnumerable&lt;T&gt;' and 'System.Collections.Generic.IEnumerable&lt;TInner&gt;' because they may unify 
    /// for some type parameter substitutions.
    /// </para>
    /// </remarks>
    [DebuggerTypeProxy( typeof( Impl.ReadOnlyCollectionDebuggerView<,> ) ), DebuggerDisplay( "Count = {Count}" )]
    public sealed class ReadOnlyListOnIList<T, TInner> : IReadOnlyList<T>, IList<T>
        where TInner : T
    {
		IList<TInner> _inner;

		/// <summary>
		/// Initializes a new <see cref="CKReadOnlyListOnIList{T}"/> around a <see cref="IList{TInner}"/>.
		/// </summary>
		/// <param name="list">List to wrap.</param>
		public ReadOnlyListOnIList( IList<TInner> list )
        {
			_inner = list;
        }

		/// <summary>
		/// Gets or sets the wrapped list.
		/// </summary>
        public IList<TInner> Inner
        {
            get { return _inner; }
            set
            {
                if( value == null ) throw new ArgumentNullException( "value" );
                _inner = value;
            }
        }

		/// <summary>
		/// Determines the index of a specific item in list.
		/// </summary>
		/// <param name="item">The item to locate in the list.</param>
		/// <returns>The index of item if found in the list; otherwise a negative value (see <see cref="ICKReadOnlyList{T}.IndexOf"/>).</returns>
		public int IndexOf( object item )
        {
            return item is TInner ? _inner.IndexOf( (TInner)item ) : Int32.MinValue;
        }

		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <param name="i">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="i"/> is not a valid index in the list.</exception>
        public T this[ int i ]
        {
            get { return _inner[i]; }
        }

		/// <summary>
		/// Whether an item is contained or not.
		/// </summary>
		/// <param name="item">Item to challenge.</param>
		/// <returns>True if the item is contained in the collection.</returns>
		public bool Contains( object item )
		{
			return item is TInner ? _inner.Contains( (TInner)item ) : false;
		}

		/// <summary>
		/// Gets the number of items of the collection.
		/// </summary>
		public int Count
		{
			get { return _inner.Count; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<T> GetEnumerator()
		{
            return (IEnumerator<T>)_inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _inner.GetEnumerator();
		}


        #region IList<T> Members

        int IList<T>.IndexOf( T item )
        {
            return IndexOf( item );
        }

        void IList<T>.Insert( int index, T item )
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt( int index )
        {
            throw new NotSupportedException();
        }

        T IList<T>.this[int index]
        {
            get
            {
                return _inner[ index ];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add( T item )
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains( T item )
        {
            return Contains( item );
        }

        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
        {
            for( int i = 0; i < _inner.Count; ++i )
                array[i+arrayIndex] = _inner[i];
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove( T item )
        {
            throw new NotSupportedException();
        }

        #endregion

    }
}

