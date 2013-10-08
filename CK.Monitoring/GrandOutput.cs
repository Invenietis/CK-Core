using System;
using System.Collections.Generic;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    public partial class GrandOutput
    {
        readonly List<WeakRef<GrandOutputClient>> _clients;
        readonly GrandOutputCompositeSink _commonSink;
        readonly ChannelHost _channelHost;
        readonly BufferingChannel _bufferingChannel;

        public GrandOutput()
        {
            _clients = new List<WeakRef<GrandOutputClient>>();
            _commonSink = new GrandOutputCompositeSink();
            _channelHost = new ChannelHost( new ChannelFactory( _commonSink ), OnConfigurationReady );
            _channelHost.ConfigurationClosing += OnConfigurationClosing;
            _bufferingChannel = new BufferingChannel( _commonSink );
        }

        /// <summary>
        /// Ensures that a client for this GrandOutput is registered on a monitor.
        /// </summary>
        /// <param name="monitor">The monitor onto which a <see cref="GrandOutputClient"/> must be registered.</param>
        /// <returns>A newly created client or the already existing one.</returns>
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
            return monitor.Output.RegisterUniqueClient( b => b.Central == this, reg );
        }

        /// <summary>
        /// Registers a <see cref="IGrandOutputSink"/>.
        /// </summary>
        /// <param name="sink">The sink to register.</param>
        public void RegisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Add( sink );
        }

        /// <summary>
        /// Unregisters a <see cref="IGrandOutputSink"/>.
        /// </summary>
        /// <param name="sink">The sink to unregister.</param>
        public void UnregisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Remove( sink );
        }

        /// <summary>
        /// Obtains an actual channel from its full name.
        /// This is called on the monitor's thread.
        /// </summary>
        /// <param name="channelName">The full channel name. Used as the key to find an actual Channel that must handle the log events.</param>
        /// <returns>A <see cref="StandardChannel"/> for the channelName, or an internal BufferingChannel if the configuration is being applied.</returns>
        internal IChannel ObtainChannel( string channelName )
        {
            var channel = _channelHost.ObtainRoute( channelName );
            if( channel == null )
            {
                lock( _bufferingChannel.FlushLock )
                {
                    channel = _channelHost.ObtainRoute( channelName );
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
        void OnConfigurationClosing( object sender, ConfiguredRouteHost<HandlerBase, IChannel>.ConfigurationClosingEventArgs e )
        {
            lock( _bufferingChannel.FlushLock ) _bufferingChannel.EnsureActive();
            SignalConfigurationChanged();
        }

        void OnConfigurationReady( ChannelHost.ConfigurationReady e )
        {
            foreach( var channel in e.GetAllRoutes() )
            {
                channel.Initialize(); 
            }
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
