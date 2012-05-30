#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IPluginInfo.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    public interface IPluginInfo : IDiscoveredInfo, INamedVersionedUniqueId, IComparable<IPluginInfo>
    {
        /// <summary>
        /// Gets the unique identifier of the plugin. This is an alias to <see cref="IUniqueId.UniqueId"/>.
        /// </summary>
        Guid PluginId { get; }

        /// <summary>
        /// Gets the public description of the plugin.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets if a better version of this plugin exists in the system.
        /// </summary>
        bool IsOldVersion { get; }

        /// <summary>
        /// Gets an optional url that describes the plugin.
        /// </summary>
        Uri RefUrl { get; }

        /// <summary>
        /// Gets an optional list of categories used to sort plugins by theme. 
        /// Never null.
        /// </summary>
        IReadOnlyList<string> Categories { get; }

        /// <summary>
        /// Gets an optional icon bounds to this plugin.
        /// </summary>
        Uri IconUri{ get; }
        
        /// <summary>
        /// Gets the full name of the plugin (namespace and class name).
        /// </summary>
        string PluginFullName { get; }

		/// <summary>
        /// Gets a <see cref="IReadOnlyList"/> of <see cref="IPluginEditorInfo"/> that the plugin owns.
		/// </summary>
		IReadOnlyList<IPluginConfigAccessorInfo> EditorsInfo { get; }

        /// <summary>
        /// Gets <see cref="IPluginEditorInfo">editors</see> that can
        /// edit the configuration of this plugin.
        /// </summary>
        IReadOnlyList<IPluginConfigAccessorInfo> EditableBy { get; }

		/// <summary>
		/// Gets the assembly info that contains this plugin.
		/// </summary>
		IAssemblyInfo AssemblyInfo { get; }

		/// <summary>
		/// Gets the services that this plugin references.
		/// </summary>
		IReadOnlyList<IServiceReferenceInfo> ServiceReferences { get; }

		/// <summary>
		/// Gets the service that this plugin implements. Null if the plugin does not implement any service.
		/// </summary>
		IServiceInfo Service { get; }
    }
}
