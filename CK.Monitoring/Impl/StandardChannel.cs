using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    internal sealed class StandardChannel : IChannel
    {
        readonly IGrandOutputSink _common;
        readonly IReadOnlyList<ConfiguredSink> _sinks;
        readonly IRouteConfigurationLock _configLock;
        readonly string _configurationName;
        int _inputCount;

        internal StandardChannel( IGrandOutputSink common, IRouteConfigurationLock configLock, IReadOnlyList<ConfiguredSink> sinks, string configurationName )
        {
            _common = common;
            _configLock = configLock;
            _sinks = sinks;
            _configurationName = configurationName;
        }

        public GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
        {
            Interlocked.Increment( ref _inputCount );
            return new GrandOutputSource( monitor, channelName );
        }

        public void ReleaseInput( GrandOutputSource source )
        {
            Interlocked.Decrement( ref _inputCount );
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _common.Handle( logEvent );
            ThreadPool.QueueUserWorkItem( o =>
            {
                try
                {
                    foreach( var s in _sinks ) s.Handle( logEvent );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.LoggingError.Add( ex, "While logging event." );
                }
                finally
                {
                    _configLock.Unlock();
                }
            } );
        }

        public void HandleBuffer( List<GrandOutputEventInfo> list )
        {
            ThreadPool.QueueUserWorkItem( o =>
            {
                try
                {
                    foreach( var e in list )
                    {
                        foreach( var s in _sinks ) s.Handle( e );
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
            } );
        }

        public LogLevelFilter MinimalFilter 
        {
            get { return LogLevelFilter.None; } 
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
