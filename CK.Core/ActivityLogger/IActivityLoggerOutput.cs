#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IActivityLoggerOutput.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Combines the two registrars (<see cref="IActivityLoggerClientRegistrar"/> and <see cref="IMuxActivityLoggerClientRegistrar"/>)
    /// and exposes an <see cref="ExternalInput"/> (an <see cref="IMuxActivityLoggerClient"/>) that can be registered as a
    /// client far any number of other loggers.
    /// </summary>
    public interface IActivityLoggerOutput : IActivityLoggerClientRegistrar, IMuxActivityLoggerClientRegistrar
    {
        /// <summary>
        /// Gets an entry point for other loggers: by registering this <see cref="IMuxActivityLoggerClient"/> in other <see cref="IActivityLogger.Output"/>,
        /// log streams can easily be merged.
        /// </summary>
        IMuxActivityLoggerClient ExternalInput { get; }

        /// <summary>
        /// Gets a modifiable list of either <see cref="IMuxActivityLoggerClient"/> or <see cref="IActivityLoggerClient"/>
        /// that can not be removed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Already registered hybrid clients (that support both <see cref="IMuxActivityLoggerClient"/> and <see cref="IActivityLoggerClient"/>)
        /// can be added at any time in <see cref="IActivityLoggerClientRegistrar.RegisteredClients"/> or <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients"/>:
        /// they are automatically removed from the other registrar.
        /// </para>
        /// <para>
        /// This behavior (that avoids stuterring: logs sent twice since the same client is registered in both registrar), applies also to clients that are 
        /// registered in this NonRemoveableClients list. This list simply guraranty that an <see cref="InvalidOperationException"/> will be thrown 
        /// if a call to <see cref="IActivityLoggerClientRegistrar.UnregisterClient"/> or <see cref="IMuxActivityLoggerClientRegistrar.UnregisterMuxClient"/> is 
        /// done on a non removeable client.
        /// </para>
        /// </remarks>
        IList<IActivityLoggerClientBase> NonRemoveableClients { get; }
    }

}
