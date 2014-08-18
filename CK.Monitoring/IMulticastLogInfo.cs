#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\IMulticastLogInfo.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Information required by a <see cref="IMulticastLogEntry"/>.
    /// </summary>
    public interface IMulticastLogInfo
    {
        /// <summary>
        /// Gets the monitor identifier.
        /// </summary>
        Guid MonitorId { get; }

        /// <summary>
        /// Gets the depth of the entry in the source <see cref="MonitorId"/>.
        /// </summary>
        int GroupDepth { get; }

        /// <summary>
        /// Gets the previous entry type. <see cref="LogEntryType.None"/> when unknown.
        /// </summary>
        LogEntryType PreviousEntryType { get; }

        /// <summary>
        /// Gets the previous log time. <see cref="DateTimeStamp.Unknown"/> when unknown.
        /// </summary>
        DateTimeStamp PreviousLogTime { get; }
    }
}
