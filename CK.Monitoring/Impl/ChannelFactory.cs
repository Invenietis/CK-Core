using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.RouteConfig;
using CK.Monitoring.GrandOutputHandlers;

namespace CK.Monitoring.Impl
{
    internal sealed class ChannelFactory : RouteActionFactory<HandlerBase,IChannel>
    {
        readonly IGrandOutputSink _commonSink;
        
        sealed class EmptyChannel : IChannel
        {
            readonly IGrandOutputSink _commonSink;

            public EmptyChannel( IGrandOutputSink common )
            {
                _commonSink = common;
            }

            public void Initialize()
            {
            }

            public GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
            {
                return new GrandOutputSource( monitor, channelName );
            }

            public void ReleaseInput( GrandOutputSource source )
            {
            }

            public void Handle( GrandOutputEventInfo logEvent )
            {
                _commonSink.Handle( logEvent );
            }

            public void HandleBuffer( List<GrandOutputEventInfo> events )
            {
                // Common sink has already handled the events.
            }

            public LogFilter MinimalFilter
            {
                get { return LogFilter.Undefined; }
            }

            public void PreHandleLock()
            {
            }

            public void CancelPreHandleLock()
            {
            }
        }

        internal ChannelFactory( IGrandOutputSink commonSink )
        {
            _commonSink = commonSink;
        }

        protected internal override IChannel DoCreateEmptyFinalRoute()
        {
            return new EmptyChannel( _commonSink );
        }

        protected override HandlerBase DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
        {
            var a = (HandlerTypeAttribute)Attribute.GetCustomAttribute( c.GetType(), typeof( HandlerTypeAttribute ), true );
            return (HandlerBase)Activator.CreateInstance( a.HandlerType, c );
        }

        protected override HandlerBase DoCreateParallel( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionParallelConfiguration c, HandlerBase[] children )
        {
            return new ParallelHandler( c, children );  
        }

        protected override HandlerBase DoCreateSequence( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionSequenceConfiguration c, HandlerBase[] children )
        {
            return new SequenceHandler( c, children );
        }

        protected internal override IChannel DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, HandlerBase[] actions, string configurationName )
        {
            return new StandardChannel( _commonSink, configLock, actions, configurationName );
        }
    }
}
