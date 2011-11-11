#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ComponentModel\ComponentModel\ISimpleTypeFinder.cs) is part of CiviKey. 
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

using System;

namespace CK.Core
{
    /// <summary>
    /// This simple interface allows to resolve types from names.
    /// </summary>
    /// <remarks>
    /// Types mapping is an option that should be used with care.    
    /// </remarks>
	public interface ISimpleTypeFinder
	{
        /// <summary>
        /// Gets an (optionnal) mapping from any name of a type to the actual
        /// assembly qualified name that must be used for it.
        /// This function MUST be idempotent (ie. MapType( MapType(x) ) == MapType(x) whatever x is).
        /// Its default implementation is simply to return its parameter unchanged (no mapping).
        /// </summary>
        /// <param name="externalName">The assembly qualified name of a type.</param>
        /// <returns>The assembly qualified name (that <see cref="ResolveType"/> can use) of the type to use.</returns>
        string MapType( string externalName );

        /// <summary>
        /// First calls <see cref="MapType"/> and then resolves the <see cref="Type"/> from the mapped string.
        /// If <paramref name="throwOnError"/> is true, a <see cref="TypeLoadException"/> will be fired if the resolution fails.
        /// </summary>
        /// <param name="externalName">Assembly qualified name of the type</param>
        /// <param name="throwOnError">
        /// True to ALWAYS throw a <see cref="TypeLoadException"/> if the type is not found.
        /// False prevents any exception to be thrown and simply returns null.
        /// </param>
        /// <returns>The type or null if not found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception cref="TypeLoadException">
        /// When <paramref name="throwOnError"/> is true, always throws this kind of exception.
        /// The original error (not a <see cref="TypeLoadException"/>) is available in the <see cref="Exception.InnerException"/>.
        /// </exception>
        Type ResolveType( string externalName, bool throwOnError );
	}

}
