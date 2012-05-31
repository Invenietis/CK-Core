#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IConfigPluginEntry.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{

	/// <summary>
    /// Defines one entry seen by a plugin that a in the configuration. 
	/// This is the symmetric of <see cref="IConfigObjectEntry"/> interface.
	/// </summary>
	public interface IConfigPluginEntry
	{
		/// <summary>
		/// The object: it can be of any object type (not a value type).
		/// </summary>
		object Obj { get; }
		
		/// <summary>
		/// Gets the name of this entry.
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Gets the value associated to the <see cref="Key"/>.
		/// </summary>
		object Value { get; }
	}

}
