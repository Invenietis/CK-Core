using CK.RouteConfig;

namespace CK.Monitoring
{
    public abstract class ConfiguredSink
    {
        readonly string _name;

        protected ConfiguredSink( ActionConfiguration config )
        {
            _name = config.Name;
        }

        /// <summary>
        /// Gets the name of this sink. It is the name of its configuration.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Initializes this sink. 
        /// This is called once for all the configured sink at the start of a new 
        /// configuration, before the first call to <see cref="Handle"/>.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Enables this sink to interact with any channel to which it belongs. 
        /// This is called after <see cref="Initialize"/> and for each channel where this sink appears, before the first call to <see cref="Handle"/>.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void CollectChannelOption( ChannelOption option )
        {
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        public abstract void Handle( GrandOutputEventInfo logEvent );

        /// <summary>
        /// Closes this sink.
        /// This is called when a reconfiguration occurs after all
        /// events have been <see cref="Handle"/>d.
        /// Default implementation does nothing.
        /// </summary>
        public void Close()
        {
        }

    }
}
