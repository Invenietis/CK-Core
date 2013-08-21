using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    public partial class GrandOutput
    {
        readonly List<WeakRef<GrandOutputClient>> _clients;
        readonly GrandOutputCompositeSink _commonSink;
        readonly ChannelFactory _sinkFactory;
        readonly ConfiguredRouteHost<ConfiguredSink,IChannel> _routeHost;
        readonly BufferingChannel _bufferingChannel;

        public GrandOutput()
        {
            _clients = new List<WeakRef<GrandOutputClient>>();
            _commonSink = new GrandOutputCompositeSink();
            _sinkFactory = new ChannelFactory( _commonSink );
            _routeHost = new ConfiguredRouteHost<ConfiguredSink, IChannel>( _sinkFactory, OnConfigurationReady, ( m, a ) => a.Initialize(), ( m, a ) => a.Close() );
            _routeHost.ConfigurationClosing += OnConfigurationClosing;
            _bufferingChannel = new BufferingChannel( _commonSink );
        }

        public GrandOutputClient Register( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );           
            Func<GrandOutputClient> reg = () =>
                {
                    var c = new GrandOutputClient( this );
                    lock( _clients ) 
                    {
                        _clients.Add( new WeakRef<GrandOutputClient>( c ) ); 
                    }
                    return c;
                };
            return monitor.Output.AtomicRegisterClient( b => b.Central == this, reg );
        }

        public void RegisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Add( sink );
        }

        public void UnregisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Remove( sink );
        }

        /// <summary>
        /// Obtains an actual channel from its full name.
        /// This is called on the monitor's thread.
        /// </summary>
        /// <param name="monitorId">The monitor identifier. Used only when routes are beeing reconfigured to return a BufferingChannel dedicated to the monitor.</param>
        /// <param name="channelName">The full channel name. Used as the key to find an actual Channel that must handle the log events.</param>
        /// <returns>A <see cref="StandardChannel"/> for the channelName, or an internal BufferingChannel if the configuration is being applied.</returns>
        internal IChannel ObtainChannel( Guid monitorId, string channelName )
        {
            var channel = _routeHost.ObtainRoute( channelName );
            if( channel == null )
            {
                lock( _bufferingChannel.FlushLock )
                {
                    channel = _routeHost.ObtainRoute( channelName );
                    if( channel == null )
                    {
                        _bufferingChannel.EnsureActive();
                        _bufferingChannel.PreHandleLock();
                        channel = _bufferingChannel;
                    }
                }
            }
            return channel;
        }

        /// <summary>
        /// This is called by the host when current configuration must be closed.
        /// </summary>
        void OnConfigurationClosing( object sender, ConfiguredRouteHost<ConfiguredSink, IChannel>.ConfigurationClosingEventArgs e )
        {
            lock( _bufferingChannel.FlushLock ) _bufferingChannel.EnsureActive();
            SignalConfigurationChanged();
        }

        void OnConfigurationReady( ConfiguredRouteHost<ConfiguredSink, IChannel>.ConfigurationReady e )
        {
            lock( _bufferingChannel.FlushLock )
            {
                _bufferingChannel.FlushBuffer( e.IsClosed ? (Func<string,IChannel>)null : e.ObtainRoute );
                e.ApplyConfiguration();
            }
            SignalConfigurationChanged();
        }

        void SignalConfigurationChanged()
        {
            WeakRef<GrandOutputClient>[] current;
            lock( _clients ) current = _clients.ToArray();
            foreach( var cw in current )
            {
                GrandOutputClient c = cw.Target;
                if( c != null ) c.OnChannelConfigurationChanged();
            }
        }


        private void DoGarbageDeadClients()
        {
            throw new NotImplementedException();
        }
    }
}
