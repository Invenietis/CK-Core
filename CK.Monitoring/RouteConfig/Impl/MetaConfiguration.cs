using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Base class for meta configuration object: those objects configure the configuration.
    /// </summary>
    public abstract class MetaConfiguration
    {
        /// <summary>
        /// Check the configuration validity.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        protected internal abstract bool CheckValidity( string routeName, IActivityMonitor monitor );

        /// <summary>
        /// Applies the configuration.
        /// By default, adds this meta configuration to the context for <see cref="Apply(IRouteConfigurationContext)"/> to be called.
        /// </summary>
        /// <param name="protoContext">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal virtual void Apply( IProtoRouteConfigurationContext protoContext )
        {
            protoContext.AddMeta( this );
        }

        /// <summary>
        /// Applies the configuration (second step).
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal abstract void Apply( IRouteConfigurationContext context );

        static public bool CheckActionNameValidity( string routeName, IActivityMonitor monitor, string nameToCheck )
        {
            if( String.IsNullOrWhiteSpace( nameToCheck ) ) monitor.Error().Send( "Invalid name '{0}' in route '{1}'. Name must not be empty or contains only white space.", nameToCheck, routeName );
            else if( nameToCheck.Contains( '/' ) ) monitor.Error().Send( "Invalid name '{0}' in route '{1}'. Name must not contain '/'.", nameToCheck, routeName );
            else return true;
            return false;
        }

    }
}
