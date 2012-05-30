#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\ISolvedPluginConfiguration.cs) is part of CiviKey. 
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
using CK.SharedDic;
using CK.Storage;
using System.ComponentModel;
using CK.Core;

namespace CK.Plugin.Config
{

    /// <summary>
    /// Event argument for <see cref="ISolvedPluginConfiguration.Changed"/>.
    /// </summary>
    public class SolvedPluginConfigurationChangedEventArs : EventArgs
    {
        /// <summary>
        /// Gets the only element that changed. Null if a <see cref="GlobalChange"/> occured.
        /// </summary>
        public SolvedPluginConfigElement SolvedPluginConfigElement { get; private set; }

        /// <summary>
        /// Gets whether the change concerns more than one plugin.
        /// </summary>
        public bool GlobalChange { get { return SolvedPluginConfigElement == null; } }

        /// <summary>
        /// Initializes a new <see cref="SolvedPluginConfigurationChangedEventArs"/>.
        /// </summary>
        /// <param name="e">The element that changed (null for a global change).</param>
        public SolvedPluginConfigurationChangedEventArs( SolvedPluginConfigElement e )
        {
            SolvedPluginConfigElement = e;
        }
    }

    public interface ISolvedPluginConfiguration : IReadOnlyCollection<SolvedPluginConfigElement>
    {
        /// <summary>
        /// Fires whenever a configuration changed.
        /// </summary>
        event EventHandler<SolvedPluginConfigurationChangedEventArs> Changed;

        /// <summary>
        /// Gets the plugin status of the given plugin (by its identifier). 
        /// If no plugin status had been set for the plugin, returns <see cref="SolvedConfigStatus.Optional"/>.
        /// </summary>
        /// <param name="pluginID">Plugin identifier.</param>
        /// <returns>The configuration status, <see cref="SolvedConfigStatus.Optional"/> if no configuration exists for the plugin.</returns>
        SolvedConfigStatus GetStatus( Guid pluginID );

        /// <summary>
        /// Gets the <see cref="SolvedPluginConfigElement"/> of the given plugin (by its identifier). 
        /// </summary>
        /// <param name="pluginID">Plugin identifier.</param>
        /// <returns>Null if no configuration exists.</returns>
        SolvedPluginConfigElement Find( Guid pluginId );

    }

}
