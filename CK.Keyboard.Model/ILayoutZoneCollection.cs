#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutZoneCollection.cs) is part of CiviKey. 
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
    /// Defines the collection of zone layout. This collection is fully under the control of the keyboard zones since a 
    /// layout object (for zone and key) is automatically available for a any existing <see cref="ILayout"/> (their life cycle 
    /// is automatically handled by the kernel).
    /// </summary>
    public interface ILayoutZoneCollection : IReadOnlyCollection<ILayoutZone>
    {
        /// <summary>
        /// Gets the <see cref="ILayout"/> to which this collection belongs. 
        /// </summary>
        ILayout Layout { get; }

        /// <summary>
        /// Gets the zone layout given the name of the zone.
        /// </summary>
        /// <param name="zoneName">Name of the zone in the keyboard.</param>
        /// <returns>The zone layout or null if no such zone exist.</returns>
        ILayoutZone this[ string zoneName ] { get; }

        /// <summary>
        /// Gets the layout of the default zone (the default zone has an empty name).
        /// </summary>
        ILayoutZone Default { get; }

    }
}
