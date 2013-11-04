using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    /// <summary>
    /// A GrandOutput collects activity of multiple <see cref="IActivityMonitor"/>. It routes log events to 
    /// multiple channels based on <see cref="IActivityMonitor.Topic"/>.
    /// 
    /// It is usually useless to explicitely create an instance of GrandOutput: the <see cref="Default"/> one is 
    /// available as soon as <see cref="EnsureActiveDefault"/> is called and will be automatically used by new <see cref="ActivityMonitor"/>.
    /// </summary>
    public partial class GrandOutput : IDisposable
    {
        readonly List<WeakRef<GrandOutputClient>> _clients;
        readonly GrandOutputCompositeSink _commonSink;
        readonly ChannelHost _channelHost;
        readonly BufferingChannel _bufferingChannel;
        readonly EventDispatcher _dispatcher;
        DateTime _nextDeadClientGarbage;

        static GrandOutput _default;
        static readonly object _defaultLock = new object();


        /// <summary>
        /// Gets the default <see cref="GrandOutput"/> for the current Application Domain.
        /// Note that <see cref="EnsureActiveDefault"/> must have been called, otherwise this static property is null.
        /// </summary>
        public static GrandOutput Default 
        { 
            get { return _default; } 
        }

        /// <summary>
        /// Ensures that the <see cref="Default"/> GrandOutput is created and that any <see cref="ActivityMonitor"/> that will be created in this
        /// application domain will automatically have a <see cref="GrandOutpuClient"/> registered for this Default GrandOutput.
        /// </summary>
        /// <returns>The Default GrandOutput.</returns>
        /// <remarks>
        /// This method is thread-safe (a simple lock protects it) and uses a <see cref="ActivityMonitor.AutoConfiguration"/> action 
        /// that <see cref="Register"/>s newly created <see cref="ActivityMonitor"/>.
        /// </remarks>
        static public GrandOutput EnsureActiveDefault()
        {
            lock( _defaultLock )
            {
                SystemActivityMonitor.EnsureStaticInitialization();
                _default = new GrandOutput();
                ActivityMonitor.AutoConfiguration += m => Default.Register( m );
            }
            return _default;
        }

        /// <summary>
        /// Initializes a new <see cref="GrandOutput"/>. 
        /// </summary>
        public GrandOutput()
        {
            _clients = new List<WeakRef<GrandOutputClient>>();
            _dispatcher = new EventDispatcher();
            _commonSink = new GrandOutputCompositeSink();
            var factory = new ChannelFactory( _commonSink, _dispatcher );
            _channelHost = new ChannelHost( factory, OnConfigurationReady );
            _channelHost.ConfigurationClosing += OnConfigurationClosing;
            _bufferingChannel = new BufferingChannel( _commonSink, _dispatcher, factory.CommonSinkOnlyReceiver );
            _nextDeadClientGarbage = DateTime.UtcNow.AddMinutes( 5 );
        }

        /// <summary>
        /// Ensures that a client for this GrandOutput is registered on a monitor.
        /// </summary>
        /// <param name="monitor">The monitor onto which a <see cref="GrandOutputClient"/> must be registered.</param>
        /// <returns>A newly created client or the already existing one.</returns>
        public GrandOutputClient Register( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            AttemptGarbageDeadClients();
            Func<GrandOutputClient> reg = () =>
                {
                    var c = new GrandOutputClient( this );
                    lock( _clients ) _clients.Add( new WeakRef<GrandOutputClient>( c ) ); 
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
            AttemptGarbageDeadClients();
            _commonSink.Add( sink );
        }

        /// <summary>
        /// Unregisters a <see cref="IGrandOutputSink"/>.
        /// </summary>
        /// <param name="sink">The sink to unregister.</param>
        public void UnregisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            AttemptGarbageDeadClients();
            _commonSink.Remove( sink );
        }

        /// <summary>
        /// Attempts to set a new configuration.
        /// </summary>
        /// <param name="config">The configuration that must be set.</param>
        /// <param name="monitor">Optional monitor.</param>
        /// <param name="millisecondsBeforeForceClose">Optional timeout to wait before forcing the close of the currently active configuration.</param>
        /// <returns>True on succees.</returns>
        public bool SetConfiguration( GrandOutputConfiguration config, IActivityMonitor monitor = null, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( config == null ) throw new ArgumentNullException( "config" );
            if( monitor == null ) monitor = new SystemActivityMonitor();
            using( monitor.OpenInfo().Send( this == Default ? "Default GrandOutput configuration." : "GrandOutput configuration." ) )
            {
                if( _channelHost.SetConfiguration( monitor, config.RouteConfiguration, millisecondsBeforeForceClose ) )
                {
                    if( this == _default && config.AppDomainDefaultFilter.HasValue ) ActivityMonitor.DefaultFilter = config.AppDomainDefaultFilter.Value;
                    monitor.CloseGroup( "Success." );
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Obtains an actual channel based on the activity <see cref="IActivityMonitor.Topic"/> (null when .
        /// This is called on the monitor's thread.
        /// </summary>
        /// <param name="topic">The topic. Used as the key to find an actual Channel that must handle the log events.</param>
        /// <returns>A channel for the topic (or an internal BufferingChannel if a configuration is being applied) or null if the GrandOutput has been disposed.</returns>
        internal IChannel ObtainChannel( string topic )
        {
            var channel = _channelHost.ObtainRoute( topic );
            if( channel == null )
            {
                lock( _bufferingChannel.FlushLock )
                {
                    channel = _channelHost.ObtainRoute( topic );
                    if( channel == null && !_channelHost.IsDisposed )
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
            if( e.IsDisposed )
            {
                _dispatcher.Dispose(); 
            }
            else
            {
                lock( _bufferingChannel.FlushLock ) _bufferingChannel.EnsureActive();
            }
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
                _bufferingChannel.FlushBuffer( e.IsEmptyConfiguration ? (Func<string,IChannel>)null : e.ObtainRoute );
                e.ApplyConfiguration();
            }
            // The new configuration is applied: we signal the clients
            // and use this configuration thread to clean the weak refs list if needed.
            if( SignalConfigurationChanged() )
            {
                int nbDeadClients;
                lock( _clients ) nbDeadClients = DoGarbageDeadClients( DateTime.UtcNow );
                if( nbDeadClients > 0 ) e.Monitor.Info( "Removing {0} dead client(s).", nbDeadClients );
                else e.Monitor.Trace( "No dead client to remove." );
            }
        }

        /// <summary>
        /// Signals the clients referenced by weak refs that they need to obtain a new channel
        /// and returns true if at least one weak ref is not alive.
        /// </summary>
        bool SignalConfigurationChanged()
        {
            WeakRef<GrandOutputClient>[] current;
            lock( _clients ) current = _clients.ToArray();
            bool hasDeadClients = false;
            foreach( var cw in current )
            {
                GrandOutputClient c = cw.Target;
                if( c != null ) c.OnChannelConfigurationChanged();
                else hasDeadClients = true;
            }
            return hasDeadClients;
        }

        void AttemptGarbageDeadClients()
        {
            DateTime t = DateTime.UtcNow;
            if( t > _nextDeadClientGarbage ) DoGarbageDeadClients( t );
        }

        int DoGarbageDeadClients( DateTime utcNow )
        {
            Debug.Assert( Monitor.IsEntered( _clients ) );
            _nextDeadClientGarbage = utcNow.AddMinutes( 5 );
            int count = 0;
            for( int i = 0; i < _clients.Count; ++i )
            {
                if( !_clients[i].IsAlive )
                {
                    _clients.RemoveAt( i-- );
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// Gets whether this GrandOutput has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _channelHost.IsDisposed; }
        }

        /// <summary>
        /// Closes this <see cref="GrandOutput"/>.
        /// </summary>
        /// <param name="monitor">Monitor that will be used. Must not be null.</param>
        /// <param name="millisecondsBeforeForceClose">Maximal time to wait for current routes to be unlocked (see <see cref="IRouteConfigurationLock"/>).</param>
        public void Dispose( IActivityMonitor monitor, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            if( !_channelHost.IsDisposed )
            {
                _channelHost.Dispose( monitor, millisecondsBeforeForceClose );
            }
        }

        /// <summary>
        /// Calls <see cref="Dispose(IActivityMonitor,int)"/> with a <see cref="SystemActivityMonitor"/> and no closing time limit.
        /// </summary>
        public void Dispose()
        {
            Dispose( new SystemActivityMonitor(), Timeout.Infinite );
        }
    }
}
