#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyMode.cs) is part of CiviKey. 
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
    /// An actual key is the real <see cref="IKey"/> that is active depending on the <see cref="IKeyboard.Mode"/>. 
    /// </summary>
    public interface IKeyMode : IZoneElement, IKeyPropertyHolder
    {
        /// <summary>
        /// Gets the mode that defines this actual key for the key.
        /// </summary>
        IKeyboardMode Mode { get; }

        /// <summary>
        /// Attempts to change the <see cref="Mode"/> associated to this actual key (the mode must not be 
        /// associated to another actual key).
        /// Raises the <see cref="IKey.KeyModeModeChanged"/> and <see cref="IKeyboard.KeyModeModeChanged"/> events on success.
        /// </summary>
        /// <param name="mode">New mode for this actual key. 
        /// It must be a <see cref="IKeyboardMode"/> from our <see cref="Context"/> otherwise an exception is thrown.</param>
        /// <returns>True if the mode has been successfully set. If the proposed mode is already associated to another
        /// actual key of this key, nothing is done and false is returned.</returns>
        bool ChangeMode( IKeyboardMode mode );

        /// <summary>
        /// Swaps this <see cref="Mode"/> with the one of <paramref name="other" />.
        /// Raises the <see cref="IKey.KeyModeModeChanged"/> and <see cref="IKeyboard.KeyModeModeChanged"/> events first on 
        /// the other one and then for this actual key, both with an <see cref="KeyModeModeSwappedEventArgs"/> argument.
        /// </summary>
        /// <param name="other">The actual key which mode will be exchanged with this actual key.
        /// It must be one of the <see cref="IKeyMode"/> of this key otherwise an exception is thrown.
        /// </param>
        void SwapModes( IKeyMode other );

        /// <summary>
        /// Destroys this <see cref="IKeyMode"/>.
        /// If <see cref="Mode"/> is <see cref="IContext.EmptyMode"/>, an exception is thrown.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is enabled or not.
        /// The fact that the key is visible or not is driven by <see cref="ILayoutKeyMode.Visible"/>.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets an optional description for this key. Can never be null.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the label that must be used when the key is up. 
        /// If set to null or empty, it will automatically default to <see cref="DownLabel"/>.
        /// Can never be null.
        /// </summary>
        string UpLabel { get; set; }

        /// <summary>
        /// Gets or sets the label that must be used when the key is down. 
        /// If set to null or empty, it will automatically default to <see cref="UpLabel"/>.
        /// Can never be null.
        /// </summary>
        string DownLabel { get; set; }

        /// <summary>
        /// Gets the program that this key must raise when the key is <see cref="IKey.Push">pushed</see> down.
        /// May be empty but will never be null.
        /// </summary>
        IKeyProgram OnKeyDownCommands { get; }

        /// <summary>
        /// Gets the program that this key must raise when the key is <see cref="M:IKey.Release">released</see>.
        /// May be empty but will never be null.
        /// </summary>
        IKeyProgram OnKeyUpCommands { get; }

        /// <summary>
        /// Gets the program that this key must raise when the key is pressed.
        /// A key is pressed when it is <see cref="IKey.Push">pushed</see> down and <see cref="IKey.Release()">released</see>.
        /// May be empty but will never be null.
        /// </summary>
        IKeyProgram OnKeyPressedCommands { get; }

    }
}
