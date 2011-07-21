#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyCollection.cs) is part of CiviKey. 
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
    /// The collection of <see cref="IKey"/> accessible from <see cref="IZone.Keys"/>.
    /// </summary>
    public interface IKeyCollection : IReadOnlyList<IKey>
    {
        /// <summary>
        /// Gets the <see cref="IZone"/> to which these keys belong.
        /// </summary>
        IZone Zone { get; }

        /// <summary>
        /// Creates a new <see cref="IKey"/> at the end of this collection.
        /// </summary>
        /// <returns>The newly created key.</returns>
        IKey Create();
        
        /// <summary>
        /// Creates a new <see cref="IKey"/> at a specified position.
        /// </summary>
        /// <param name="index">Index of the new key.</param>
        /// <returns>The newly created key.</returns>
        IKey Create( int index );

        /// <summary>
        /// Fires whenever a new <see cref="IKey"/> has been created.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyCreated;

        /// <summary>
        /// Fires whenever a <see cref="IKey"/> has been destroyed.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyDestroyed;

        /// <summary>
        /// Fires whenever the <see cref="IKey.Index">index</see> of one of the key contained in this collection has changed.
        /// </summary>
        event EventHandler<KeyMovedEventArgs> KeyMoved;

    }

}
