#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\UniqueId\SimpleNamedVersionedUniqueId.cs) is part of CiviKey. 
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
    /// Minimal implementation of the minimal <see cref="INamedVersionedUniqueId"/> interface.
    /// </summary>
    public class SimpleNamedVersionedUniqueId : INamedVersionedUniqueId
    {
        /// <summary>
        /// Empty <see cref="INamedVersionedUniqueId"/> bount to the <see cref="Guid.Empty"/>, <see cref="Util.EmptyVersion"/> and <see cref="String.Empty"/>.
        /// </summary>
        public static readonly INamedVersionedUniqueId Empty = new SimpleNamedVersionedUniqueId( Guid.Empty, Util.EmptyVersion, String.Empty );

        /// <summary>
        /// Gets a <see cref="INamedVersionedUniqueId"/> that must be used to denote an invalid key.
        /// This value MUST NOT be used for anything else than a temporary marker.
        /// </summary>
        public static readonly INamedVersionedUniqueId InvalidId = new SimpleNamedVersionedUniqueId( "{61A505AD-4D7E-4BD2-9C38-ADDCE7C87A88}", Util.EmptyVersion, "InvalidUniqueId" );

        /// <summary>
        /// Empty array of <see cref="IUniqueId"/>.
        /// </summary>
        public static readonly INamedVersionedUniqueId[] EmptyArray = new INamedVersionedUniqueId[0];

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleNamedVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        /// <param name="version">Version object. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        /// <param name="publicName">Public name for the object. If null, <see cref="String.Empty"/> is used.</param>
        public SimpleNamedVersionedUniqueId( string p, Version version, string publicName )
            : this( new Guid( p ), version, publicName )
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleNamedVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        /// <param name="version">Version as a string. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        /// <param name="publicName">Public name of the object. If null, <see cref="String.Empty"/> is used.</param>
        public SimpleNamedVersionedUniqueId( string p, string version, string publicName )
            : this( new Guid( p ), version != null ? new Version( version ) : Util.EmptyVersion, publicName )
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleNamedVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> for the <see cref="UniqueId"/>.</param>
        /// <param name="version">Version as a string. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        /// <param name="publicName">Public name of the object. If null, <see cref="String.Empty"/> is used.</param>
        public SimpleNamedVersionedUniqueId( Guid p, Version version, string publicName )
        {
            UniqueId = p;
            Version = version ?? Util.EmptyVersion;
            PublicName = publicName ?? String.Empty;
        }

        /// <summary>
        /// Gets the unique identifier that this object represents.
        /// </summary>
        public Guid UniqueId { get; private set; }

        /// <summary>
        /// Gets the version of this object.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Gets the public name of this object.
        /// </summary>
        public string PublicName { get; private set; }
    }

    
}
