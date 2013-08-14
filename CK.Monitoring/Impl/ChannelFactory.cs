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
    internal class ChannelFactory : RouteActionFactory<ConfiguredSink,IChannel>
    {
        readonly IGrandOutputSink _commonSink;
        
        class EmptyChannel : IChannel
        {
            readonly IGrandOutputSink _commonSink;

            public EmptyChannel( IGrandOutputSink common )
            {
                _commonSink = common;
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

            public LogLevelFilter MinimalFilter
            {
                get { return LogLevelFilter.None; }
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

        CountdownEvent _useLock;

        protected override void DoInitialize()
        {
            Debug.Assert( _useLock == null );
            _useLock = new CountdownEvent( 1 );
        }

        protected override ConfiguredSink DoCreate( IActivityMonitor monitor, ActionConfiguration c )
        {
            ConfiguredSinkTypeAttribute a = (ConfiguredSinkTypeAttribute)c.GetType().GetCustomAttributes( typeof( ConfiguredSinkTypeAttribute ), true ).Single();
            return (ConfiguredSink)Activator.CreateInstance( a.SinkType, c );
        }

        protected override ConfiguredSink DoCreateParallel( IActivityMonitor monitor, ActionParallelConfiguration c, ConfiguredSink[] children )
        {
            return new ConfiguredSinkParallel( c, children );  
        }

        protected override ConfiguredSink DoCreateSequence( IActivityMonitor monitor, ActionSequenceConfiguration c, ConfiguredSink[] children )
        {
            return new ConfiguredSinkSequence( c, children );
        }

        protected internal override IChannel DoCreateFinalRoute( IActivityMonitor monitor, ConfiguredSink[] actions, string configurationName )
        {
            return new StandardChannel( _commonSink, _useLock, actions, configurationName );
        }

        protected override void DoUninitialize( bool success )
        {
            Debug.Assert( _useLock != null );
            if( !success ) _useLock.Dispose();
            _useLock = null;
        }
    }
}
