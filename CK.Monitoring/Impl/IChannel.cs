using CK.Core;

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
