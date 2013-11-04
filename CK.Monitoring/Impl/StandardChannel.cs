using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.GrandOutputHandlers;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    internal sealed class StandardChannel : IChannel
    {
        readonly EventDispatcher _dispatcher;
        readonly EventDispatcher.FinalReceiver _receiver;
        readonly EventDispatcher.FinalReceiver _receiverNoCommonSink;
        readonly string _configurationName;
        LogFilter _minimalFilter;

        internal StandardChannel( IGrandOutputSink common, EventDispatcher dispatcher, IRouteConfigurationLock configLock, HandlerBase[] handlers, string configurationName )
        {
            _dispatcher = dispatcher;
            _receiver = new EventDispatcher.FinalReceiver( common, handlers, configLock );
            _receiverNoCommonSink = new EventDispatcher.FinalReceiver( null, handlers, configLock );
            _configurationName = configurationName;
        }

        public void Initialize()
        {
            ChannelOption option = new ChannelOption();
            foreach( var s in _receiver.Handlers ) s.CollectChannelOption( option );
            _minimalFilter = option.CurrentMinimalFilter;
        }

        public void Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
        {
            if( sendToCommonSink ) _dispatcher.Add( logEvent, _receiver );
            else _dispatcher.Add( logEvent, _receiverNoCommonSink );
        }

        public LogFilter MinimalFilter 
        {
            get { return _minimalFilter; } 
        }

        public void PreHandleLock()
        {
            _receiver.ConfigLock.Lock();
        }

        public void CancelPreHandleLock()
        {
            _receiver.ConfigLock.Unlock();
        }

    }
}
