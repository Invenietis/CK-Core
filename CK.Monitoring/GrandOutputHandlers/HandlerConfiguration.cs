using System.Xml.Linq;
using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Base class for handler configuration.
    /// </summary>
    public abstract class HandlerConfiguration : ActionConfiguration
    {
        /// <summary>
        /// Initializes a new handler configuration.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        protected HandlerConfiguration( string name )
            : base( name )
        {
        }

        /// <summary>
        /// Gets or sets the minimal filter for this file handler.
        /// Defaults to <see cref="LogFilter.Undefined"/>: unless specified, the handler will log 
        /// whatever it receives thanks to the other minimal filters in the system.
        /// </summary>
        public LogFilter MinimalFilter { get; set; }

        internal void DoInitialize( IActivityMonitor m, XElement xml )
        {
            MinimalFilter = xml.GetAttributeLogFilter( "MinimalFilter", true ).Value;
            Initialize( m, xml );
        }
        
        /// <summary>
        /// Must initializes this configuration object with its specific data from an xml element.
        /// </summary>
        /// <param name="m">Monitor to use.</param>
        /// <param name="xml">The xml element.</param>
        protected abstract void Initialize( IActivityMonitor m, XElement xml );
    }
}
