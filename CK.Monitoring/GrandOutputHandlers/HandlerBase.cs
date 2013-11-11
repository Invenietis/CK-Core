using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    public abstract class HandlerBase : IGrandOutputSink
    {
        readonly string _name;
        readonly LogFilter _minimalFilter;

        /// <summary>
        /// Internal constructor used by Sequence and Parallel.
        /// </summary>
        /// <param name="config">Parallel or sequence configuration.</param>
        internal HandlerBase( CK.RouteConfig.Impl.ActionCompositeConfiguration config )
        {
            _name = config.Name;
            _minimalFilter = LogFilter.Undefined;
        }

        protected HandlerBase( HandlerConfiguration config )
        {
            _name = config.Name;
            _minimalFilter = config.MinimalFilter;
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
        /// <param name="monitor">The monitor that tracks configuration process.</param>
        public virtual void Initialize( IActivityMonitor monitor )
        {
        }

        /// <summary>
        /// Enables this sink to interact with any channel to which it belongs. 
        /// This is called after <see cref="Initialize"/> and for each channel where this sink appears, before the first call to <see cref="Handle"/>.
        /// Default implementation must be called: sets the minimal filter on the option if the <see cref="HandlerConfiguration"/> defines it.
        /// </summary>
        public virtual void CollectChannelOption( ChannelOption option )
        {
            option.SetMinimalFilter( _minimalFilter );
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        /// <param name="parrallelCall">True when this method is called in parallel with other handlers.</param>
        public abstract void Handle( GrandOutputEventInfo logEvent, bool parrallelCall );

        /// <summary>
        /// Closes this handler.
        /// This is called when a reconfiguration occurs after all
        /// events have been <see cref="Handle"/>d.
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="monitor">The monitor that tracks configuration process.</param>
        public virtual void Close( IActivityMonitor monitor )
        {
        }

    }
}
