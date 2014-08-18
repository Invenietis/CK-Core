#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputEventInfo.cs) is part of CiviKey. 
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
using CK.Core;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    /// <summary>
    /// Captures a log data with the current <see cref="IActivityMonitor.Topic"/>.
    /// </summary>
    public struct GrandOutputEventInfo
    {
        /// <summary>
        /// A unified, immutable, log data.
        /// </summary>
        public readonly IMulticastLogEntry Entry;
        
        /// <summary>
        /// The current topic of the monitor when the log occurred. 
        /// </summary>
        public readonly string Topic;

        /// <summary>
        /// Initializes a new <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="e">Log entry.</param>
        /// <param name="topic">Current topic.</param>
        public GrandOutputEventInfo( IMulticastLogEntry e, string topic )
        {
            Entry = e;
            Topic = topic;
        }
    }
}
