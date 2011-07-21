#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyProgram.cs) is part of CiviKey. 
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
    /// Defines a "program", that is a simple list of commands expressed as strings.
    /// This notion of program is very simple but also very extensible. 
    /// </summary>
    /// <remarks>
    /// Since any "program" has to be expressed as texts (at least for serialization purposes), this
    /// design heaviliy relies on thie "script approach": any language, script, commands can be defined at 
    /// this level withou any constraint.</remarks>
    public interface IKeyProgram
    {

        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the list of the commands.
        /// </summary>
        IList<string> Commands { get; }

        /// <summary>
        /// Fires when a new command is added.
        /// </summary>
        event EventHandler<KeyProgramCommandsEventArgs> CommandInserted;

        /// <summary>
        /// Fires when a command is replaced by an other.
        /// </summary>
        event EventHandler<KeyProgramCommandsEventArgs> CommandUpdated;

        /// <summary>
        /// Fires when a command is deleted.
        /// </summary>
        event EventHandler<KeyProgramCommandsEventArgs> CommandDeleted;

        /// <summary>
        /// Fires when all commands of a KeyProgram are deleted.
        /// </summary>
        event EventHandler<KeyProgramCommandsEventArgs> CommandsCleared;
    }
}
