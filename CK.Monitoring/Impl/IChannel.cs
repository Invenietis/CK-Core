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
    /// Abstraction of a Channel: it is a sink that knows how to <see cref="IGrandOutputSink.Handle"/> log events
    /// and creates a <see cref="GrandOutputSource"/> for each monitor bound to it.
    /// </summary>
    internal interface IChannel : IGrandOutputSink
    {
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
        LogLevelFilter MinimalFilter { get; }
    }
}
