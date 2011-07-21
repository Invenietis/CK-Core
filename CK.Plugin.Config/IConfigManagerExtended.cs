#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\IConfigManager.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
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
        /// Gets whether the user configuration file should be saved (if it has changed from the last call to <see cref="LoadUserConfig"/>).
		/// </summary>
		bool IsUserConfigDirty { get; }

        /// <summary>
        /// Gets whether the system configuration file should be saved.
        /// </summary>
        bool IsSystemConfigDirty { get; }

		/// <summary>
		/// Loads the system configuration from a stream. Current settings are cleared and if the stream is null or empty,
		/// the configuration remains empty and null is returned.
		/// </summary>
		/// <returns>A list (possibly empty) of <see cref="XmlReadElementObjectInfo"/> describing read errors.</returns>
		IList<ReadElementObjectInfo> LoadSystemConfig( IStructuredReader reader );

		/// <summary>
		/// Loads the user configuration from a stream. Current settings are cleared and if the stream is null or empty,
		/// the configuration remains empty and null is returned.
		/// Only &lt;User&gt; element is read.
		/// </summary>
		/// <returns>A list (possibly empty) of <see cref="XmlReadElementObjectInfo"/> describing read errors.</returns>
        IList<ReadElementObjectInfo> LoadUserConfig( IStructuredReader reader, IUserProfile setLastProfile );

		/// <summary>
		/// Writes the user config to the given stream.
		/// </summary>
        void SaveUserConfig( IStructuredWriter writer );
        
        /// <summary>
        /// Writes the system config to the given stream.
        /// </summary>
        void SaveSystemConfig( IStructuredWriter writer );

        /// <summary>
        /// Triggers the <see cref="SaveUserConfigRequired"/> event.
        /// </summary>
        void FireSaveUserConfigRequired();

        /// <summary>
        /// Triggers the <see cref="SaveSystemConfigRequired"/> event.
        /// </summary>
        void FireSaveSystemConfigRequired();

        /// <summary>
        /// Fires whenever the system needs to write User configuration.
        /// </summary>
        event EventHandler SaveUserConfigRequired;

        /// <summary>
        /// Fires whenever the system needs to write System configuration.
        /// </summary>
        event EventHandler SaveSystemConfigRequired;

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
