#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutKey.cs) is part of CiviKey. 
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
    /// </summary>
    public interface ILayoutKey : IZoneElement, IKeyPropertyHolder
    {
        /// <summary>
        /// Gets the <see cref="ILayout"/> that hold this key layout. 
        /// </summary>
        ILayout Layout { get; }

        /// <summary>
        /// Gets the <see cref="ILayoutZone"/> of the <see cref="Zone"/>'s <see cref="Key"/>. 
        /// </summary>
        ILayoutZone LayoutZone { get; }

        /// <summary>
        /// Gets the collection of <see cref="ILayoutKeyMode"/> for this <see cref="ILayoutKey"/>.
        /// </summary>
        ILayoutKeyModeCollection LayoutKeyModes { get; }

        /// <summary>
        /// Gets the <see cref="ILayoutKeyModeCurrent"/> (depends on the <see cref="IKeyboard.CurrentMode"/>).
        /// </summary>
        ILayoutKeyModeCurrent Current { get;  }

    }
}
