#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\IPluginHost.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Host for <see cref="IPlugin"/> management.
    /// </summary>
    public interface IPluginHost
    {
        /// <summary>
        /// Checks whether a plugin is running or not.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsPluginRunning( IPluginInfo key );

        /// <summary>
        /// Gets the <see cref="IPluginProxy"/> for the plugin identifier. 
        /// It may find plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        /// <param name="pluginId">Plugin identifier.</param>
        /// <param name="checkCurrentlyLoading">True to take into account plugins beeing loaded during an <see cref="Execute"/> phasis.</param>
        /// <returns>Null if not found.</returns>
        IPluginProxy FindLoadedPlugin( Guid pluginId, bool checkCurrentlyLoading );

        /// <summary>
        /// Gets the loaded plugins. This contains also the plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        IReadOnlyCollection<IPluginProxy> LoadedPlugins { get; }

        /// <summary>
        /// Fires whenever a plugin status changed.
        /// </summary>
        event EventHandler<PluginStatusChangedEventArgs> StatusChanged;

    }
}
