using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// It is usually useless to explicitly create an instance of GrandOutput: the <see cref="Default"/> one is 
    /// available as soon as <see cref="EnsureActiveDefault"/> is called and will be automatically used by new <see cref="ActivityMonitor"/>.
    /// </summary>
    public sealed partial class GrandOutput : IDisposable
    {
        readonly List<WeakRef<GrandOutputClient>> _clients;
        readonly GrandOutputCompositeSink _commonSink;
        readonly ChannelHost _channelHost;
        readonly BufferingChannel _bufferingChannel;
        readonly EventDispatcher _dispatcher;
        DateTime _nextDeadClientGarbage;

        static GrandOutput _default;
        static FileSystemWatcher _watcher;
        static readonly object _defaultLock = new object();

        /// <summary>
        /// Gets the default <see cref="GrandOutput"/> for the current Application Domain.
        /// Note that <see cref="EnsureActiveDefault()"/> must have been called, otherwise this static property is null.
        /// </summary>
        public static GrandOutput Default 
        { 
            get { return _default; } 
        }

        /// <summary>
        /// Ensures that the <see cref="Default"/> GrandOutput is created and that any <see cref="ActivityMonitor"/> that will be created in this
        /// application domain will automatically have a <see cref="GrandOutputClient"/> registered for this Default GrandOutput.
        /// Use <see cref="EnsureActiveDefaultWithDefaultSettings"/> to initially configure this default.
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
                if( _default != null )
                {
                    SystemActivityMonitor.EnsureStaticInitialization();
                    _default = new GrandOutput();
                    ActivityMonitor.AutoConfiguration += m => Default.Register( m );
                }
            }
            return _default;
        }

        /// <summary>
        /// Ensures that the <see cref="Default"/> GrandOutput is created (see <see cref="EnsureActiveDefault"/>) and configured with default settings.
        /// The <see cref="SystemActivityMonitor.RootLogPath"/> must be valid and if a GrandOutput.config file exists inside, it is loaded as the configuration
        /// that must be valid (otherwise an exception is thrown).
        /// Once loaded, the file is monitored and any change that occurs to it dynamically triggers a <see cref="SetConfiguration"/> with the new file.
        /// </summary>
        /// <param name="monitor">An optional monitor.</param>
        static public GrandOutput EnsureActiveDefaultWithDefaultSettings( IActivityMonitor monitor = null )
        {
            lock( _defaultLock )
            {
                if( _default == null )
                {
                    if( monitor == null ) monitor = new SystemActivityMonitor( true, "GrandOutput" );
                    using( monitor.OpenInfo().Send( "Attempting Default GrandOutput configuration." ) )
                    {
                        try
                        {
                            SystemActivityMonitor.AssertRootLogPathIsSet();
                            string conventionalConfigPath = SystemActivityMonitor.RootLogPath + "GrandOutput.config";
                            GrandOutputConfiguration def = CreateDefaultConfig();
                            if( File.Exists( conventionalConfigPath ) && !def.LoadFromFile( conventionalConfigPath, monitor ) ) 
                            {
                                throw new CKException( "Unable to load Configuration file: '{0}'.", conventionalConfigPath );
                            }
                            GrandOutput output = new GrandOutput();
                            ActivityMonitor.AutoConfiguration += m => Default.Register( m );
                            if( !output.SetConfiguration( def, monitor ) )
                            {
                                throw new CKException( "Failed to set Configuration." );
                            }
                            StartMonitoring( monitor );
                            _default = output;
                        }
                        catch( Exception ex )
                        {
                            monitor.Fatal().Send( ex );
                            throw;
                        }
                    }
                }
            }
            return _default;
        }

        static GrandOutputConfiguration CreateDefaultConfig()
        {
            GrandOutputConfiguration def = new GrandOutputConfiguration();
            Debug.Assert( def.SourceFilterApplicationMode == SourceFilterApplyMode.None );
            Debug.Assert( def.AppDomainDefaultFilter == null );
            var route = new RouteConfiguration();
            route.ConfigData = new GrandOutputChannelConfigData();
            route.AddAction( new BinaryFileConfiguration( "All" ) { Path = SystemActivityMonitor.RootLogPath + "GrandOutputDefault" } );
            def.ChannelsConfiguration = route;
            return def;
        }

        static void StartMonitoring( IActivityMonitor monitor )
        {
            if( _watcher != null ) _watcher.Dispose();
            _watcher = new FileSystemWatcher();
            _watcher.Path = SystemActivityMonitor.RootLogPath;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Filter = "GrandOutput.config";
            _watcher.Changed += _watcher_Changed;
            _watcher.Error += _watcher_Error;
            _watcher.EnableRaisingEvents = true;
        }

        static void _watcher_Error( object sender, ErrorEventArgs e )
        {
            ActivityMonitor.MonitoringError.Add( e.GetException(), String.Format( "While monitoring GrandOutput.Default configuration file '{0}'.", _watcher.Path ) );
        }

        static void _watcher_Changed( object sender, FileSystemEventArgs e )
        {
            if( _watcher == null ) return;
            string conventionalConfigPath = SystemActivityMonitor.RootLogPath + "GrandOutput.config";
            var monitor = new SystemActivityMonitor( true, "GrandOutput" );
            monitor.MinimalFilter = LogFilter.Debug;
            using( monitor.OpenInfo().Send( "AppDomain '{0}',  file '{1}' changed.", AppDomain.CurrentDomain.FriendlyName, conventionalConfigPath ) )
            {
                var def = CreateDefaultConfig();
                if( File.Exists( conventionalConfigPath ) )
                {
                    def.LoadFromFile( conventionalConfigPath, monitor );
                }
                else monitor.Trace().Send( "File missing: applying catch-all default configuration." );
                if( !_default._channelHost.IsDisposed ) _default.SetConfiguration( def, monitor );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="GrandOutput"/>. 
        /// </summary>
        /// <param name="dispatcherStrategy">Strategy to use to handle the throughput.</param>
        public GrandOutput( IGrandOutputDispatcherStrategy dispatcherStrategy = null )
        {
            _clients = new List<WeakRef<GrandOutputClient>>();
            _dispatcher = new EventDispatcher( dispatcherStrategy ?? new EventDispatcherBasicStrategy() );
            _commonSink = new GrandOutputCompositeSink();
            var factory = new ChannelFactory( _commonSink, _dispatcher );
            _channelHost = new ChannelHost( factory, OnConfigurationReady );
            _channelHost.ConfigurationClosing += OnConfigurationClosing;
            _bufferingChannel = new BufferingChannel( _commonSink, _dispatcher, factory.CommonSinkOnlyReceiver );
            _nextDeadClientGarbage = DateTime.UtcNow.AddMinutes( 5 );
            var h = new EventHandler( OnDomainTermination );
            AppDomain.CurrentDomain.DomainUnload += h;
            AppDomain.CurrentDomain.ProcessExit += h;
        }

        void OnDomainTermination( object sender, EventArgs e )
        {
            var w = _watcher;
            if( w != null )
            {
                _watcher = null;
                w.Dispose();
            }
            Dispose( new SystemActivityMonitor(), 10 );
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
        /// Gets the number of lost events since this <see cref="GrandOutput"/> has been created.
        /// </summary>
        public int LostEventCount 
        { 
            get { return _dispatcher.LostEventCount; } 
        }

        /// <summary>
        /// Maximal queue size that has been used.
        /// </summary>
        public int MaxQueuedCount
        {
            get { return _dispatcher.MaxQueuedCount; }
        }

        public int IgnoredConcurrentCallCount
        {
            get { return _dispatcher.IgnoredConcurrentCallCount; }
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

        ///// <summary>
        ///// Attempts to set a new configuration from a file.
        ///// </summary>
        ///// <param name="configFilePath">The path of the configuration that must be set.</param>
        ///// <param name="monitor">Optional monitor.</param>
        ///// <param name="millisecondsBeforeForceClose">Optional timeout to wait before forcing the close of the currently active configuration.</param>
        ///// <returns>True on success.</returns>
        //public bool SetConfiguration( string configFilePath, IActivityMonitor monitor = null, int millisecondsBeforeForceClose = Timeout.Infinite )
        //{
        //    if( monitor == null ) monitor = new SystemActivityMonitor( true, "GrandOutput" );
        //    using( monitor.OpenTrace().Send( "Loading configuration from file: '{0}'.", configFilePath ) )
        //    {
        //        GrandOutputConfiguration c = new GrandOutputConfiguration();
        //        if( !c.LoadFromFile( configFilePath, monitor ) ) return false;
        //        return SetConfiguration( c, monitor, millisecondsBeforeForceClose );
        //    }
        //}

        /// <summary>
        /// Gets the total number of calls to <see cref="SetConfiguration"/> (and to <see cref="Dispose"/> method).
        /// This can be used to call <see cref="WaitForNextConfiguration"/>.
        /// </summary>
        public int ConfigurationAttemptCount
        {
            get { return _channelHost.ConfigurationAttemptCount; }
        }

        /// <summary>
        /// Attempts to set a new configuration.
        /// </summary>
        /// <param name="config">The configuration that must be set.</param>
        /// <param name="monitor">Optional monitor.</param>
        /// <param name="millisecondsBeforeForceClose">Optional timeout to wait before forcing the close of the currently active configuration.</param>
        /// <returns>True on success.</returns>
        public bool SetConfiguration( GrandOutputConfiguration config, IActivityMonitor monitor = null, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( config == null ) throw new ArgumentNullException( "config" );
            if( monitor == null ) monitor = new SystemActivityMonitor( true, "GrandOutput" );
            using( monitor.OpenInfo().Send( this == Default ? "Applying Default GrandOutput configuration." : "Applying GrandOutput configuration." ) )
            {
                if( _channelHost.SetConfiguration( monitor, config.ChannelsConfiguration ?? new RouteConfiguration(), millisecondsBeforeForceClose ) )
                {
                    if( this == _default &&  config.AppDomainDefaultFilter.HasValue ) ActivityMonitor.DefaultFilter = config.AppDomainDefaultFilter.Value;

                    if( config.SourceFilterApplicationMode == SourceFilterApplyMode.Clear || config.SourceFilterApplicationMode == SourceFilterApplyMode.ClearThenApply )
                    {
                        ActivityMonitor.SourceFilter.Clear();
                    }
                    if( config.SourceFilter != null && (config.SourceFilterApplicationMode == SourceFilterApplyMode.Apply || config.SourceFilterApplicationMode == SourceFilterApplyMode.ClearThenApply) )
                    {
                        foreach( var k in config.SourceFilter ) ActivityMonitor.SourceFilter.SetFileFilter( k.Value, k.Key );
                    }
                    monitor.CloseGroup( "Success." );
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Blocks the caller until the current <see cref="ConfigurationAttemptCount"/> is greater or equal to the given number and the last 
        /// configuration has been applied (or this object is disposed).
        /// </summary>
        /// <param name="configurationAttemptCount">The number of configuration attempt count to wait for.</param>
        /// <param name="millisecondsTimeout">Maximum number of milliseconds to wait. Use <see cref="Timeout.Infinite"/> or -1 for no limit.</param>
        /// <returns>False if specified timeout expired.</returns>
        public bool WaitForNextConfiguration( int configurationAttemptCount, int millisecondsTimeout )
        {
            return _channelHost.WaitForNextConfiguration( configurationAttemptCount, millisecondsTimeout );
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
            #if !net40
            Debug.Assert( Monitor.IsEntered( _clients ) );
            #endif
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
                if( _channelHost.Dispose( monitor, millisecondsBeforeForceClose ) )
                {
                    var h = new EventHandler( OnDomainTermination );
                    AppDomain.CurrentDomain.DomainUnload -= h;
                    AppDomain.CurrentDomain.ProcessExit -= h;
                    _dispatcher.Dispose();
                    _bufferingChannel.Dispose();
                }
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
