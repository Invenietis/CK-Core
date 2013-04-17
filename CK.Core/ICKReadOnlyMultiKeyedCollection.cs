#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ICKReadOnlyMultiKeyedCollection.cs) is part of CiviKey. 
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
    /// Represents a generic read only keyed collections of covariant items with
    /// a contravariant key that can support duplicate items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key associated to the elements.</typeparam>
    public interface ICKReadOnlyMultiKeyedCollection<out T, in TKey> : ICKReadOnlyUniqueKeyedCollection<T,TKey>
    {
        /// <summary>
        /// Gets whether this collection supports duplicates.
        /// </summary>
        bool AllowDuplicates { get; }

        /// <summary>
        /// Gets the number of items in this keyed collection that are associated to the
        /// given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>Number of items with the <paramref name="key"/>.</returns>
        int KeyCount( TKey key );

        /// <summary>
        /// Gets an independant collection of the items that 
        /// are associated to the given key value.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>An independant collection of <typeparamref name="T"/>.</returns>
        IReadOnlyCollection<T> GetAllByKey( TKey key );
    }
}
