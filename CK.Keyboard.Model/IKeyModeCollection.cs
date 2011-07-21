#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyModeCollection.cs) is part of CiviKey. 
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
    public interface IKeyModeCollection : IReadOnlyCollection<IKeyMode>
    {
        /// <summary>
        /// Gets the <see cref="IKey"/> that holds this collection.
        /// </summary>
        IKey Key { get; }

        /// <summary>
        /// Gets the <see cref="IKeyMode"/> for the exact specified mode.
        /// </summary>
        /// <param name="mode"><see cref="IKeyboardMode">Mode</see> to find.</param>
        /// <returns>Null if no actual key exists for this exact mode.</returns>
        IKeyMode this[IKeyboardMode mode] { get; }

        /// <summary>
        /// Finds the best <see cref="IKeyMode"/> given the specified mode.
        /// </summary>
        /// <param name="mode"><see cref="IKeyboardMode">Mode</see> for which an actual key must be found.</param>
        /// <returns>Never null since in the worst case the default actual key (the one with the <see cref="IContext.EmptyMode"/>) will be returned.</returns>
        IKeyMode FindBest( IKeyboardMode mode );

        /// <summary>
        /// Finds or creates a <see cref="IKeyMode"/> into this collection.
        /// </summary>
        /// <param name="mode">The mode for which an <see cref="IKeyMode"/> must exist.</param>
        /// <returns>The <see cref="IKeyMode"/> either created or found.</returns>
        IKeyMode Create( IKeyboardMode mode );

        /// <summary>
        /// Fires whenever a <see cref="IKeyMode"/> has been created.
        /// </summary>
        event EventHandler<KeyModeEventArgs> KeyModeCreated;

        /// <summary>
        /// Fires whenever a <see cref="IKeyMode"/> has been destroyed.
        /// </summary>
        event EventHandler<KeyModeEventArgs> KeyModeDestroyed;

    }

}
