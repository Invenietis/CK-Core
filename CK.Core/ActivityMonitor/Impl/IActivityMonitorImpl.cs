using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets a disposable object that checks for reentrancy and concurrent calls.
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
        void OnClientMinimalFilterChanged( LogLevelFilter oldLevel, LogLevelFilter newLevel );

        /// <summary>
        /// Signals the monitor that one of the <see cref="IActivityMonitorBoundClient.MinimalFilter"/> has changed:
        /// the <see cref="IActivityMonitor.ActualFilter"/> is marked as needing a recomputation in a thread-safe manner.
        /// This can be called by bound clients on any thread at any time as opposed to <see cref="OnClientMinimalFilterChanged"/>
        /// that can only be called non-concurrently (typically from inside client methods).
        /// </summary>
        void SetClientMinimalFilterDirty();

    }
}
