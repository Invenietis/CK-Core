#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\ReadOnlyListOnIList.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections;
using System.Collections.Generic;
using System;

namespace CK.Core
{
	/// <summary>
	/// Adapts a <see cref="IList{T}"/> object to the <see cref="IReadOnlyList{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">Type of the element.</typeparam>
	public sealed class ReadOnlyListOnIList<T> : IReadOnlyList<T>, IList<T>
    {
		IList<T> _inner;

		/// <summary>
		/// Initializes a new <see cref="ReadOnlyListOnIList{T}"/> around a <see cref="IList{T}"/>.
		/// </summary>
		/// <param name="list">List to wrap.</param>
		public ReadOnlyListOnIList( IList<T> list )
        {
			_inner = list;
        }

		/// <summary>
		/// Gets the wrapped list.
		/// </summary>
        public IList<T> Inner
        {
            get { return _inner; }
        }

		/// <summary>
		/// Determines the index of a specific item in list.
		/// </summary>
		/// <param name="item">The item to locate in the list.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf( object item )
        {
            return item is T ? _inner.IndexOf( (T)item ) : -1;
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

        /// <summary>
        /// Obsolete, use ToReadOnlyList or ToReadOnlyCollection extension method instead.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [Obsolete( "Use ToReadOnlyList or ToReadOnlyCollection extension method instead.", true )]
        static public IReadOnlyList<T> NewImmutableCopy( IList<T> list )
        {
            if( list == null || list.Count == 0 ) return ReadOnlyListEmpty<T>.Empty;
            if( list.Count == 1 ) return new ReadOnlyListMono<T>( list[0] );
            T[] t = new T[list.Count];
            list.CopyTo( t, 0 );
            return new ReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Obsolete, use ToReadOnlyList or ToReadOnlyCollection extension method instead.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="list"></param>
        /// <param name="convertor"></param>
        /// <returns></returns>
        [Obsolete( "Use ToReadOnlyList or ToReadOnlyCollection extension method instead.", true )]
        static public IReadOnlyList<T> NewImmutableCopy<U>( IList<U> list, Func<U, T> convertor )
        {
            if( convertor == null ) throw new ArgumentNullException( "convertor" );

            if( list == null || list.Count == 0 ) return ReadOnlyListEmpty<T>.Empty;
            if( list.Count == 1 ) return new ReadOnlyListMono<T>( convertor( list[0] ) );
            T[] t = new T[list.Count];
            for( int i = 0; i < t.Length; ++i ) t[i] = convertor( list[i] );
            return new ReadOnlyListOnIList<T>( t );
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
}

