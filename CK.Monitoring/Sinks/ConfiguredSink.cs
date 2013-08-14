using CK.RouteConfig;

namespace CK.Monitoring
{
    public abstract class ConfiguredSink : IGrandOutputSink
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
        /// Handles a <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        public abstract void Handle( GrandOutputEventInfo logEvent );

        /// <summary>
        /// Closes this sink.
        /// This is called when a reconfiguration occurs after all
        /// events have been <see cref="Handle"/>d.
        /// </summary>
        public void Close()
        {
        }
    }
}
