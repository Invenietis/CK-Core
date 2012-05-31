#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\IPluginStatusCollection.cs) is part of CiviKey. 
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
using CK.Core;
using System.ComponentModel;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Can be used by all objects that whants to keep a collection of <see cref="IPluginStatus"/>.
    /// Typically system or user configuration.
    /// </summary>
    public interface IPluginStatusCollection : IReadOnlyCollection<IPluginStatus>
    {
        event EventHandler<PluginStatusCollectionChangedEventArgs> Changed;

        event EventHandler<PluginStatusCollectionChangingEventArgs> Changing;

        /// <summary>
        /// Sets the given <see cref="ConfiguPluginStatus"/> on the given plugin (by its ID).
        /// </summary>
        void SetStatus( Guid pluginID, ConfigPluginStatus status );

        /// <summary>
        /// Gets the plugin status of the given plugin (by its ID). 
        /// If no plugin status had been set for the given plugin, returns the given default status.
        /// </summary>
        /// <param name="pluginID">Plugin identifier.</param>
        /// <param name="defaultStatus">Default status if the plugin is not configured.</param>
        /// <returns>The status of the plugin.</returns>
        ConfigPluginStatus GetStatus( Guid pluginID, ConfigPluginStatus defaultStatus );

        /// <summary>
        /// Gets the <see cref="IPluginStatus"/> related for the given id. Can be null.
        /// </summary>
        /// <param name="pluginID"></param>
        /// <returns></returns>
        IPluginStatus GetPluginStatus( Guid pluginID );

        /// <summary>
        /// Removes the status from the configuration.
        /// </summary>
        /// <param name="pluginID">Plugin identifier.</param>
        void Clear( Guid pluginID );

        /// <summary>
        /// Removes all the status from the configuration.
        /// </summary>
        void Clear();

    }
}
