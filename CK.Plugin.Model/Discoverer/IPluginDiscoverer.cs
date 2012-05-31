#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IPluginDiscoverer.cs) is part of CiviKey. 
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

using System.IO;
using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.Plugin
{
	public interface IPluginDiscoverer
	{
		/// <summary>
		/// Fires at the beginning of a discovery process.
		/// </summary>
		event EventHandler DiscoverBegin;

		/// <summary>
		/// Fires at the end of the discovery process: all plugins information is available
		/// and up to date.
		/// </summary>
		event EventHandler<DiscoverDoneEventArgs> DiscoverDone;

        /// <summary>
        /// Contains all the <see cref="IAssemblyInfo"/> that have been processed.
        /// They may contain an error or no plugins at all.
        /// </summary>
        IReadOnlyCollection<IAssemblyInfo> AllAssemblies { get; }

        /// <summary>
        /// Contains all the <see cref="IAssemblyInfo"/> that have been succesfully discovered 
        /// and have at least one plugin or one service defined in it.
        /// </summary>
        IReadOnlyCollection<IAssemblyInfo> PluginOrServiceAssemblies { get; }

        /// <summary>
        /// Contains all the <see cref="IPluginInfo"/> that have been succesfully discovered with the best
        /// available version.
        /// </summary>
        IReadOnlyCollection<IPluginInfo> Plugins { get; }

        /// <summary>
        /// Contains all the <see cref="IPluginInfo"/>. This groups <see cref="Plugins"/> and <see cref="OldVersionnedPlugins"/>
        /// and plugins that are on error.
        /// </summary>
        IReadOnlyCollection<IPluginInfo> AllPlugins { get; }

        /// <summary>
        /// Contains all the <see cref="IPluginInfo"/> that have been succesfully discovered.
        /// </summary>
        IReadOnlyCollection<IPluginInfo> OldVersionnedPlugins { get; }

        /// <summary>
        /// Contains all the <see cref="IServiceInfo"/> that have been succesfully discovered
        /// with their implementations.
        /// </summary>
        IReadOnlyCollection<IServiceInfo> Services { get; }

        /// <summary>
        /// Contains all the <see cref="IServiceInfo"/> that have been succesfully discovered.
        /// </summary>
        IReadOnlyCollection<IServiceInfo> AllServices { get; }

        /// <summary>
        /// Contains all the <see cref="IServiceInfo"/> implemented or referenced by plugins and not
        /// founded into assemblies.
        /// </summary>
        IReadOnlyCollection<IServiceInfo> NotFoundServices { get; }

        /// <summary>
        /// Gets <see cref="IPluginInfo"/> best version with the given plugin identifier.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        IPluginInfo FindPlugin( Guid pluginId );

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> associated to the given assembly qualified name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        IServiceInfo FindService( string assemblyQualifiedName );

        /// <summary>
        /// Gets the number of discover previously done.
        /// </summary>
        int CurrentVersion { get; }

        /// <summary>
        /// Start the discover in a given <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="dir">Directory that we have to look into.</param>
        /// <param name="recurse">Sets if the discover is recursive of not.</param>
		void Discover( DirectoryInfo dir, bool recurse );

        /// <summary>
        /// Discover only one file.
        /// </summary>
        /// <param name="file">An exisiting file (a dll).</param>
		void Discover( FileInfo file );
	}
}
