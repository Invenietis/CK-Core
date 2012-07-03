#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\UniqueId\SimpleUniqueId.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Minimal implementation of the minimal <see cref="IUniqueId"/> interface.
    /// </summary>
    public class SimpleUniqueId : IUniqueId
    {
        /// <summary>
        /// Empty <see cref="IUniqueId"/> bount to the <see cref="Guid.Empty"/>.
        /// </summary>
        public static readonly IUniqueId Empty = SimpleNamedVersionedUniqueId.Empty;

        /// <summary>
        /// Gets a <see cref="IUniqueId"/> that must be used to denote an invalid key.
        /// This value MUST NOT be used for anything else than a marker.
        /// </summary>
        public static readonly IUniqueId InvalidId = SimpleNamedVersionedUniqueId.InvalidId;

        /// <summary>
        /// Empty array of <see cref="IUniqueId"/>.
        /// </summary>
        public static readonly IUniqueId[] EmptyArray = SimpleNamedVersionedUniqueId.EmptyArray;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        public SimpleUniqueId( string p ) 
        { 
            UniqueId = new Guid( p ); 
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleUniqueId"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> for the <see cref="UniqueId"/>.</param>
        public SimpleUniqueId( Guid p ) 
        { 
            UniqueId = p; 
        }

        /// <summary>
        /// Gets the unique identifier that this object represents.
        /// </summary>
        public Guid UniqueId { get; private set; }
    }

    
}
