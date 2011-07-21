#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutKeyModeCollection.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;

#region CodeAnalysis
[module: SuppressMessage( "Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Scope = "member", Target =  "CK.Keyboard.Model.ILLayoutKeyModeCollection.#Item[CK.Keyboard.Model.IKeyboardMode]" )]
#endregion

namespace CK.Keyboard.Model
{
    /// <summary>
    /// This collection is hold by <see cref="ILayoutKey"/>.
    /// </summary>
    public interface ILayoutKeyModeCollection : IReadOnlyCollection<ILayoutKeyMode>
    {
        /// <summary>
        /// Gets the <see cref="ILayoutKey"/>.
        /// </summary>
        ILayoutKey LayoutKey { get; }

        /// <summary>
        /// Finds or creates a <see cref="ILayoutKeyMode"/> into this collection
        /// </summary>
        /// <param name="mode">The mode for which an <see cref="ILayoutKeyMode"/> must exist.</param>
        /// <returns>The <see cref="ILayoutKeyMode"/> either created or found.</returns>
        ILayoutKeyMode Create( IKeyboardMode mode );

        /// <summary>
        /// Gets the <see cref="ILayoutKeyMode"/> for the given mode.
        /// </summary>
        /// <param name="mode"><see cref="IKeyboardMode">Mode</see> to find.</param>
        /// <returns>Null if not found.</returns>
        ILayoutKeyMode this[ IKeyboardMode mode ] { get; }

        /// <summary>
        /// Returns the best key layout given a mode.
        /// </summary>
        /// <param name="mode"><see cref="IKeyboardMode">Mode</see> to find.</param>
        /// <returns>A non null layout since in the worst case the layout of the default actual key (empty mode) is returned.</returns>
        ILayoutKeyMode FindBest( IKeyboardMode mode );

        /// <summary>
        /// Fires whenever a <see cref="ILayoutKeyMode"/> has been created for this <see cref="LayoutKey"/>, regardless 
        /// of its layout.
        /// </summary>
        event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeCreated;

        /// <summary>
        /// Fires whenever a <see cref="ILayoutKeyMode"/> has been changed for this <see cref="LayoutKey"/>, regardless 
        /// of its layout.
        /// </summary>
        event EventHandler<LayoutKeyModeModeChangedEventArgs>  LayoutKeyModeModeChanged;

        /// <summary>
        /// Fires whenever a <see cref="IKeyModelayout"/> has been destroyed for this <see cref="LayoutKey"/>, regardless 
        /// of its layout.
        /// </summary>
        event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeDestroyed;



    }

}
