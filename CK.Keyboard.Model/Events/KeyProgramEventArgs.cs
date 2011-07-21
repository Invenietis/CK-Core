#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\KeyProgramEventArgs.cs) is part of CiviKey. 
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
    /// Defines the possible types of <see cref="KeyProgramCommandsEventArgs"/>.
    /// </summary>
    public enum KeyProgramCommandsEventType
    {
        /// <summary>
        /// A new command has been added to the command list.
        /// </summary>
        Inserted,

        /// <summary>
        /// A command has been modified.
        /// </summary>
        Updated,
        
        /// <summary>
        /// A command has been removed.
        /// </summary>
        Deleted,

        /// <summary>
        /// All commands have been removed.
        /// <see cref="KeyProgramCommandsEventArgs.Index"/> is set to -1.
        /// </summary>
        Cleared
    }

    /// <summary>
    /// Defines a KeyProgram event argument : <see cref="Index"/> represents the index 
    /// of the command concerned by the event, in the command list
    /// </summary>
    public class KeyProgramCommandsEventArgs : KeyboardContextEventArgs
    {
        /// <summary>
        /// Gets the <see cref="IKeyProgram"/> that has been modified.
        /// </summary>
        public IKeyProgram KeyProgram { get; private set; }

        /// <summary>
        /// Represents the index in the command list where the command was create, updated, or delete.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the type of event.
        /// </summary>
        public KeyProgramCommandsEventType EventType { get; private set; }

        public KeyProgramCommandsEventArgs( IKeyProgram p, KeyProgramCommandsEventType eventType, int index )
            : base( p.Context )
        {
            KeyProgram = p;
            EventType = eventType;
            Index = index;
        }
    }
}
