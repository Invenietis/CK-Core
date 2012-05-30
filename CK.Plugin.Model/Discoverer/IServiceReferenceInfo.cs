#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IServiceReferenceInfo.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Defines the relation that a plugin declares regarding services.
    /// </summary>
	public interface IServiceReferenceInfo : IDiscoveredInfo
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo"/> that defines this reference.
        /// </summary>
        IPluginInfo Owner { get; }

		/// <summary>
		/// Gets the name of the property that references the service.
		/// </summary>
		string PropertyName { get; }

        /// <summary>
        /// Gets a reference to the actual service.
        /// </summary>
        IServiceInfo Reference { get; }

		/// <summary>
		/// Gets the requirements for the referenced service.
		/// </summary>
		RunningRequirement Requirements { get; }
	}
}
