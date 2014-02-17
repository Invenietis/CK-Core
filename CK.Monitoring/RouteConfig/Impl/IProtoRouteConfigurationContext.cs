using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Temporary context used to resolve the routes/actions associations.
    /// </summary>
    public interface IProtoRouteConfigurationContext
    {
        /// <summary>
        /// Gets the monitor to use.
        /// </summary>
        IActivityMonitor Monitor { get; }

        /// <summary>
        /// Adds a new subordinated route.
        /// </summary>
        /// <param name="route">The new subordinated route.</param>
        /// <returns>True on success, false if an error occurred such as a name clash for the route.</returns>
        bool AddRoute( SubRouteConfiguration route );

        /// <summary>
        /// Declares an action that can be an override of an existing one.
        /// </summary>
        /// <param name="a">Action to declare.</param>
        /// <param name="overridden">True if the action overrides an existing one.</param>
        /// <returns>True on success, false if an error occurred such as a name clash for the action and it is not an override.</returns>
        bool DeclareAction( ActionConfiguration a, bool overridden );
        
        /// <summary>
        /// Adds a <see cref="MetaConfiguration"/> to the route.
        /// </summary>
        /// <param name="meta">The meta configuration.</param>
        void AddMeta( MetaConfiguration meta );
    }
}
