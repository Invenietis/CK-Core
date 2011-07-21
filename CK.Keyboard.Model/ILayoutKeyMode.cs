#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutKeyMode.cs) is part of CiviKey. 
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
    /// Defines the layout associated to a <see cref="IKeyMode"/>.
    /// </summary>
    public interface ILayoutKeyMode : IZoneElement, IKeyPropertyHolder
    {
        /// <summary>
        /// Gets the <see cref="ILayout"/> for which this actual key layout is defined. 
        /// </summary>
        ILayout Layout { get; }

        /// <summary>
        /// Gets the <see cref="ILayoutKey"/> for which this actual key layout is defined. 
        /// </summary>
        ILayoutKey LayoutKey { get; }

        /// <summary>
        /// Gets the mode that defines this actual key for the key.
        /// </summary>
        IKeyboardMode Mode { get; }

        /// <summary>
        /// Destroys this actual key layout.
        /// If the <see cref="Mode"/> is <see cref="IKeyboardContextMode.EmptyMode">empty</see>, an 
        /// exception is thrown (default layout can not be destroyed).
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets or sets whether the actual key is visible in this <see cref="Layout"/>.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate of this key.
        /// </summary>
        int X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of this key.
        /// </summary>
        int Y { get; set; }

        /// <summary>
        /// Gets or sets the width of this key.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of this key.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Swaps this <see cref="Mode"/> with the one of <paramref name="other" />.
        /// Raises the <see cref="IKey.KeyModeModeChanged"/> and <see cref="IKeyboard.KeyModeModeChanged"/> events first on 
        /// the other one and then for this actual key, both with an <see cref="KeyModeModeSwappedEventArgs"/> argument.
        /// </summary>
        /// <param name="other">The actual key which mode will be exchanged with this actual key.
        /// It must be one of the <see cref="IKeyMode"/> of this key otherwise an exception is thrown.
        /// </param>
        void SwapModes( ILayoutKeyMode other );
    }
}
