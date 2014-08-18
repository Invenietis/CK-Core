#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\IMulticastLogEntry.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CK.Monitoring
{

    /// <summary>
    /// Unified interface for multi-cast log entries whatever their <see cref="ILogEntry.LogType"/> or their source <see cref="IMulticastLogInfo.MonitorId"/> is.
    /// All log entries can be exposed through this "rich" interface.
    /// </summary>
    public interface IMulticastLogEntry : ILogEntry, IMulticastLogInfo
    {
        /// <summary>
        /// Gets the depth of the entry in the source <see cref="IMulticastLogInfo.MonitorId"/>.
        /// This is always available (whatever the <see cref="ILogEntry.LogType">LogType</see> is <see cref="LogEntryType.OpenGroup"/>, <see cref="LogEntryType.CloseGroup"/>,
        /// or <see cref="LogEntryType.Line"/>).
        /// </summary>
        new int GroupDepth { get; }

        /// <summary>
        /// Creates a unicast entry from this multi-cast one.
        /// The <see cref="IMulticastLogInfo.MonitorId"/> and <see cref="GroupDepth"/> are lost (but less memory is used).
        /// </summary>
        ILogEntry CreateUnicastLogEntry();
    }
}
