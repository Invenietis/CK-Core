#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IConfigEntry.cs) is part of CiviKey. 
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
	/// Simple dictionary entry where the <see cref="Key"/> is a string and 
    /// the <see cref="Value"/> any object (including null).
	/// </summary>
	public interface IConfigEntry
	{
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
