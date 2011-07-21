#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\LayoutKeyModeModeChangedEventArgs.cs) is part of CiviKey. 
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
    /// Defines the argument of event that fires when the mode of a key layout mode
    /// is modified. This happens only during the edition of a keyboard: the event is raised 
    /// only by <see cref="ILayoutKeyMode.ChangeMode"/> and <see cref="ILayoutKeyMode.SwapModes"/>
    /// (note that when swapping the more specialized <see cref="LayoutKeyModeModeSwappedEventArgs"/> is used).
    /// </summary>
    public class LayoutKeyModeModeChangedEventArgs : LayoutKeyModeEventArgs
    {
        /// <summary>
        /// Gets the previous <see cref="IKeyboardMode">mode</see> of the key layout.
        /// </summary>
        public IKeyboardMode PreviousMode { get; private set; }

        public LayoutKeyModeModeChangedEventArgs( ILayoutKeyMode key, IKeyboardMode previousMode )
            : base( key )
        {
            PreviousMode = previousMode;
        }
    }
}
