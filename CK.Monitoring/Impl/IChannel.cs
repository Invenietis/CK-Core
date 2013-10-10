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
    /// Abstraction of a Channel: it knows how to <see cref="Handle"/> log events
    /// and creates a <see cref="GrandOutputSource"/> for each monitor bound to it.
    /// </summary>
    internal interface IChannel
    {
        /// <summary>
        /// Called once the channel is ready to <see cref="Handle"/> events (but before the new configuration is actually applied).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Creates a source for a monitor.
        /// </summary>
        /// <param name="monitor">Monitor that uses this channel.</param>
        /// <param name="channelName">Full name of the required channel.</param>
        /// <returns>A <see cref="GrandOutputSource"/> that captures information related to the relation between a monitor and this channel/</returns>
        GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName );

        /// <summary>
        /// Called by <see cref="GrandOutputClient"/> when the currently bound channel is no more used.
        /// </summary>
        /// <param name="source">The source previously obtained by <see cref="CreateInput"/>.</param>
        void ReleaseInput( GrandOutputSource source );

        /// <summary>
        /// Gets the minimal log level that this channel expects. 
        /// Should default to <see cref="LogLevelFilter.None"/>.
        /// </summary>
        LogFilter MinimalFilter { get; }

        /// <summary>
        /// Locks the channel: a call to <see cref="Handle"/> is pending.
        /// This is required to avoid a race condition between obtention of the Channel by a GrandOutputClient 
        /// and the call to Handle.
        /// </summary>
        void PreHandleLock();

        /// <summary>
        /// Cancels a previous call to <see cref="PreHandleLock"/>.
        /// This is used when the Channel to use must be changed during its obtention by a GrandOutputClient. 
        /// </summary>
        void CancelPreHandleLock();

        /// <summary>
        /// Handles one event.
        /// This is called by GrandOutputClients that are bound to this channel.
        /// The lock previously obtained by a call to PreHandleLock is released.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        void Handle( GrandOutputEventInfo e );

        /// <summary>
        /// Handles multiple events that have been buffered.
        /// </summary>
        /// <param name="list">Buffered events.</param>
        void HandleBuffer( List<GrandOutputEventInfo> list );

    }
}
