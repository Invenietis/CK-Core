#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IServiceInfo.cs) is part of CiviKey. 
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
    /// <summary>
    /// Describes a service interface.
    /// </summary>
    public interface IServiceInfo : IDiscoveredInfo, IComparable<IServiceInfo>
    {
        /// <summary>
        /// Gets the full name of the service.
        /// </summary>
        string AssemblyQualifiedName { get; }

        /// <summary>
        /// Gets the full name of the service (namespace and interface name).
        /// </summary>
        string ServiceFullName { get; }

        /// <summary>
        /// Gets whether the service is a <see cref="IDynamicService"/>.
        /// </summary>
        bool IsDynamicService { get; }

        /// <summary>
		/// Gets the assembly info that contains (defines) this interface.
        /// If the service interface itself has not been found, this is null.
		/// </summary>
		IAssemblyInfo AssemblyInfo { get; }

        /// <summary>
        /// Gets the different <see cref="IPluginInfo"/> that implement this service.
        /// </summary>
        IReadOnlyList<IPluginInfo> Implementations { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimpleMethodInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimpleMethodInfo> MethodsInfoCollection { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimpleEventInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimpleEventInfo> EventsInfoCollection { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimplePropertyInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimplePropertyInfo> PropertiesInfoCollection { get; }
    }
}
