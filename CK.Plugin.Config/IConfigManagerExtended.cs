#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\IConfigManagerExtended.cs) is part of CiviKey. 
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
using CK.SharedDic;
using System;
using CK.Storage;
using CK.Core;

namespace CK.Plugin.Config
{
    public interface IConfigManagerExtended
    {
        /// <summary>
        /// Gets the simple configuration object.
        /// </summary>
        IConfigManager ConfigManager { get; }

        /// <summary>
        /// Gets the <see cref="IConfigContainer"/> that gives access to the global
        /// configuration (for any object and any plugin).
        /// </summary>
        IConfigContainer Container { get; }

        /// <summary>
        /// Gets the host dictionary for System wide configuration.
        /// </summary>
        IObjectPluginConfig HostSystemConfig { get; }

        /// <summary>
        /// Gets the host dictionary for current user configuration.
        /// </summary>
        IObjectPluginConfig HostUserConfig { get; }

        /// <summary>
        /// Gets the <see cref="INamedVersionedUniqueId"/> that represents the configuration itself.
        /// </summary>
        INamedVersionedUniqueId ConfigPluginId { get; }

        /// <summary>
        /// Gets whether the system configuration file should be saved.
        /// </summary>
        bool IsSystemConfigDirty { get; }

        /// <summary>
        /// Gets whether the user configuration should be saved (if it has changed from the last call to <see cref="LoadUserConfig"/>).
		/// </summary>
		bool IsUserConfigDirty { get; }

		/// <summary>
		/// Loads the system configuration from a stream. Current settings are cleared and if the stream is null or empty,
		/// the configuration remains empty and null is returned.
		/// </summary>
        /// <returns>A list (possibly empty) of <see cref="ISimpleErrorMessage"/> describing read errors.</returns>
		IReadOnlyList<ISimpleErrorMessage> LoadSystemConfig( IStructuredReader reader );

		/// <summary>
		/// Loads the user configuration from a stream. Current settings are cleared and if the stream is null or empty,
		/// the configuration remains empty and null is returned.
		/// Only &lt;User&gt; element is read.
		/// </summary>
        /// <returns>A list (possibly empty) of <see cref="ISimpleErrorMessage"/> describing read errors.</returns>
        IReadOnlyList<ISimpleErrorMessage> LoadUserConfig( IStructuredReader reader );

		/// <summary>
		/// Writes the user config to the given stream.
		/// </summary>
        void SaveUserConfig( IStructuredWriter writer );
        
        /// <summary>
        /// Writes the system config to the given stream.
        /// </summary>
        void SaveSystemConfig( IStructuredWriter writer );

        /// <summary>
        /// Fires whenever the system needs to load User configuration.
        /// </summary>
        event EventHandler LoadUserConfigRequired;

        /// <summary>
        /// Fires whenever the system needs to load System configuration.
        /// </summary>
        event EventHandler LoadSystemConfigRequired;

	}
}
