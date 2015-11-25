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
        /// Gets or sets the minimal filter for this handler.
        /// Defaults to <see cref="LogFilter.Undefined"/>: unless specified, the handler will not 
        /// participate to <see cref="IActivityMonitor.ActualFilter"/> resolution of monitors that eventually 
        /// are handled by this handler.See remarks. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is NOT a filter: this is the minimal filter that guaranties that, at least, the specified 
        /// levels will reach this handler.
        /// </para>
        /// <para>
        /// A concrete handler can, if needed, define a true filter: it is its business to retain or forget 
        /// what it wants.
        /// </para>
        /// </remarks>
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
