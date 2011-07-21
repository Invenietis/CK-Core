#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKey.cs) is part of CiviKey. 
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
using CK.SharedDic;
using System.Xml.Serialization;
using CK.Storage;
using CK.Plugin;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Represents the execution context of all <see cref="IKeyboardElement"/>.
    /// This context is typically a subset of a more global application context.
    /// </summary>
    public interface IKeyboardContext : IKeyboardContextMode,  IDynamicService
    {
        /// <summary>
        /// Gets all available keyboards in this context.
        /// </summary>
        IKeyboardCollection Keyboards { get; }

        /// <summary>
        /// Gets <see cref="IKeyboard"/> that is currently in use.
        /// </summary>
        IKeyboard CurrentKeyboard { get; set; }

        /// <summary>
        /// Fires whenever the <see cref="CurrentKeyboard"/> has changed.
        /// </summary>
        event EventHandler<CurrentKeyboardChangedEventArgs> CurrentKeyboardChanged;

        /// <summary>
        /// Sets the <see cref="KeyboardContext"/> as dirty.
        /// </summary>
        void SetKeyboardContextDirty();

        /// <summary>
        /// Gets whether the <see cref="IKeyboardContext"/> has been modified since the last time it has been loaded.
        /// </summary>
        bool IsDirty { get; }

    }
}
