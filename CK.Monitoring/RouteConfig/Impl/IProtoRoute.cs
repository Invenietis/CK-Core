using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig.Impl
{
    public interface IProtoRoute
    {
        /// <summary>
        /// Gets the associated <see cref="RouteConfiguration"/> object.
        /// </summary>
        RouteConfiguration Configuration { get; }

        string Namespace { get; }

        string FullName { get; }

        IReadOnlyList<MetaConfiguration> MetaConfigurations { get; }

        ActionConfiguration FindDeclaredAction( string name );

        IReadOnlyList<IProtoSubRoute> SubRoutes { get; }
    }

}
