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
        readonly IGrandOutputSink _common;
        readonly IReadOnlyList<HandlerBase> _handlers;
        readonly IRouteConfigurationLock _configLock;
        readonly string _configurationName;
        LogLevelFilter _minimalFilter;
        int _inputCount;

        internal StandardChannel( IGrandOutputSink common, IRouteConfigurationLock configLock, IReadOnlyList<HandlerBase> handlers, string configurationName )
        {
            _common = common;
            _configLock = configLock;
            _handlers = handlers;
            _configurationName = configurationName;
        }

        public void Initialize()
        {
            ChannelOption option = new ChannelOption();
            foreach( var s in _handlers ) s.CollectChannelOption( option );
            _minimalFilter = option.CurrentMinimalFilter;
        }

        public GrandOutputSource CreateSource( IActivityMonitorImpl monitor, string channelName )
        {
            Interlocked.Increment( ref _inputCount );
            return new GrandOutputSource( monitor, channelName );
        }

        public void ReleaseSource( GrandOutputSource source )
        {
            Interlocked.Decrement( ref _inputCount );
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _common.Handle( logEvent );
            // HandleWaitCallback avoids a closure.
            // Use the threadpool to guaranty that the message handling will be executed
            // on another thread.
            ThreadPool.QueueUserWorkItem( HandleWaitCallback, logEvent );
        }

        void HandleWaitCallback( object logEventObject )
        {
            GrandOutputEventInfo logEvent = (GrandOutputEventInfo)logEventObject;
            try
            {
                foreach( var s in _handlers ) s.Handle( logEvent );
            }
            catch( Exception ex )
            {
                ActivityMonitor.LoggingError.Add( ex, "While logging event." );
            }
            finally
            {
                _configLock.Unlock();
            }
        }

        public void HandleBuffer( List<GrandOutputEventInfo> list )
        {
            // HandleWaitCallbackBuffer avoids a closure.
            // Using the thread pool here is a good idea: it triggers an immediate 
            // schedule of a pooled thread.
            ThreadPool.QueueUserWorkItem( HandleWaitCallbackBuffer, list );
        }

        void HandleWaitCallbackBuffer( object listObject )
        {
            List<GrandOutputEventInfo> list = (List<GrandOutputEventInfo>)listObject;
            try
            {
                foreach( var e in list )
                {
                    foreach( var s in _handlers ) s.Handle( e );
                }
            }
            catch( Exception ex )
            {
                ActivityMonitor.LoggingError.Add( ex, "While logging event." );
            }
            finally
            {
                _configLock.Unlock();
            }
        }

        public LogFilter MinimalFilter 
        {
            get { return LogFilter.Undefined; } 
        }

        public void PreHandleLock()
        {
            _configLock.Lock();
        }

        public void CancelPreHandleLock()
        {
            _configLock.Unlock();
        }

    }
}
