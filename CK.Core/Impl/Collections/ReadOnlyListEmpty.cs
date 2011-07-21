#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\ReadOnlyListEmpty.cs) is part of CiviKey. 
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
    /// Empty read only list.
    /// </summary>
    /// <typeparam name="T">Contained elements type.</typeparam>
    public sealed class ReadOnlyListEmpty<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Static empty <see cref="ReadOnlyListEmpty{T}"/>
        /// that will be used by the entire system.
        /// </summary>
		static public readonly IReadOnlyList<T> Empty = new ReadOnlyListEmpty<T>();

        /// <summary>
        /// Constructor
        /// </summary>
		public ReadOnlyListEmpty()
        {
        }

        /// <summary>
        /// Gets the index of the given element into the list.
        /// </summary>
        /// <param name="item">Element to find in the list</param>
        /// <returns>Index of the given element</returns>
        public int IndexOf( object item )
        {
            return -1;
        }

        /// <summary>
        /// Gets an element at the given index.
        /// </summary>
        /// <param name="i">index of the element to find</param>
        /// <returns>New <see cref="IndexOutOfRangeException"/>. 
        /// Because a <see cref="ReadOnlyListEmpty{T}"/> doesn't contains any elements.</returns>
        public T this[ int i ]
        {
            get { throw new ArgumentOutOfRangeException( "i", "The index is out of the range of acceptable values. The list is empty." ); }
        }

        /// <summary>
        /// Gets if the given element is contained into the list.
        /// </summary>
        /// <param name="item">Element to find</param>
        /// <returns>False in all cases, a <see cref="ReadOnlyListEmpty{T}"/> doesn't contains any elements.</returns>
		public bool Contains( object item )
		{
			return false;
		}

        /// <summary>
        /// Gets the count of the list.
        /// It will be 0 in all cases.
        /// </summary>
		public int Count
		{
			get { return 0; }
		}

        /// <summary>
        /// Gets the underlying enumerator.
        /// </summary>
        /// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return EnumEmpty<T>.Empty;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
            return EnumEmpty<T>.Empty;
		}

	}
}

