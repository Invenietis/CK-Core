#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyListMono.cs) is part of CiviKey. 
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
    /// Implements a mono element <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element in the read only list.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.CKReadOnlyCollectionDebuggerView<> ) ), DebuggerDisplay( "Count = 1" )]
    public sealed class CKReadOnlyListMono<T> : ICKReadOnlyList<T>
    {
        T _val;

        /// <summary>
        /// Initializes a new <see cref="IReadOnlyList{T}"/> with one element (that can be null if <typeparamref name="T"/> is a reference type).
        /// </summary>
        /// <param name="val">Element contained by the <see cref="CKReadOnlyListMono{T}"/> (can be null).</param>
		public CKReadOnlyListMono( T val )
        {
            _val = val;
        }

        /// <summary>
        /// Gets the index of the given item (uses <see cref="EqualityComparer{T}.Default"/>).
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns>Index of the item (for this implementation, it is necessarily 0) or <see cref="Int32.MinValue"/> if it is not found</returns>
        public int IndexOf( object item )
        {
            return item is T ? (EqualityComparer<T>.Default.Equals( _val, (T)item ) ? 0 : Int32.MinValue) : Int32.MinValue;
        }

        /// <summary>
        /// Gets the item at the given index.
        /// </summary>
        /// <param name="i">Index of the item to find.</param>
        /// <returns>Found item at the index. If the index is not 0 an <see cref="IndexOutOfRangeException"/> will be thrown.</returns>
        public T this[ int i ]
        {
            get { if( i != 0 ) throw new ArgumentOutOfRangeException( "i", "The index is out of the range of acceptable values. Acceptable value is 0: this is a mono implementation"); return _val; }
        }

        /// <summary>
        /// Gets if the given item is contained into the list (uses <see cref="EqualityComparer{T}.Default"/>).
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns>True if the item is found, false otherwise.</returns>
		public bool Contains( object item )
		{
            return item is T ? EqualityComparer<T>.Default.Equals( _val, (T)item ) : false;
		}

        /// <summary>
        /// Gets the count of the list, 1 in all cases.
        /// </summary>
		public int Count
		{
			get { return 1; }
		}

        /// <summary>
        /// Gets the underlying enumerator, <see cref="CKEnumeratorMono{T}"/> actually.
        /// </summary>
        /// <returns>An enumerator.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new CKEnumeratorMono<T>( _val );
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

	}
}

