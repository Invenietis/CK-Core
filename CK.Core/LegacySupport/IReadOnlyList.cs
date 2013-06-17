#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\LegacySupport\IReadOnlyList.cs) is part of CiviKey. 
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

#if net40

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a read only collection of objects that can be individually accessed by index.
    /// This interface is only defined for framework 3.5 or 4.0. The same System.Collections.Generic.IReadOnlyList
    /// has been defined in the .Net framework 4.5.
    /// Previous versions exposed a "contravariant" IndexOf( object ) method now transfered to ICKReadOnlyList.
    /// It has been removed in order to fit the 4.5 definition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not a valid index in the list.</exception>
        T this[ int index ] { get; }

    }
}

#endif
