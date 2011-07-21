#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKey.cs) is part of CiviKey. 
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
    /// Implements a key. A key belongs to a <see cref="IZone"/> and holds a collection
    /// of <see cref="IKeyMode"/>.
    /// </summary>
    public interface IKey : IZoneElement
    {
        /// <summary>
        /// Fires whenever a property of this <see cref="IKey"/> or its <see cref="Current"/> actual key 
        /// or its <see cref="CurrentLayout"/> or its <see cref="CurrentActualLayout"/> changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event handles the change of real properties (such as the <see cref="ILayoutKeyMode.X">X</see> coordinate
        /// of the current actual key layout) but also the change of the <see cref="Current"/> actual key itself (when setting a 
        /// new <see cref="IKeyboard.CurrentMode"/>, if and only if the new mode does change the current actual key)
        /// or the change of the <see cref="CurrentActualLayout"/> (when setting a new <see cref="IKeyboard.CurrentMode"/>, if and only if 
        /// the new mode does change the current actual key layout).
        /// </para>
        /// <para>
        /// A contrario, changes of the <see cref="IKeyboard.CurrentLayout">current layout (for the whole keyboard)</see> do 
        /// not trigger this event: layout changes must be tracked at the keyboard level.
        /// </para>
        /// </remarks>
        event EventHandler<KeyPropertyChangedEventArgs> KeyPropertyChanged;

        /// <summary>
        /// Fires whenever a property of one of the non current <see cref="KeyModes"/> or one
        /// of the non current <see cref="ILayoutKeyMode"/> changed.
        /// </summary>
        event EventHandler<KeyPropertyChangedEventArgs> KeyOtherPropertyChanged;

        /// <summary>
        /// Fires whenever the mode of one of our <see cref="KeyModes"/> changed.
        /// The event argument may be an instance of the <see cref="KeyModeModeSwappedEventArgs"/> class if
        /// the change is the result of a call to <see cref="IKeyMode.SwapModes"/> instead of <see cref="IKeyMode.ChangeMode"/>.
        /// </summary>
        event EventHandler<KeyModeModeChangedEventArgs> KeyModeModeChanged;

        /// <summary>
        /// Fires whenever a <see cref="Key"/> is down.
        /// </summary>
        event EventHandler<KeyInteractionEventArgs> KeyDown;

        /// <summary>
        /// Fires whenever a <see cref="Key"/> is pressed.
        /// </summary>
        event EventHandler<KeyPressedEventArgs> KeyPressed;
        
        /// <summary>
        /// Fires whenever a <see cref="Key"/> is up.
        /// </summary>
        event EventHandler<KeyInteractionEventArgs> KeyUp;

        /// <summary>
        /// Destroys this key.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets or sets the index of this <see cref="IKey"/> in its <see cref="Zone"/>.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Pushes this key. Event <see cref="KeyDown"/> is raised.
        /// Must be called only when the key is up (<see cref="IsDown"/> is false) otherwise an exception is thrown.
        /// </summary>
        void Push();

        /// <summary>
        /// True if this key is down.
        /// </summary>
        bool IsDown { get; }

        /// <summary>
        /// Repeat the <see cref="KeyPressed"/> event. The event repeat count is incremented at each call. 
        /// This is typically called by some sort of "repeater" plugin.
        /// Must be called only when the key is down otherwise an exception is thrown.
        /// </summary>
        void RepeatPressed();

        /// <summary>
        /// Releases the key and emits a <see cref="KeyPressed"/>.
        /// Must be called only when the key is up otherwise an exception is thrown.
        /// </summary>
        void Release();

        /// <summary>
        /// Releases the key.
        /// Must be called only when the key is up otherwise an exception is thrown.
        /// </summary>
        /// <param name="doPress">True to trigger the <see cref="KeyPressed"/> event.</param>
        void Release( bool doPress );

        /// <summary>
        /// Gets the collection of <see cref="IKeyMode"/> associated to this <see cref="IKey"/>.
        /// </summary>
        IKeyModeCollection KeyModes { get; }

        /// <summary>
        /// Gets the current key <see cref="ILayoutKey"/> (depends on the current keyboard layout).
        /// </summary>
        ILayoutKey CurrentLayout { get; }

        /// <summary>
        /// Gets the current actual key (depends on the current <see cref="IKeyboard.CurrentMode">mode</see>).
        /// </summary>
        IKeyModeCurrent Current { get; }
    }
}
