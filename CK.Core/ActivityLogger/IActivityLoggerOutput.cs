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
    /// Offers <see cref="IActivityLoggerClient"/> registration/unregistration and exposes an <see cref="ExternalInput"/> 
    /// (an <see cref="ActivityLoggerBridgeTarget"/>) that can be used to accept logs from other loggers.
    /// </summary>
    public interface IActivityLoggerOutput
    {
        /// <summary>
        /// Registers an <see cref="IActivityLoggerClient"/> to the <see cref="RegisteredClients"/> list.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IActivityLoggerOutput RegisterClient( IActivityLoggerClient client );

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerClient"/> from the <see cref="RegisteredClients"/> list.
        /// Silently ignored unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IActivityLoggerOutput UnregisterClient( IActivityLoggerClient client );

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        IReadOnlyList<IActivityLoggerClient> RegisteredClients { get; }

        /// <summary>
        /// Gets an entry point for other loggers: by registering <see cref="ActivityLoggerBridge"/> in other <see cref="IActivityLogger.Output"/>
        /// bound to this <see cref="ActivityLoggerBridgeTarget"/>, log streams can easily be merged.
        /// </summary>
        ActivityLoggerBridgeTarget ExternalInput { get; }
    }

}
