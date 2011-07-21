#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\KeyInteractionEventArgs.cs) is part of CiviKey. 
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
using System.Linq;
using CK.Core;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines the posible types of <see cref="KeyInteractionEventArgs"/>.
    /// </summary>
    public enum KeyInteractionEventType
    {
        /// <summary>
        /// The key has been pushed down.
        /// </summary>
        Down,

        /// <summary>
        /// The key is pressed: it can be because of the user released it or because 
        /// a repeat occured.
        /// </summary>
        Pressed,

        /// <summary>
        /// The key has been released.
        /// </summary>
        Up
    }

    /// <summary>
    /// Defines a key event argument related to user interaction: it can be one of the <see cref="KeyInteractionEventType"/>.
    /// </summary>
    public class KeyInteractionEventArgs : KeyEventArgs
    {
        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public KeyInteractionEventType EventType { get; private set; }
        
        /// <summary>
        /// Gets the commands asociated to the event.
        /// These commands are a copy of the key commands at the time of the event: any change can 
        /// be made to the <see cref="IKeyProgram"/> associated to the key without interfering
        /// with these commands.
        /// </summary>
        public IReadOnlyList<string> Commands { get; private set; }

        public KeyInteractionEventArgs( IKey k, IKeyProgram p, KeyInteractionEventType eventType )
            : base( k )
        {
            EventType = eventType;
            // Clone the commands: the emitted commands is a snapshot of the commands
            // at the time of the event.
            string[] copy = p.Commands.ToArray();
            Commands = new ReadOnlyListOnIList<string>( copy );
        }
    }

}
