#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyPropertyHolder.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// This abstraction is common to all objects attached to a key that can hold properties.
    /// </summary>
    public interface IKeyPropertyHolder
    {
        /// <summary>
        /// Gets the <see cref="IKey"/> associated to this object.
        /// </summary>
        IKey Key { get; }

        /// <summary>
        /// Gets whether this object belongs to the "Current" scope: <see cref="IKey"/> and <see cref="ILayoutKey"/>
        /// objects are always current whereas for <see cref="IKeyMode"/>, it depends on current keyboard mode
        /// and for <see cref="ILayoutKeyMode"/>, it depends on both current keyboard mode and current keyboard layout.
        /// </summary>
        bool IsCurrent { get; }
    }

}
