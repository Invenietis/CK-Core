using System;
using System.Collections.Generic;
using CK.Core;
using CK.RouteConfig;
using CK.Monitoring.GrandOutputHandlers;
using System.Reflection;

namespace CK.Monitoring.Impl
{
    internal sealed class ChannelFactory : RouteActionFactory<HandlerBase, IChannel>, IChannel
    {
        public readonly GrandOutput _grandOutput;
        public readonly EventDispatcher _dispatcher;
        public readonly EventDispatcher.FinalReceiver CommonSinkOnlyReceiver;

        #region EmptyChannel direct implementation

        void IChannel.Initialize()
        {
        }

        void IChannel.Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
        {
            if( sendToCommonSink ) _dispatcher.Add( logEvent, CommonSinkOnlyReceiver );
        }

        LogFilter IChannel.MinimalFilter
        {
            get { return LogFilter.Undefined; }
        }

        void IChannel.PreHandleLock()
        {
        }

        void IChannel.CancelPreHandleLock()
        {
        }

        #endregion

        internal ChannelFactory( GrandOutput grandOutput, EventDispatcher dispatcher )
        {
            _grandOutput = grandOutput;
            _dispatcher = dispatcher;
            CommonSinkOnlyReceiver = new EventDispatcher.FinalReceiver( grandOutput.CommonSink, Util.Array.Empty<HandlerBase>(), null );
        }

        protected internal override IChannel DoCreateEmptyFinalRoute()
        {
            return this;
        }

        protected override HandlerBase DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
        {
            var a = c.GetType().GetTypeInfo().GetCustomAttribute<HandlerTypeAttribute>();
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
            return new StandardChannel( _grandOutput.CommonSink, _dispatcher, configLock, actions, configurationName, (GrandOutputChannelConfigData)configData );
        }
    }
}
