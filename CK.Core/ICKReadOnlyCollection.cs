#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ICKReadOnlyCollection.cs) is part of CiviKey. 
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

using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Represents a generic read only collections of objects with a contravariant <see cref="Contains"/> method.
    /// This enables collection implementing this interface to support better lookup complexity than O(n) if possible. 
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public interface ICKReadOnlyCollection<out T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Determines whether collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to find in the collecion.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        bool Contains( object item );

    }
}
