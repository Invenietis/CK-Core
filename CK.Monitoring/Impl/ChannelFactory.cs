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

namespace CK.Monitoring.Impl
{
    internal sealed class ChannelFactory : RouteActionFactory<ConfiguredSink,IChannel>
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

            public LogLevelFilter MinimalFilter
            {
                get { return LogLevelFilter.None; }
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

        protected override ConfiguredSink DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
        {
            ConfiguredSinkTypeAttribute a = (ConfiguredSinkTypeAttribute)c.GetType().GetCustomAttributes( typeof( ConfiguredSinkTypeAttribute ), true ).Single();
            return (ConfiguredSink)Activator.CreateInstance( a.SinkType, c );
        }

        protected override ConfiguredSink DoCreateParallel( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionParallelConfiguration c, ConfiguredSink[] children )
        {
            return new ConfiguredSinkParallel( c, children );  
        }

        protected override ConfiguredSink DoCreateSequence( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionSequenceConfiguration c, ConfiguredSink[] children )
        {
            return new ConfiguredSinkSequence( c, children );
        }

        protected internal override IChannel DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, ConfiguredSink[] actions, string configurationName )
        {
            return new StandardChannel( _commonSink, configLock, actions, configurationName );
        }
    }
}
