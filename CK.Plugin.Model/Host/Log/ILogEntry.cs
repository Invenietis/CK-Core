#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\ILogEntry.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;

namespace CK.Plugin
{
    /// <summary>
    /// Common interface for log entries.
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// Gets the actual type of this entry.
        /// </summary>
        LogEntryType EntryType { get; }
        
        /// <summary>
        /// Gets the creation time.
        /// </summary>
        DateTime CreationTimeUtc { get; }

        /// <summary>
        /// Log Serial Number.
        /// </summary>
        int LSN { get; }

        /// <summary>
        /// Depth on the call stack.
        /// </summary>
        int Depth { get; }

    }
}
