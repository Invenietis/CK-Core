using System;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;

namespace CK.Monitoring.Impl
{
    /// <summary>
    /// Actual specialization of the generic <see cref="ConfiguredRouteHost{TAction,TRoute}"/> with <see cref="HandlerBase"/> and <see cref="IChannel"/>.
    /// </summary>
    internal class ChannelHost : ConfiguredRouteHost<HandlerBase, IChannel>
    {
        public ChannelHost( ChannelFactory handlerFactory, Action<ConfigurationReady> readyCallback )
            : base( handlerFactory, readyCallback, ( m, a ) => a.Initialize( m ), ( m, a ) => a.Close( m ) )
        {
        }

    }
}
