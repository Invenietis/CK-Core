#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyboard.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines virtual keyboard.
    /// </summary>
    public interface IKeyboard : IKeyboardElement
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/> that hold this <see cref="IKeyboard"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Destroys this keyboard.
        /// Once destroyed, a keyboard is no more functionnal and no method nor properties should be called.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets this <see cref="IKeyboard"/> name. This name is unique in the <see cref="Context"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Renames this <see cref="IKeyboard"/>: this may fail or an automatic renaming may occur 
        /// in order to maintain the unicity of keyboard's name in a <see cref="IContext"/>.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <returns>The final name of this keyboard.</returns>
        string Rename( string name );

        /// <summary> 
        /// Gets or sets the available modes for this keyboard: the <see cref="CurrentMode"/> is necessarily a subset 
        /// of this modes.
        /// </summary>
        IKeyboardMode AvailableMode { get; set; }

        /// <summary>
        /// Gets or sets the current mode. Any atomic mode that do not exist in <see cref="AvailableMode"/> are automatically removed
        /// from this property.
        /// </summary>
        IKeyboardMode CurrentMode { get; set; }

        /// <summary>
        /// Fires while trying to set <see cref="AvailableMode"/> to a new value. The event argument can be used to reject the change.
        /// </summary>
        /// <remarks>
        /// Even if the <see cref="KeyboardModeChangingEventArgs.Cancel"/> is let to false (ie. no listeners rejects the change), 
        /// the change can be cancelled by <see cref="CurrentModeChanging"/> if the new available modes has an impact on the <see cref="CurrentMode"/>
        /// (by removing some of its atomic modes).
        /// </remarks>
        event EventHandler<KeyboardModeChangingEventArgs> AvailableModeChanging;
        
        /// <summary>
        /// Fires when <see cref="AvailableMode"/> changed.
        /// </summary>
        event EventHandler<KeyboardModeChangedEventArgs> AvailableModeChanged;

        /// <summary>
        /// Fires while trying to set <see cref="CurrentMode"/>.
        /// </summary>
        event EventHandler<KeyboardModeChangingEventArgs> CurrentModeChanging;
        
        /// <summary>
        /// Fires when <see cref="CurrentMode"/> changed.
        /// </summary>
        event EventHandler<KeyboardModeChangedEventArgs> CurrentModeChanged;

        /// <summary>
        /// Collection of the <see cref="ILayout"/> hold by this keyboard.
        /// This collection maintains the current layout and exposes events that can be used 
        /// to track layout related changes.
        /// </summary>
        ILayoutCollection Layouts { get; }

        /// <summary>
        /// Gets or sets the current <see cref="ILayout"/> of this <see cref="IKeyboard"/>. 
        /// Simply relays to <see cref="Layouts">Layouts</see>.<see cref="ILayoutCollection.Current">Current</see>.
        /// </summary>
        ILayout CurrentLayout { get; set; }

        /// <summary>
        /// Gets the <see cref="RequirementLayer"/> of this IKeyboard.
        /// </summary>
        RequirementLayer RequirementLayer { get; }

        /// <summary>
        /// Fires whenever the <see cref="CurrentLayout"/> changed. 
        /// It is the same event as <see cref="Layouts">Layouts</see>.<see cref="ILayoutCollection.CurrentChanged">CurrentChanged</see>.
        /// </summary>
        event EventHandler<KeyboardCurrentLayoutChangedEventArgs> CurrentLayoutChanged;

        /// <summary>
        /// Collection of the <see cref="IZone"/> hold by this keyboard.
        /// </summary>
        IZoneCollection Zones { get; }

        /// <summary>
        /// Gets all the <see cref="IKey"/> that this keyboard contains.
        /// </summary>
        IReadOnlyList<IKey> Keys { get; }

        /// <summary>
        /// Fires when a new <see cref="IKey">key</see> has been created in a one 
        /// of this keyboard's <see cref="Zones">zone</see>.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyCreated;

        /// <summary>
        /// Fires when a <see cref="IKey">key</see> in a one of this keyboard's <see cref="Zones">zone</see>
        /// has been <see cref="IKey.Destroy">destroyed</see>.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyDestroyed;
        
        /// <summary>
        /// Fires when a <see cref="IKey">key</see>.<see cref="IKey.Index">Index</see> has changed in a one 
        /// of this keyboard's <see cref="Zones">zone</see>.
        /// </summary>
        event EventHandler<KeyMovedEventArgs> KeyMoved;


        /// <summary>
        /// Fires when a new <see cref="IKeyMode">actual key</see> has been created for one 
        /// of this keyboard's keys.
        /// </summary>
        event EventHandler<KeyModeEventArgs> KeyModeCreated;
        
        
        /// <summary>
        /// Fires when a <see cref="IKeyMode">actual key</see> in a one of this keyboard's keys 
        /// has been <see cref="IKey.Destroy">destroyed</see>.
        /// </summary>
        event EventHandler<KeyModeEventArgs> KeyModeDestroyed;

        /// <summary>
        /// Fires whenever a property of this <see cref="IKey"/> or its <see cref="Current"/> key mode
        /// or its <see cref="CurrentLayout"/> or its <see cref="CurrentActualLayout"/> changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event handles the change of real properties (such as the <see cref="ILayoutKeyMode.X">X</see> coordinate
        /// of the current key mode layout) but also the change of the <see cref="Current"/> actual key itself (when setting a 
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
        /// Fires whenever the mode of one of our actual keys changed.
        /// The event argument may be an instance of the <see cref="KeyModeModeSwappedEventArgs"/> class if
        /// the change is the result of a call to <see cref="IKeyMode.SwapModes"/> instead of <see cref="IKeyMode.ChangeMode"/>.
        /// </summary>
        event EventHandler<KeyModeModeChangedEventArgs> KeyModeModeChanged;

        /// <summary>
        /// Fires whenever a <see cref="ILayoutKeyMode"/> has been created.
        /// </summary>
        event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeCreated;

        /// <summary>
        /// Fires whenever a <see cref="IKeyModelayout"/> has been destroyed.
        /// </summary>
        event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeDestroyed;

        /// <summary>
        /// Fires whenever a <see cref="Key"/> is down.
        /// </summary>
        event EventHandler<KeyInteractionEventArgs> KeyDown;

        /// <summary>
        /// Fires whenever a <see cref="Key"/> is up.
        /// </summary>
        event EventHandler<KeyInteractionEventArgs> KeyUp;

        /// <summary>
        /// Fires whenever a <see cref="Key"/> is pressed.
        /// </summary>
        event EventHandler<KeyPressedEventArgs> KeyPressed;
    }
}
