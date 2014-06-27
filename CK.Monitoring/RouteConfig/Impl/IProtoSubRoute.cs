using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Specialize <see cref="IProtoRoute"/> to expose a <see cref="SubRouteConfiguration"/>.
    /// A SubRouteConfiguration specifies the <see cref="SubRouteConfiguration.RoutePredicate"/> that is 
    /// the filter for the subordinated route.
    /// </summary>
    public interface IProtoSubRoute : IProtoRoute
    {
        /// <summary>
        /// Masked base <see cref="IProtoRoute.Configuration"/> so that it is a <see cref="SubRouteConfiguration"/>.
        /// </summary>
        new SubRouteConfiguration Configuration { get; }
    }
}
