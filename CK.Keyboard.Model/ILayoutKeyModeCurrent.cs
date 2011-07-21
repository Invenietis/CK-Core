#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\ILayoutKeyModeCurrent.cs) is part of CiviKey. 
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
    /// The current key layout is obtained by <see cref="ILayoutKey.Current"/>. It is one of the key layouts
    /// associated to the key and its <see cref="ILayoutKeyMode.Mode"/> may not be the same as
    /// the <see cref="IKeyboard.CurrentMode"/>: in such case, <see cref="IsFallBack"/> is true.
    /// </summary>
    public interface ILayoutKeyModeCurrent : ILayoutKeyMode
    {
        /// <summary>
        /// Gets a boolean that states whether this key layout is not the exact one defined 
        /// for the <see cref="IKeyboard.CurrentMode">current keyboard mode</see>.
        /// </summary>
        bool IsFallBack { get; }
    }
}
