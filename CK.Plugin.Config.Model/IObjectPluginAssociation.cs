#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IObjectPluginAssociation.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{
	/// <summary>
	/// Primary interface that define the association of a plugin and an object.
	/// </summary>
	public interface IObjectPluginAssociation
	{
        /// <summary>
        /// The object that carries the data: it can be of any object type (not a value type).
        /// </summary>
        object Obj { get; }
        
        /// <summary>
        /// The <see cref="INamedVersionedUniqueId"/> that uniquely identifies the plugin.
		/// </summary>
        INamedVersionedUniqueId PluginId { get; }
	}

}
