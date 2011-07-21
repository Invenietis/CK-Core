#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\KeyEventArgs.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines a key event argument: gives access to the key that is the 
    /// subject of the event.
    /// </summary>
    public class KeyEventArgs : KeyboardEventArgs
    {
        /// <summary>
        /// Gets the key concerned by the event.
        /// </summary>
        public IKey Key { get; private set; }

        public KeyEventArgs( IKey k )
            : base( k.Keyboard )
        {
            Key = k;
        }
    }
}
