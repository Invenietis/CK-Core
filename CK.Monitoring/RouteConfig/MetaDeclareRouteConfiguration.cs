using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Declares a new <see cref="SubRouteConfiguration"/>.
    /// </summary>
    public class MetaDeclareRouteConfiguration : Impl.MetaConfiguration
    {
        readonly SubRouteConfiguration _route;

        /// <summary>
        /// Initializes a new <see cref="MetaDeclareRouteConfiguration"/> bound to a <see cref="SubRouteConfiguration"/>.
        /// </summary>
        /// <param name="route">Subordinated route configuration.</param>
        public MetaDeclareRouteConfiguration( SubRouteConfiguration route )
        {
            _route = route;
        }

        /// <summary>
        /// Gets the subordinated route configuration.
        /// </summary>
        public SubRouteConfiguration RouteConfiguration
        {
            get { return _route; }
        }

        /// <summary>
        /// Always true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains this configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>Always true.</returns>
        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        /// <summary>
        /// Applies the configuration (first step) by calling <see cref="Impl.IProtoRouteConfigurationContext.AddRoute"/>.
        /// </summary>
        /// <param name="protoContext">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            protoContext.AddRoute( _route );
        }

        /// <summary>
        /// Never called: the first <see cref="Apply(Impl.IProtoRouteConfigurationContext)"/> does not register this object
        /// since we have nothing more to do than adding the route.
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
