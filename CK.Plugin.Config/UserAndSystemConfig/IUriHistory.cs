#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\IUriHistory.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{

    /// <summary>
    /// Describes a user profile identified by its <see cref="Address"/>.
    /// </summary>
    public interface IUriHistory
    {
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> itself.
        /// This is the key of the entry: there can be only one entry with a given address, setting
        /// an address to one that already exist in the list, removes the previous one.
        /// </summary>
        Uri Address { get; set; }

        /// <summary>
        /// Friendly name of the entry. Defaults to the user name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the index in the <see cref="IUriHistoryCollection"/>.
        /// Changing this index changes the other indices.
        /// </summary>
        int Index { get; set; }

    }
}