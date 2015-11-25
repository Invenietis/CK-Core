using CK.Core;
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

        internal StandardChannel( IGrandOutputSink commonSink, EventDispatcher dispatcher, IRouteConfigurationLock configLock, HandlerBase[] handlers, string configurationName, GrandOutputChannelConfigData configData )
        {
            _dispatcher = dispatcher;
            _receiver = new EventDispatcher.FinalReceiver( commonSink, handlers, configLock );
            _receiverNoCommonSink = new EventDispatcher.FinalReceiver( null, handlers, configLock );
            _configurationName = configurationName;
            if( configData != null ) _minimalFilter = configData.MinimalFilter;
        }

        public void Initialize()
        {
            ChannelOption option = new ChannelOption( _minimalFilter );
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
