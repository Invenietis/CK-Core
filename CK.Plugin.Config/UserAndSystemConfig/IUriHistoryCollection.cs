#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\IUriHistoryCollection.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.ComponentModel;
using System.Collections.Specialized;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Used by the <see cref="ISystemConfiguration"/> and <see cref="IUserConfiguration"/> to keep an history of all user profiles 
    /// and all contexts used. The <see cref="IUriHistory"/> items are considered as a list: the first, top item (the one at index 0)
    /// is considered to be the current one.
    /// </summary>
    public interface IUriHistoryCollection : IReadOnlyList<IUriHistory>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current, top <see cref="IUriHistory"/>. 
        /// Can be null only if this list is empty.
        /// </summary>
        IUriHistory Current { get; }

        /// <summary>
        /// Gets a <see cref="IUriHistory"/> by its address.
        /// </summary>
        /// <param name="address">The address to find.</param>
        /// <returns>An existing <see cref="IUriHistory"/> or null if not found.</returns>
        IUriHistory Find( Uri address );

        /// <summary>
        /// Finds or adds a new <see cref="IUriHistory"/> in the collection.
        /// If there is no <see cref="Current"/> the new element (and first) becomes the current one.
        /// </summary>
        /// <param name="address">The address to find.</param>
        /// <returns>An existing <see cref="IUriHistory"/> or a new one if not found.</returns>
        IUriHistory FindOrCreate( Uri address );

        /// <summary>
        /// Removes an existing <see cref="IUriHistory"/>.
        /// If it is the <see cref="Current"/> one, the next element becomes the current one.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        void Remove( IUriHistory entry );

    }
}
