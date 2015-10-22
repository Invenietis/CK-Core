#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyExtension.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="ICKReadOnlyCollection{T}"/>, <see cref="ICKReadOnlyList{T}"/> and <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}"/>.
    /// </summary>
    public static class CKReadOnlyExtension
    {
        /// <summary>
        /// Gets the item with the associated key, forgetting the exists out parameter in <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}.GetByKey(TKey,out bool)"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="this">Keyed collection of elements.</param>
        /// <param name="key">The item key.</param>
        /// <returns>The item that matches the key, default(T) if the key can not be found.</returns>
        static public T GetByKey<T, TKey>( this ICKReadOnlyUniqueKeyedCollection<T, TKey> @this, TKey key )
        {
            bool exists;
            return @this.GetByKey( key, out exists );
        }

        /// <summary>
        /// Creates an array from a read only collection.
        /// This is a much more efficient version than the IEnumerable ToArray extension method
        /// since this implementation allocates one and only one array. 
        /// </summary>
        /// <typeparam name="T">Type of the array and lists elements.</typeparam>
        /// <param name="this">Read only collection of elements.</param>
        /// <returns>A new array that contains the same element as the collection.</returns>
        static public T[] ToArray<T>( this IReadOnlyCollection<T> @this )
        {
            T[] r = new T[@this.Count];
            int i = 0;
            foreach( T item in @this ) r[i++] = item;
            return r;
        }

    }
}
