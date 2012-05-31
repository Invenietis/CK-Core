#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\IPluginRequirementCollection.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Core;
using System.ComponentModel;

namespace CK.Plugin
{
    /// <summary>
    /// Plugin requirements associates plugin <see cref="Guid"/> to <see cref="RunningRequirement"/>.
    /// </summary>
    public interface IPluginRequirementCollection : IReadOnlyCollection<PluginRequirement>
    {
        /// <summary>
        /// Fires before a change occurs.
        /// </summary>
        event EventHandler<PluginRequirementCollectionChangingEventArgs> Changing;

        /// <summary>
        /// Fires when a change occured.
        /// </summary>
        event EventHandler<PluginRequirementCollectionChangedEventArgs> Changed;

        /// <summary>
        /// Add or set the given requirement to the given plugin (by its UniqueID).
        /// </summary>
        /// <param name="pluginId">Identifier of the plugin to configure.</param>
        /// <param name="requirement">Requirement to add or set.</param>
        /// <returns>New or updated requirement. May be unchanged if <see cref="Changing"/> canceled the action.</returns>
        PluginRequirement AddOrSet( Guid pluginId, RunningRequirement requirement );
        
        /// <summary>
        /// Gets the <see cref="PluginRequirement"/> for the given plugin (by its unique identifier).
        /// </summary>
        /// <param name="pluginId">Unique identifier of the plugin to find.</param>
        /// <returns>Found requirement (if any, null otherwise).</returns>
        PluginRequirement Find( Guid pluginId );

        /// <summary>
        /// Removes the given requirement.
        /// When no explicit requirement exists, <see cref="RunningRequirement.Optional"/> is the default.
        /// </summary>
        /// <param name="pluginId">Unique identifier of the plugin to remove.</param>
        /// <returns>True if the element does not exist or has been successfully removed. False if <see cref="Changing"/> canceled the action.</returns>
        bool Remove( Guid pluginId );

        /// <summary>
        /// Clears all requirements.
        /// </summary>
        bool Clear();
    }
}
