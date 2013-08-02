using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    public interface IProtoRouteConfigurationContext
    {
        IActivityMonitor Monitor { get; }

        bool AddRoute( SubRouteConfiguration route );

        bool DeclareAction( ActionConfiguration a, bool overridden );
        
        void AddMeta( MetaConfiguration meta );
    }
}
