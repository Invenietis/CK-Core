#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IAssemblyInfo.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Reflection;

namespace CK.Plugin
{
    public interface IAssemblyInfo : IDiscoveredInfo, IComparable<IAssemblyInfo>
	{
        /// <summary>
        /// Gets the file name of the assembly.
        /// </summary>
        string AssemblyFileName { get; }

        /// <summary>
        /// Gets the size of the assembly file.
        /// </summary>
        int AssemblyFileSize { get; }

        /// <summary>
        /// Gets the <see cref="AssemblyName"/> of the assembly.
        /// </summary>
		AssemblyName AssemblyName { get; }

	    /// <summary>
	    /// Gets that the assembly contains plugins or services.
	    /// </summary>
		bool HasPluginsOrServices { get; }

        /// <summary>
        /// Gets the collections of plugins contained into the assembly.
        /// </summary>
		IReadOnlyList<IPluginInfo> Plugins { get; }

        /// <summary>
        /// Gets the collections of services contained into the assembly.
        /// </summary>
		IReadOnlyList<IServiceInfo> Services { get; }
	}

}
