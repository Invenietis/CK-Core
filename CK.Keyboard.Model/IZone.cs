#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IZone.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// A zone has a unique <see cref="Name"/> inside a <see cref="IKeyboard"/> and holds a list of <see cref="Keys"/>.
    /// </summary>
    public interface IZone : IKeyboardElement
    {
        /// <summary>
        /// Destroys this zone. The default zone can not be destroyed.
        /// On success, the <see cref="IKeyboard.ZoneDestroyed"/> event is fired.
        /// </summary>
        /// <remarks>
        /// Once destroyed, a zone is no more functionnal and no method nor properties should be called.
        /// </remarks>
        void Destroy();
        
        /// <summary>
        /// Gets the zone name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Renames the keyboard zone. The <see cref="Name"/> is automatically numbered to avoid name clashes.
        /// It also throws an <see cref="ApplicationException"/> if the user tries to rename the default zone (its name
        /// must remain to the empty string).
        /// </summary>
        /// <param name="name">New name for the zone. Must not be null nor empty.</param>
        /// <returns>The new name.</returns>
        string Rename( string name );

        /// <summary>
        /// Gets a value indicating whether this <see cref="IZone"/> is the default one.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// Gets the current layout for this zone.
        /// </summary>
        ILayoutZone CurrentLayout { get; }

        /// <summary>
        /// Gets the keys that this zone contains.
        /// </summary>
        IKeyCollection Keys { get; }

    }
}
