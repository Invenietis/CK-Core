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
    /// Defines a strategy to manage dispatching log events overload.
    /// </summary>
    public interface IGrandOutputDispatcherStrategy
    {
        /// <summary>
        /// Called once and only once during <see cref="GrandOutput"/> initialization.
        /// </summary>
        /// <param name="instantLoad">Offers access to the number of items waiting to be processed.</param>
        /// <param name="dispatcher">The dispatcher thread.</param>
        void Initialize( Func<int> instantLoad, Thread dispatcher );

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
