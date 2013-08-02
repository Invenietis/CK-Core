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

        public MetaDeclareRouteConfiguration( SubRouteConfiguration route )
        {
            _route = route;
        }

        public SubRouteConfiguration RouteConfiguration
        {
            get { return _route; }
        }

        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            protoContext.AddRoute( _route );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
