using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;

namespace CK.Monitoring.Impl
{
    /// <summary>
    /// 
    /// </summary>
    internal class ChannelHost : ConfiguredRouteHost<HandlerBase, IChannel>
    {
        public ChannelHost( ChannelFactory handlerFactory, Action<ConfigurationReady> readyCallback )
            : base( handlerFactory, readyCallback, ( m, a ) => a.Initialize(), ( m, a ) => a.Close() )
        {
        }

    }
}
