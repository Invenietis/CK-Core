#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\IPluginStatus.cs) is part of CiviKey. 
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
    /// Describes what's a <see cref="IPluginStatus"/>.
    /// </summary>
    public interface IPluginStatus
    {
        /// <summary>
        /// Gets the unique ID of the plugin
        /// </summary>
        Guid PluginId { get; }

        /// <summary>
        /// Gets ConfigPluginStatus.
        /// </summary>
        ConfigPluginStatus Status { get; set; }

        /// <summary>
        /// It will destroy the plugin status, and remove it from its parent collection.
        /// </summary>
        void Destroy();
    }
}
