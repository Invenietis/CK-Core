#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IZoneCollection.cs) is part of CiviKey. 
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
    /// Collection containing all the zones corresponding to a keyboard.
    /// </summary>
    public interface IZoneCollection : IReadOnlyCollection<IZone>
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/> that hold the <see cref="Keyboard"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IKeyboard"/> that hold this <see cref="IZoneCollection"/>. 
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Gets one of the <see cref="IZone"/> by its name.
        /// </summary>
        /// <param name="name">Name of the zone to find.</param>
        /// <returns>The <see cref="IZone"/> object or null if not found.</returns>
        IZone this[string name] { get; }
        
        /// <summary>
        /// This method creates and adds a <see cref="IZone"/> in this collection.
        /// The <see cref="ZoneCreated"/> event is raised.
        /// </summary>
        /// <param name="name">The proposed zone name.</param>
        /// <returns>The new zone.</returns>
        /// <remarks>
        /// Note that its <see cref="IZone.Name"/> may be different than <paramref name="name"/> if a zone already exists
        /// with the proposed name.
        /// </remarks>
        IZone Create( string name );

        /// <summary>
        /// Gets the default zone: its <see cref="IZone.Name"/> is an empty string and it 
        /// can not be <see cref="IZone.Destroy">destroyed</see> nor <see cref="IZone.Rename">renamed</see>.
        /// </summary>
        IZone Default { get; }

        /// <summary>
        /// Fires whenever a <see cref="IZone"/> has been created.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneCreated;

        /// <summary>
        /// Fires whenever a <see cref="IZone"/> has been destroyed.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneDestroyed;


        /// <summary>
        /// Fires whenever one of the zone contained in this collection has been renamed.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneRenamed;



    }
}
