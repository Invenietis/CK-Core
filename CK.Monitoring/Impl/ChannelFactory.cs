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
        readonly EventDispatcher _dispatcher;
        public readonly EventDispatcher.FinalReceiver CommonSinkOnlyReceiver;

        sealed class EmptyChannel : IChannel
        {
            readonly ChannelFactory _factory;

            public EmptyChannel( ChannelFactory f )
            {
                _factory = f;
            }

            public void Initialize()
            {
            }

            public void Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
            {
                if( sendToCommonSink ) _factory._dispatcher.Add( logEvent, _factory.CommonSinkOnlyReceiver );
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

        internal ChannelFactory( IGrandOutputSink commonSink, EventDispatcher dispatcher )
        {
            _commonSink = commonSink;
            _dispatcher = dispatcher;
            CommonSinkOnlyReceiver = new EventDispatcher.FinalReceiver( commonSink, Util.EmptyArray<HandlerBase>.Empty, null );
        }

        protected internal override IChannel DoCreateEmptyFinalRoute()
        {
            return new EmptyChannel( this );
        }

        protected override HandlerBase DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
        {
            var a = (HandlerTypeAttribute)Attribute.GetCustomAttribute( c.GetType(), typeof( HandlerTypeAttribute ), true );
            if( a == null ) throw new CKException( "A [HandlerType(typeof(H))] attribute (where H is a CK.Monitoring.GrandOutputHandlers.HandlerBase class) is missing on class '{0}'.", c.GetType().FullName );
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

        protected internal override IChannel DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, HandlerBase[] actions, string configurationName, object configData, IReadOnlyList<IChannel> routePath )
        {
            return new StandardChannel( _commonSink, _dispatcher, configLock, actions, configurationName, (GrandOutputChannelConfigData)configData );
        }
    }
}
