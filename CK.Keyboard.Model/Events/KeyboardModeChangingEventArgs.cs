#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\KeyboardModeChangingEventArgs.cs) is part of CiviKey. 
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
    /// Defines a keyboard mode event argument.
    /// </summary>
    public class KeyboardModeChangingEventArgs : KeyboardEventArgs
    {
        /// <summary>
        /// The mode that will be set.
        /// </summary>
        public IKeyboardMode Mode { get; private set; }

        /// <summary>
        /// Gets or sets whether the <see cref="Mode"/> must be set. Defaults to false.
        /// </summary>
        public bool Cancel { get; set; }

        public KeyboardModeChangingEventArgs( IKeyboard kb, IKeyboardMode next )
            : base( kb )
        {
            Mode = next;
        }
    }
}
