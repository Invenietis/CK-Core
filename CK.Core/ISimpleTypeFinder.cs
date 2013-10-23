#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\ISimpleTypeFinder.cs) is part of CiviKey. 
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
    /// This simple interface allows to resolve types from names.
    /// </summary>
    /// <remarks>
    /// Types mapping (ie. changing the simple rule that says that the name of a type is simply the type's assembly qualified name) is an 
    /// option that should be used with care.    
    /// </remarks>
	public interface ISimpleTypeFinder
	{
        /// <summary>
        /// Maps and resolves the <see cref="Type"/> from the assembly qualified name set as parameter.
        /// You should use the <see cref="SimpleTypeFinder.Default"/> or <see cref="SimpleTypeFinder.WeakDefault"/> standard
        /// implementations.
        /// If <paramref name="throwOnError"/> is true, a <see cref="TypeLoadException"/> will be fired if the resolution fails.
        /// </summary>
        /// <param name="externalName">Assembly qualified name of the type</param>
        /// <param name="throwOnError">
        /// True to ALWAYS throw a <see cref="TypeLoadException"/> if the type is not found. 
        /// It may also throw <see cref="ArgumentNullException"/> and <see cref="ArgumentException"/> when the assembly qualified name is not valid
        /// False prevents any exception from being thrown and simply returns null.
        /// </param>
        /// <returns>The type or null if not found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception cref="TypeLoadException">
        /// When <paramref name="throwOnError"/> is true, always throws a TypeLoadException whatever the actual exception is.
        /// The original error (not a <see cref="TypeLoadException"/>) is available in the <see cref="Exception.InnerException"/>.
        /// </exception>
        Type ResolveType( string externalName, bool throwOnError );
	}

}
