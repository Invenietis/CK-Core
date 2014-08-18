#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\LogEntryType.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{

    /// <summary>
    /// Type of a <see cref="ILogEntry"/>.
    /// </summary>
    public enum LogEntryType
    {
        /// <summary>
        /// Non applicable.
        /// </summary>
        None = 0,

        /// <summary>
        /// A standard log entry.
        /// Except <see cref="ILogEntry.Conclusions"/> (reserved to <see cref="CloseGroup"/>), all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        Line = 1,

        /// <summary>
        /// Group is opened.
        /// Except <see cref="ILogEntry.Conclusions"/> (reserved to <see cref="CloseGroup"/>), all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        OpenGroup = 2,

        /// <summary>
        /// Group is closed. 
        /// Note that the only available information are <see cref="ILogEntry.Conclusions"/>, <see cref="ILogEntry.LogLevel"/> and <see cref="ILogEntry.LogTime"/>.
        /// All other properties are set to their default: <see cref="ILogEntry.Text"/> for instance is null.
        /// </summary>
        CloseGroup = 3
    }
}
