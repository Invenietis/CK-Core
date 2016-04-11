#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\IActivityMonitorOutput.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Offers <see cref="IActivityMonitorClient"/> registration/unregistration and exposes a <see cref="BridgeTarget"/> 
    /// (an <see cref="ActivityMonitorBridgeTarget"/>) that can be used to accept logs from other monitors.
    /// </summary>
    public interface IActivityMonitorOutput
    {
        /// <summary>
        /// Registers an <see cref="IActivityMonitorClient"/> to the <see cref="Clients"/> list.
        /// Duplicate IActivityMonitorClient instances are ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityMonitorClient"/> implementation.</param>
        /// <param name="added">True if the client has been added, false if it was already registered.</param>
        /// <returns>The registered client.</returns>
        IActivityMonitorClient RegisterClient( IActivityMonitorClient client, out bool added );

        /// <summary>
        /// Registers a typed <see cref="IActivityMonitorClient"/>.
        /// Duplicate IActivityMonitorClient instances are ignored.
        /// </summary>
        /// <typeparam name="T">Any type that specializes <see cref="IActivityMonitorClient"/>.</typeparam>
        /// <param name="client">Client to register.</param>
        /// <param name="added">True if the client has been added, false if it was already registered.</param>
        /// <returns>The registered client.</returns>
        T RegisterClient<T>( T client, out bool added ) where T : IActivityMonitorClient;

        /// <summary>
        /// Unregisters the given <see cref="IActivityMonitorClient"/> from the <see cref="Clients"/> list.
        /// Silently ignores an unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityMonitorClient"/> implementation.</param>
        /// <returns>The unregistered client or null if it has not been found.</returns>
        IActivityMonitorClient UnregisterClient( IActivityMonitorClient client );

        /// <summary>
        /// Registers a <see cref="IActivityMonitorClient"/> that must be unique in a sense.
        /// </summary>
        /// <param name="tester">Predicate that must be satisfied for at least one registered client.</param>
        /// <param name="factory">Factory that will be called if no existing client satisfies <paramref name="tester"/>.</param>
        /// <returns>The found or newly created client.</returns>
        /// <remarks>
        /// The factory function MUST return a client that satisfies the tester function otherwise a <see cref="InvalidOperationException"/> is thrown.
        /// The factory is called only when the no client satisfies the tester function: this makes the 'added' out parameter useless.
        /// </remarks>
        T RegisterUniqueClient<T>( Func<T, bool> tester, Func<T> factory ) where T : IActivityMonitorClient;

        /// <summary>
        /// Gets the list of registered <see cref="IActivityMonitorClient"/>.
        /// </summary>
        IReadOnlyList<IActivityMonitorClient> Clients { get; }

        /// <summary>
        /// Gets an entry point for other monitors: by registering <see cref="ActivityMonitorBridge"/> in other <see cref="IActivityMonitor.Output"/>
        /// bound to this <see cref="ActivityMonitorBridgeTarget"/>, log streams can easily be merged.
        /// </summary>
        ActivityMonitorBridgeTarget BridgeTarget { get; }
    }

}
