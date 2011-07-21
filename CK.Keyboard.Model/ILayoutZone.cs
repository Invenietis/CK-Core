#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutZone.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Text;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// </summary>
    public interface ILayoutZone : IKeyboardElement
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IKeyboard"/> of the <see cref="Zone"/>. 
        /// This avoids the choice between <see cref="IKeyboardElement.Keyboard">Zone.Keyboard</see>
        /// and <see cref="ILayout.Keyboard">Layout.Keyboard</see>.
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Gets the <see cref="IZone"/> to which this layout applies. 
        /// </summary>
        IZone Zone { get; }

        /// <summary>
        /// Gets the <see cref="ILayout"/> to which this <see cref="ILayoutZone"/> belongs. 
        /// </summary>
        ILayout Layout { get; }

        /// <summary>
        /// Gets the collection of key layouts: there is one <see cref="ILayoutKey"/> per <see cref="IKey"/>
        /// in a keyboard.
        /// </summary>
        ILayoutKeyCollection LayoutKeys { get; }


    }
}
