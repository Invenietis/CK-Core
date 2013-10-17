using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig.Impl;

namespace CK.RouteConfig
{
    public class SubRouteConfigurationResolved : RouteConfigurationResolved
    {
        readonly Func<string,bool> _routePredicate;

        internal SubRouteConfigurationResolved( IProtoSubRoute c, IReadOnlyList<ActionConfigurationResolved> actions )
            : base( c.FullName, c.Configuration.ConfigData, actions )
        {
            _routePredicate = c.Configuration.RoutePredicate;
        }

        /// <summary>
        /// Gets the filter that route must respect to enter this sub route.
        /// </summary>
        public Func<string, bool> RoutePredicate { get { return _routePredicate; } }
    }

}
