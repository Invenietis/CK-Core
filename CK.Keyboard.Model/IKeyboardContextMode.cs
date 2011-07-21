#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\IContextMode.cs) is part of CiviKey. 
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
using CK.Core;
using System.Collections.Generic;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines the registration root for any <see cref="IKeyboardMode"/> object.
    /// </summary>
    public interface IKeyboardContextMode 
    {
        /// <summary>
        /// Gets the empty mode for this context. It corresponds to the empty string.
        /// </summary>
        IKeyboardMode EmptyMode { get; }

        /// <summary>
        /// Obtains a <see cref="IKeyboardMode"/> (either combined or atomic).
        /// </summary>
        /// <param name="modes">Atomic mode or modes separated by +.</param>
        /// <returns>A keyboard mode.</returns>
        IKeyboardMode ObtainMode( string modes );

        IKeyboardMode ObtainMode( List<IKeyboardMode> atomicModes );

        IKeyboardMode ObtainMode( IKeyboardMode[] atomicModes, int count );
    }
}
