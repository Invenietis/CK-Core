#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyboardCollection.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    public interface IKeyboardCollection : IReadOnlyCollection<IKeyboard>
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/> to which these keyboards belong.
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets one of the <see cref="IKeyboard"/> by its name.
        /// </summary>
        /// <param name="name">Name of the keyboard to find.</param>
        /// <returns>The <see cref="IKeyboard"/> object or null if not found.</returns>
        IKeyboard this[string name] { get; }

        /// <summary>
        /// This method creates and adds a <see cref="IKeyboard"/> into this context.
        /// The <see cref="KeyboardCreated"/> event is raised.
        /// </summary>
        /// <param name="name">The proposed keyboard name.</param>
        /// <returns>The new keyboard.</returns>
        /// <remarks>
        /// If the <see cref="IContext.CurrentKeyboard"/> is null, the newly created <see cref="IKeyboard"/> becomes the current one.
        /// Note that its <see cref="IKeyboard.Name"/> may be different than <paramref name="name"/> if a keyboard already exists
        /// with the proposed name.
        /// </remarks>
        IKeyboard Create( string name );

        /// <summary>
        /// Destroys all the keyboards from this collection. <see cref="IContext.CurrentKeyboard"/> becomes null.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets or sets the current <see cref="IKeyboard"/> for this context.
        /// The new keyboard must be null or belong to this collection otherwise an <see cref="ApplicationException"/>
        /// is thrown.
        /// </summary>
        /// <seealso cref="CurrentChanged"/>
        IKeyboard Current { get; set; }
        
        /// <summary>
        /// Fires whenever the <see cref="Current"/> changed.
        /// </summary>
        event EventHandler<CurrentKeyboardChangedEventArgs> CurrentChanged;

        /// <summary>
        /// Fires whenever a <see cref="IKeyboard"/> has been created.
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyboardCreated;

        /// <summary>
        /// Fires whenever a <see cref="IKeyboard"/> has been destroyed.
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyboardDestroyed;

        /// <summary>
        /// Fires whenever one of the keyboard contained in this collection has been renamed.
        /// </summary>
        event EventHandler<KeyboardRenamedEventArgs> KeyboardRenamed;

    }

}
