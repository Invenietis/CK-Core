#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyListEmpty.cs) is part of CiviKey. 
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
    /// Empty read only list. The <see cref="Empty"/> null object is also a <see cref="IList{T}"/>:
    /// by casting it, it also provides an empty read only <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Contained elements type.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.CKReadOnlyCollectionDebuggerView<> ) ), DebuggerDisplay( "Count = 0" )]
    public sealed class CKReadOnlyListEmpty<T> : ICKReadOnlyList<T>, IList<T>
    {
        /// <summary>
        /// Static empty <see cref="CKReadOnlyListEmpty{T}"/>. Can also be used as an 
        /// empty <see cref="IList{T}"/>
        /// </summary>
		static public readonly ICKReadOnlyList<T> Empty = new CKReadOnlyListEmpty<T>();

		private CKReadOnlyListEmpty()
        {
        }

        /// <summary>
        /// Gets the index of the an element: always <see cref="Int32.MinValue"/>.
        /// </summary>
        /// <param name="item">Element to find in the list</param>
        /// <returns>Index of the given element</returns>
        public int IndexOf( object item )
        {
            return Int32.MinValue;
        }

        /// <summary>
        /// Gets an element at the given index: always throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="i">index of the element to find</param>
        /// <returns>New <see cref="IndexOutOfRangeException"/>. 
        /// Because a <see cref="CKReadOnlyListEmpty{T}"/> doesn't contains any elements.</returns>
        public T this[ int i ]
        {
            get { throw new ArgumentOutOfRangeException( "i", "The index is out of the range of acceptable values. The list is empty." ); }
        }

        /// <summary>
        /// Gets always false.
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns>False in all cases, a <see cref="CKReadOnlyListEmpty{T}"/> doesn't contains any elements.</returns>
		public bool Contains( object item )
		{
			return false;
		}

        /// <summary>
        /// Gets the number of items of the list: it will always be 0.
        /// </summary>
		public int Count
		{
			get { return 0; }
		}

        /// <summary>
        /// Gets the underlying empty enumerator.
        /// </summary>
        /// <returns>An empty enumerator.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return CKEnumeratorEmpty<T>.Empty;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
            return CKEnumeratorEmpty<T>.Empty;
		}


        #region IList<T> explicit implementation

        int IList<T>.IndexOf( T item )
        {
            return IndexOf( item );
        }

        void IList<T>.Insert( int index, T item )
        {
        }

        void IList<T>.RemoveAt( int index )
        {
            throw new ArgumentOutOfRangeException( "i", "The index is out of the range of acceptable values. The list is empty." );
        }

        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set
            {
                throw new ArgumentOutOfRangeException( "i", "The index is out of the range of acceptable values. The list is empty." );
            }
        }

        void ICollection<T>.Add( T item )
        {
        }

        void ICollection<T>.Clear()
        {
        }

        bool ICollection<T>.Contains( T item )
        {
            return false;
        }

        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
        {
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove( T item )
        {
            return false;
        }

        #endregion
    }
}

