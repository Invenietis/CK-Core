#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\ISimpleTypeNaming.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// This simple interface allows to obtain names from types.
    /// This can be used in conjuction with <see cref="ISimpleTypeFinder"/>.
    /// </summary>
	public interface ISimpleTypeNaming
	{
        /// <summary>
        /// Gets an external type name.
        /// </summary>
        /// <param name="t">The type for which an external type name must be obtained.</param>
        /// <returns>
        /// A string that represents the given type. 
        /// Can default to <see cref="Type.AssemblyQualifiedName"/>.
        /// </returns>
        string GetTypeName( Type t );
	}

}
