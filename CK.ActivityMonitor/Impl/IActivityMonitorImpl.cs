#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\IActivityMonitorImpl.cs) is part of CiviKey. 
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Impl
{
    /// <summary>
    /// Defines required aspects that an actual monitor implementation must support.
    /// </summary>
    public interface IActivityMonitorImpl : IActivityMonitor, IUniqueId
    {
        /// <summary>
        /// Gets the currently opened group.
        /// Null when no group is currently opened.
        /// </summary>
        IActivityLogGroup CurrentGroup { get; }

        /// <summary>
        /// Gets a disposable object that checks for reentrant and concurrent calls.
        /// </summary>
        /// <returns>A disposable object (that must be disposed).</returns>
        IDisposable ReentrancyAndConcurrencyLock();

        /// <summary>
        /// Enables a <see cref="IActivityMonitorBoundClient"/> to warn its Monitor
        /// whenever its <see cref="IActivityMonitorBoundClient.MinimalFilter"/> changed.
        /// This can be called from any <see cref="IActivityMonitorBoundClient"/> methods (when a <see cref="ReentrancyAndConcurrencyLock"/> has 
        /// been acquired) or not, but NOT concurrently: <see cref="SetClientMinimalFilterDirty"/> must be used to signal
        /// a change on any thread at any time.
        /// </summary>
        /// <param name="oldLevel">The previous minimal level that the client expected.</param>
        /// <param name="newLevel">The new minimal level that the client expects.</param>
        void OnClientMinimalFilterChanged( LogFilter oldLevel, LogFilter newLevel );

        /// <summary>
        /// Signals the monitor that one of the <see cref="IActivityMonitorBoundClient.MinimalFilter"/> has changed:
        /// the <see cref="IActivityMonitor.ActualFilter"/> is marked as needing a re computation in a thread-safe manner.
        /// This can be called by bound clients on any thread at any time as opposed to <see cref="OnClientMinimalFilterChanged"/>
        /// that can only be called non-concurrently (typically from inside client methods).
        /// </summary>
        void SetClientMinimalFilterDirty();

        /// <summary>
        /// Enables <see cref="IActivityMonitorBoundClient"/> clients to initialize Topic and AutoTag typically from 
        /// inside their <see cref="IActivityMonitorBoundClient.SetMonitor"/> method or any other methods provided 
        /// that a reentrant and concurrent lock has been obtained (otherwise an <see cref="InvalidOperationException"/> is thrown).
        /// </summary>
        /// <param name="newTopic">New topic to set. When null, it is ignored.</param>
        /// <param name="newTags">new tags to set. When null, it is ignored.</param>
        /// <param name="fileName">Source file name of the caller. Do not set it: the attribute will do the job.</param>
        /// <param name="lineNumber">Line number in the source file. Do not set it: the attribute will do the job.</param>
        void InitializeTopicAndAutoTags( string newTopic, CKTrait newTags, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 );

    }
}
