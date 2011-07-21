#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayout.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// A keyboard layout holds a collection of <see cref="ILayoutZone"/> that is automatically 
    /// synchronized with the keyboard to which it belongs.
    /// It is identified by a <see cref="Name"/> that is unique in the scope of a keyboard.
    /// </summary>
    public interface ILayout : IKeyboardElement
    {
        /// <summary>
        /// Gets the <see cref="IContext"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IKeyboard"/> that holds this <see cref="ILayoutCollection"/>. 
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Destroys this layout. If the layout is the current layout in use, the default layout is set as the current layout (the default layout 
        /// can not be destroyed).
        /// On success, the <see cref="ILayoutCollection.LayoutDestroyed"/> event is fired.
        /// </summary>
        /// <remarks>
        /// Once destroyed, a layout is no more functionnal and no method nor properties should be called.
        /// </remarks>
        void Destroy();
        
        /// <summary>
        /// Gets the layout name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Renames the keyboard layout. The <see cref="Name"/> is automatically numbered to avoid name clashes.
		/// It also throws an exception if the user tries to rename the default layout (its name
        /// must always be the empty string).
        /// </summary>
        /// <param name="name">New name for the layout. Must not be null nor empty.</param>
        /// <returns>The new name.</returns>
        string Rename( string name );

        /// <summary>
        /// Gets whether this <see cref="ILayout"/> is the default one.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// Gets whether this <see cref="ILayout"/> is the current one.
        /// </summary>
        bool IsCurrent { get; }

        /// <summary>
        /// Gets or sets the width of this layout. Key layouts are not concerned by this change.
        /// </summary>
        int W { get; set; }

        /// <summary>
        /// Gets or sets the height of this layout. Key layouts are not concerned by this change.
        /// </summary>
        int H { get; set; }

        /// <summary>
        /// Gets the collection of layouts: there is one <see cref="ILayoutZone"/> per <see cref="IZone"/>
        /// in the <see cref="Keyboard"/>.
        /// </summary>
        ILayoutZoneCollection LayoutZones { get; }

        /// <summary>
        /// Gets the <see cref="RequirementLayer"/> of this ILayout.
        /// </summary>
        RequirementLayer RequirementLayer { get; }
    }
}
