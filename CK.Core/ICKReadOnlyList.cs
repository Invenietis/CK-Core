#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\LegacySupport\IReadOnlyList.cs) is part of CiviKey. 
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
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Represents a read only collection of objects that can be individually accessed by index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface ICKReadOnlyList<out T> : ICKReadOnlyCollection<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// Determines the index of a specific item in the list.
        /// </summary>
        /// <param name="item">The object to locate in the list.</param>
        /// <returns>
        /// A positive index of the item in this list, if found, a negative index otherwise.
        /// If this list is sorted, this must work exactly like <see cref="Array.BinarySearch(Array,object)"/>: if the item is not found and could be added, the result 
        /// is a negative number which is the bitwise complement of the index at which the new item should be inserted.
        /// To handle the case where the item can NOT be inserted and to be consistent with the positive/negative index semantics, the <see cref="Int32.MinValue"/>
        /// must be returned. See remarks.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The fact that <see cref="Int32.MinValue"/> is used to indicate an "impossible index" enables this covariant list to gracefully handle 
        /// the case where the item's type is more general that the actual type list.
        /// </para>
        /// <para>
        /// Similarly, if the implementation is associated to any kind of filters, returning <see cref="Int32.MinValue"/> instead of any other negative value
        /// indicates to the caller that the item does not appear in the list, but more than that, that it CAN NOT appear.
        /// </para>
        /// <para>
        /// Note that an implementation can perfectly ignore these guidelines and returns -1 (typically) for any unexisting items: it is not an obligation
        /// to challenge any possible filters or constraints inside this IndexOf method.
        /// </para>
        /// <para>
        /// On the other hand, if this method returns <see cref="Int32.MinValue"/> then it MUST mean that the item can NOT appear in this list.
        /// </para>
        /// </remarks>
        int IndexOf( object item );

    }
}
