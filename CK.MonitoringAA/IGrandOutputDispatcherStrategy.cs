#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\IGrandOutputDispatcherStrategy.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace CK.Monitoring
{

    /// <summary>
    /// Defines a strategy to manage dispatching log events overload and idle time management.
    /// </summary>
    public interface IGrandOutputDispatcherStrategy
    {
        /// <summary>
        /// Called once and only once during <see cref="GrandOutput"/> initialization.
        /// </summary>
        /// <param name="instantLoad">Gets the number of items waiting to be processed.</param>
        /// <param name="dispatcher">The dispatcher thread.</param>
        /// <param name="idleManager">Function that returns the time in milliseconds to wait for a given idle count.</param>
        void Initialize( Func<int> instantLoad, Thread dispatcher, out Func<int,int> idleManager );

        /// <summary>
        /// Called concurrently for each new event to handle: this must be fully thread-safe and as much efficient as it could be 
        /// since this is called on the monitored side.
        /// </summary>
        /// <returns>True to accept the event, false to reject it.</returns>
        bool IsOpened( ref int maxQueuedCount );

        /// <summary>
        /// Gets the count of concurrent sampling: each time <see cref="IsOpened"/> has been
        /// called while it was already called by another thread.
        /// </summary>
        int IgnoredConcurrentCallCount { get; }
    }
}
