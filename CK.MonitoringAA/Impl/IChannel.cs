#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Impl\IChannel.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    /// <summary>
    /// Abstraction of a Channel: it knows how to <see cref="Handle"/> log events.
    /// </summary>
    internal interface IChannel
    {
        /// <summary>
        /// Called once the channel is ready to <see cref="Handle"/> events (but before the new configuration is actually applied).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets the minimal log level that this channel expects. 
        /// Should default to <see cref="LogLevelFilter.None"/>.
        /// </summary>
        LogFilter MinimalFilter { get; }

        /// <summary>
        /// Locks the channel: a call to <see cref="Handle"/> is pending.
        /// This is required to avoid a race condition between Channel is obtained by a GrandOutputClient 
        /// and the call to Handle.
        /// </summary>
        void PreHandleLock();

        /// <summary>
        /// Cancels a previous call to <see cref="PreHandleLock"/>.
        /// This is used when the Channel to use must be changed while being obtained by a GrandOutputClient. 
        /// </summary>
        void CancelPreHandleLock();

        /// <summary>
        /// Handles one event.
        /// This is called by GrandOutputClients that are bound to this channel.
        /// The lock previously obtained by a call to PreHandleLock is released.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="sendToCommonSink">
        /// True when the event must be sent to the common sink. 
        /// False when the event has been buffered: it has already been sent to the common sink.
        /// </param>
        void Handle( GrandOutputEventInfo e, bool sendToCommonSink = true );

    }
}
