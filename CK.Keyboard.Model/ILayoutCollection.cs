#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutCollection.cs) is part of CiviKey. 
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
    /// <summary>
    /// Collection containing all the layouts corresponding to a keyboard.
    /// These layouts are automatically synchronized with the keyboard itself.
    /// </summary>
    public interface ILayoutCollection : IReadOnlyCollection<ILayout>
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/> that hold this <see cref="ILayoutCollection"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IKeyboard"/> that hold this <see cref="ILayoutCollection"/>. 
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Gets one of the <see cref="ILayout"/> by its name.
        /// </summary>
        /// <param name="name">Name of the layout to find.</param>
        /// <returns>The <see cref="ILayout"/> object or null if not found.</returns>
        ILayout this[string name] { get; }
        
        /// <summary>
        /// This method creates and adds a <see cref="ILayout"/> in this collection.
        /// The <see cref="LayoutCreated"/> event is raised.
        /// </summary>
        /// <param name="name">The proposed layout name.</param>
        /// <returns>The new layout.</returns>
        /// <remarks>
        /// Note that its <see cref="ILayout.Name"/> may be different than <paramref name="name"/> if a layout already exists
        /// with the proposed name.
        /// </remarks>
        ILayout Create( string name );

        /// <summary>
        /// Gets or sets the current layout. When setting if the value is null it throws an ArgumentNullException.
        /// It also throws ApplicationException if the proposed layout does not belong to this collection.
        /// When the current layout changed, the <see cref="CurrentChanged"/> event fires.
        /// </summary>
        ILayout Current { get; set; }

        /// <summary>
        /// Gets the default layout: its <see cref="ILayout.Name"/> is an empty string and it 
        /// can not be <see cref="ILayout.Destroy">destroyed</see> nor <see cref="ILayout.Rename">renamed</see>.
        /// </summary>
        ILayout Default { get; }

        /// <summary>
        /// Fires whenever the <see cref="Current"/> changed.
        /// </summary>
        event EventHandler<KeyboardCurrentLayoutChangedEventArgs> CurrentChanged;

        /// <summary>
        /// Fires whenever a <see cref="ILayout"/> has been created.
        /// </summary>
        event EventHandler<LayoutEventArgs> LayoutCreated;

        /// <summary>
        /// Fires whenever a <see cref="ILayout"/> has been destroyed.
        /// </summary>
        event EventHandler<LayoutEventArgs> LayoutDestroyed;

        /// <summary>
        /// Fires whenever one of the layout contained in this collection has been renamed.
        /// </summary>
        event EventHandler<LayoutEventArgs> LayoutRenamed;

        /// <summary>
        /// Fires whenever <see cref="ILayout.Width"/> or <see cref="ILayout.Height"/> of one of 
        /// the layout contained in this collection changed.
        /// </summary>
        event EventHandler<LayoutEventArgs> LayoutSizeChanged;

    }
}
