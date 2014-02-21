using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig.Impl;

namespace CK.RouteConfig
{
    /// <summary>
    /// Thread-safe management of hierarchical routes configured (and reconfigured) by a <see cref="RouteConfiguration"/>.
    /// </summary>
    /// <typeparam name="TAction">Actual type of the actions. Can be any reference type.</typeparam>
    /// <typeparam name="TRoute">Route class that encapsulates actions.</typeparam>
    public class ConfiguredRouteHost<TAction,TRoute> : IDisposable
        where TAction : class
        where TRoute : class
    {
        readonly RouteActionFactory<TAction,TRoute> _actionFactory;
        readonly Action<IActivityMonitor,TAction> _starter;
        readonly Action<IActivityMonitor,TAction> _closer;
        readonly Action<ConfigurationReady> _readyCallback;
        readonly object _initializeLock = new Object();
        readonly object _configuredLock = new Object();

        internal class RouteHost
        {
            public readonly TRoute FinalRoute;
            public readonly TAction[] Actions;
            public readonly SubRouteHost[] Routes;

            internal RouteHost( RouteActionFactory<TAction, TRoute> factory )
            {
                Actions = Util.EmptyArray<TAction>.Empty;
                Routes = Util.EmptyArray<SubRouteHost>.Empty;
                FinalRoute = factory.DoCreateEmptyFinalRoute();
            }

            internal RouteHost( IActivityMonitor monitor, List<TRoute> routePath, RouteConfigurationLockShell configLock, RouteActionFactory<TAction, TRoute> factory, RouteConfigurationResolved c )
            {
                using( monitor.OpenInfo().Send( "Initializing compiled route '{0}'.", c.Name ) )
                {
                    try
                    {
                        Actions = c.ActionsResolved.Select( r => factory.Create( monitor, r.ActionConfiguration ) ).ToArray();
                        FinalRoute = factory.DoCreateFinalRoute( monitor, configLock, Actions, c.Name, c.ConfigData, routePath.AsReadOnlyList() );
                        routePath.Add( FinalRoute ); 
                        Routes = c.SubRoutes.Where( r => r.ActionsResolved.Any() ).Select( r => new SubRouteHost( monitor, routePath, configLock, factory, r ) ).ToArray();
                        routePath.RemoveAt( routePath.Count-1 );
                    }
                    catch( Exception ex )
                    {
                        monitor.Fatal().Send( ex );
                        Actions = null;
                    }
                }
            }


            public RouteHost FindRoute( string route )
            {
                foreach( var sub in Routes )
                    if( sub.Filter( route ) ) return sub.FindRoute( route );
                return this;
            }

            public void WalkFinalRoutes( Action<TRoute> walker )
            {
                foreach( var r in Routes )
                {
                    walker( r.FinalRoute );
                    r.WalkFinalRoutes( walker );
                }
            }
        }

        internal class SubRouteHost : RouteHost
        {
            public readonly Func<string,bool> Filter;

            internal SubRouteHost( IActivityMonitor monitor, List<TRoute> routePath, RouteConfigurationLockShell configLock, RouteActionFactory<TAction, TRoute> factory, SubRouteConfigurationResolved c )
                : base( monitor, routePath, configLock, factory, c )
            {
                Filter = c.RoutePredicate;
            }
        }

        readonly RouteHost _emptyHost;

        readonly CountdownEvent _configLock;
        RouteHost _root;
        TAction[] _allActions;
        TAction[] _allActionsDying;

        RouteHost _futureRoot;
        TAction[] _futureAllActions;
        int _configurationAttemptCount;
        int _succesfulConfigurationCount;
        bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="ConfiguredRouteHost{TAction,TRoute}"/> initially <see cref="IsEmpty">empty</see>.
        /// </summary>
        /// <param name="actionFactory">Factory for <typeparamref name="TAction"/> based on an <see cref="ActionConfiguration"/> for final <typeparamref name="TRoute"/>.</param>
        /// <param name="readyCallback">Optional callback that will be called right before applying a new configuration.</param>
        /// <param name="starter">Optional starter function for a <typeparamref name="TAction"/>.</param>
        /// <param name="closer">Optional closer function.</param>
        public ConfiguredRouteHost( RouteActionFactory<TAction, TRoute> actionFactory, Action<ConfigurationReady> readyCallback = null, Action<IActivityMonitor, TAction> starter = null, Action<IActivityMonitor, TAction> closer = null )
        {
            if( actionFactory == null ) throw new ArgumentNullException( "actionFactory" );
            _actionFactory = actionFactory;
            _readyCallback = readyCallback;
            _starter = starter;
            _closer = closer;
            // When initialCount is 0, the event is created in a signaled state.
            // Wait does not wait: this corresponds to the Empty route.
            _configLock = new CountdownEvent( 0 );
            _emptyHost = new RouteHost( actionFactory );
            _root = _emptyHost;
            _allActions = Util.EmptyArray<TAction>.Empty;
        }

        /// <summary>
        /// Returns the <typeparamref name="TRoute"/> for a full name.
        /// Obtaining a route locks the configuration: it must be unlocked when not used anymore.
        /// When null, it means that a configuration is waiting to be applied (a route that buffers its work should be substituted)
        /// or that this host <see cref="IsDisposed"/>.
        /// </summary>
        /// <param name="route">The full route that will be matched.</param>
        /// <returns>The final route to apply or null if a configuration is applying or if <see cref="IsDisposed"/> is true.</returns>
        public TRoute ObtainRoute( string route )
        {
            var r = _root;
            if( r != _emptyHost ) _configLock.AddCount();
            if( _futureRoot != null )
            {
                if( r != _emptyHost ) _configLock.Signal();
                return null;
            }
            return r.FindRoute( route ?? String.Empty ).FinalRoute;
        }

        /// <summary>
        /// Gets the total number of calls to <see cref="SetConfiguration"/> (and to <see cref="Dispose()"/> method).
        /// This can be used to call <see cref="WaitForNextConfiguration"/>.
        /// </summary>
        public int ConfigurationAttemptCount
        {
            get { return _configurationAttemptCount; }
        }

        /// <summary>
        /// Gets the total number of successful calls to <see cref="SetConfiguration"/>.
        /// </summary>
        public int SuccessfulConfigurationCount
        {
            get { return _succesfulConfigurationCount; }
        }

        /// <summary>
        /// Gets whether this host is closed.
        /// When closed, all routes are empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _root == _emptyHost; }
        }

        /// <summary>
        /// Gets whether this host has been disposed.
        /// When disposed, <see cref="ObtainRoute"/> always returns null.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Event argument raised by <see cref="ConfiguredRouteHost{TAction,TRoute}.ConfigurationClosing"/>.
        /// </summary>
        public class ConfigurationClosingEventArgs : EventArgs
        {
            /// <summary>
            /// The configuration waiting to be applied.
            /// Null when the host is disposed.
            /// </summary>
            public readonly RouteConfigurationResult NewConfiguration;

            /// <summary>
            /// The <see cref="IActivityMonitor"/> that monitors the change of the configuration.
            /// </summary>
            public readonly IActivityMonitor Monitor;

            /// <summary>
            /// Gets whether the host is disposed.
            /// </summary>
            public bool IsDisposed { get { return NewConfiguration == null; } }

            internal ConfigurationClosingEventArgs( IActivityMonitor m, RouteConfigurationResult c )
            {
                Monitor = m;
                NewConfiguration = c;
            }
        }

        /// <summary>
        /// Argument of the callback called when the configuration is ready to be applied.
        /// </summary>
        public class ConfigurationReady
        {
            readonly ConfiguredRouteHost<TAction,TRoute> _host;

            /// <summary>
            /// The <see cref="IActivityMonitor"/> that monitors the change of the configuration.
            /// </summary>
            public readonly IActivityMonitor Monitor;

            internal ConfigurationReady( IActivityMonitor m, ConfiguredRouteHost<TAction, TRoute> host )
            {
                Monitor = m;
                _host = host;
            }

            /// <summary>
            /// Gets whether the new configuration is empty.
            /// </summary>
            public bool IsEmptyConfiguration
            {
                get { return _host._futureRoot == _host._emptyHost; }
            }

            /// <summary>
            /// Returns the <typeparamref name="TRoute"/> for a full name based on the new configuration.
            /// This obtention locks the configuration: it must be unlocked when not used anymore.
            /// </summary>
            /// <param name="route">The full route that will be matched.</param>
            /// <returns>The final route to use.</returns>
            public TRoute ObtainRoute( string route )
            {
                if( _host._futureRoot != _host._emptyHost ) _host._configLock.AddCount();
                return _host._futureRoot.FindRoute( route ?? String.Empty ).FinalRoute;
            }

            /// <summary>
            /// Gets all future routes. This must be used before calling <see cref="ApplyConfiguration"/> otherwise 
            /// an <see cref="InvalidOperationException"/> is thrown.
            /// Ordering corresponds to a depth-first traversal.
            /// </summary>
            public List<TRoute> GetAllRoutes()
            {
                if( _host._futureRoot == null ) throw new InvalidOperationException();
                List<TRoute> allRoutes = new List<TRoute>();
                allRoutes.Add( _host._futureRoot.FinalRoute );
                _host._futureRoot.WalkFinalRoutes( allRoutes.Add );
                return allRoutes;
            }

            /// <summary>
            /// Applies pending configuration: new routes are set on the host, <see cref="ConfiguredRouteHost{TAction,TRoute}.ObtainRoute"/> now returns the new ones.
            /// If this method is not called during the call back, it is automatically called before leaving <see cref="ConfiguredRouteHost{TAction,TRoute}.SetConfiguration">SetConfiguration</see>.
            /// </summary>
            public void ApplyConfiguration()
            {
                _host.DoApplyConfiguration();
            }

        }

        /// <summary>
        /// Event raised by <see cref="SetConfiguration"/> when current configured routes must be released.
        /// </summary>
        public event EventHandler<ConfigurationClosingEventArgs> ConfigurationClosing;

        /// <summary>
        /// Sets a new <see cref="RouteConfiguration"/>.
        /// <list type="number">
        /// <item>If the new routes can not be created, false is returned and current configuration remains active.</item>
        /// <item><see cref="ObtainRoute"/> starts returning null and <see cref="ConfigurationClosing"/> event is raised.</item>
        /// <item>Waiting for previous routes termination (uninitialization).</item>
        /// <item>Initializing the new routes (starter function when provided to the constructor is called for each actions).</item>
        /// <item>Calling ConfigurationReady callback: new routes can be initialized.</item>
        /// <item>If the new configuration has not been applied by the ConfigurationReady callback, it is automatically applied.</item>
        /// </list>
        /// If an error occurs while starting the new routes, false is returned and routes are empty.
        /// </summary>
        /// <param name="monitor">Monitor that will receive explanations and errors.</param>
        /// <param name="configuration">The configuration to achieve.</param>
        /// <param name="millisecondsBeforeForceClose">Maximal time to wait for current routes to be unlocked (see <see cref="IRouteConfigurationLock"/>).</param>
        /// <returns>True if the new configuration has been successfully applied, false if an error occurred.</returns>
        public bool SetConfiguration( IActivityMonitor monitor, RouteConfiguration configuration, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            if( configuration == null ) throw new ArgumentNullException( "configuration" );
            lock( _initializeLock )
            {
                Interlocked.Increment( ref _configurationAttemptCount );
                Monitor.PulseAll( _initializeLock );
                if( _disposed ) throw new ObjectDisposedException( "ConfiguredRouteHost" );
                using( monitor.OpenInfo().Send( "New route configuration initialization." ) )
                {
                    RouteConfigurationResult result = configuration.Resolve( monitor );
                    if( result == null ) return false;
                    RouteConfigurationLockShell shellLock = new RouteConfigurationLockShell( _configLock ); 
                    RouteHost newRoot;
                    if( !CreateNewRoutes( monitor, result, shellLock, out newRoot ) ) return false;
                    // From here, ObtainRoute will start to return null.
                    _futureRoot = newRoot;
                    _allActionsDying = _allActions;
                    _futureAllActions = _actionFactory.GetAllActionsAndUnitialize( true );
                    // Let outside world know that current routes must be released.
                    var h1 = ConfigurationClosing;
                    if( h1 != null )
                    {
                        using( monitor.OpenInfo().Send( "Raising ConfigurationClosing event." ) )
                        {
                            h1( this, new ConfigurationClosingEventArgs( monitor, result ) );
                        }
                    }
                    using( monitor.OpenInfo().Send( "Waiting for current routes to terminate." ) )
                    {
                        if( _root != _emptyHost ) _configLock.Signal();
                        if( !_configLock.Wait( millisecondsBeforeForceClose ) ) monitor.Warn().Send( "Timeout expired. Force the termination." );
                    }
                    bool success = CloseCurrentRoutesAndStartNewOnes( monitor );
                    // The new routes are ready.
                    Debug.Assert( (_futureRoot == _emptyHost && _configLock.CurrentCount == 0) || (_futureRoot != _emptyHost && _configLock.CurrentCount == 1) );
                    if( _futureRoot != _emptyHost ) shellLock.Open();
                    ConfigurationReady ready = new ConfigurationReady( monitor, this );
                    if( _readyCallback != null )
                    {
                        using( monitor.OpenInfo().Send( "Calling ConfigurationReady callback." ) )
                        {
                            _readyCallback( ready );
                        }
                    }
                    ready.ApplyConfiguration();
                    return success;
                }
            }
        }

        void DoApplyConfiguration()
        {
            if( _futureRoot != null )
            {
                _allActions = _futureAllActions;
                _root = _futureRoot;
                _futureAllActions = null;
                _futureRoot = null;
                lock( _configuredLock ) Monitor.PulseAll( _configuredLock );
            }
        }

        bool CreateNewRoutes( IActivityMonitor monitor, RouteConfigurationResult result, RouteConfigurationLockShell shellLock, out RouteHost newRoot )
        {
            _actionFactory.Initialize( shellLock );
            newRoot = null;
            using( monitor.OpenTrace().Send( "Routes creation." ) )
            {
                try
                {
                    newRoot = new RouteHost( monitor, new List<TRoute>(), shellLock, _actionFactory, result.Root );
                }
                catch( Exception ex )
                {
                    monitor.Error().Send( ex );
                }
            }
            if( newRoot == null || newRoot.Actions == null )
            {
                _actionFactory.GetAllActionsAndUnitialize( false );
                return false;
            }
            if( newRoot.Actions.Length == 0 && newRoot.Routes.Length == 0 )
            {
                newRoot = _emptyHost;
                monitor.Info().Send( "New route configuration is empty." );
            }
            return true;
        }

        bool CloseCurrentRoutesAndStartNewOnes( IActivityMonitor monitor )
        {
            if( _allActionsDying != null )
            {
                CloseActions( monitor, _allActionsDying );
                _allActionsDying = null;
            }
            List<int> failed = null;
            if( _starter != null && _futureAllActions.Length > 0 )
            {
                using( monitor.OpenInfo().Send( "Starting {0} new action(s).", _futureAllActions.Length ) )
                {
                    int i = 0;
                    foreach( var d in _futureAllActions )
                    {
                        int errorCount = 0;
                        using( monitor.CatchCounter( nbError => errorCount = nbError ) )
                        {
                            try
                            {
                                _starter( monitor, d );
                            }
                            catch( Exception ex )
                            {
                                monitor.Fatal().Send( ex );
                            }
                        }
                        if( errorCount > 0 )
                        {
                            if( failed == null ) failed = new List<int>();
                            failed.Add( i );
                        }
                    }
                    monitor.CloseGroup( failed != null ? "Failed to start new actions." : "Actions successfully started." );
                }
            }
            if( failed == null )
            {
                Interlocked.Increment( ref _succesfulConfigurationCount );
                if( _futureRoot != _emptyHost ) _configLock.Reset( 1 );
                return true;
            }
            if( _closer != null && _futureAllActions.Length > failed.Count )
            {
                using( monitor.OpenInfo().Send( "Closing {0} started action(s) due to {1} failure(s).", _futureAllActions.Length - failed.Count, failed.Count ) )
                {
                    for( int i = 0; i < _futureAllActions.Length; ++i )
                    {
                        if( !failed.Contains( i ) )
                        {
                            try
                            {
                                _closer( monitor, _futureAllActions[i] );
                            }
                            catch( Exception ex )
                            {
                                monitor.Warn().Send( ex );
                            }
                        }
                    }
                }
            }
            _configLock.Reset( 0 );
            _futureRoot = _emptyHost;
            _futureAllActions = Util.EmptyArray<TAction>.Empty;
            return false;
        }

        /// <summary>
        /// Blocks the caller if a new configuration is waiting to be applied until it has effectively be applied.
        /// </summary>
        /// <param name="millisecondsTimeout">Maximum number of milliseconds to wait.</param>
        /// <returns>False if specified timeout expired.</returns>
        public bool WaitForAppliedPendingConfiguration( int millisecondsTimeout = Timeout.Infinite )
        {
            lock( _configuredLock )
            {
                while( _futureAllActions != null )
                    if( !Monitor.Wait( _configuredLock, millisecondsTimeout ) ) return false;
            }
            return true;
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
            lock( _initializeLock )
            {
                if( _disposed ) return true;
                while( configurationAttemptCount > _configurationAttemptCount )
                    if( !Monitor.Wait( _initializeLock, millisecondsTimeout ) ) return false;
            }
            // We can miss here a reconfiguration but we do not care.
            return WaitForAppliedPendingConfiguration( millisecondsTimeout );
        }

        /// <summary>
        /// Closes the current configuration. 
        /// Current actions are closed. <see cref="IsDisposed"/> is set to true.
        /// </summary>
        /// <param name="monitor">Monitor that will be used. Must not be null.</param>
        /// <param name="millisecondsBeforeForceClose">Maximal time to wait for current routes to be unlocked (see <see cref="IRouteConfigurationLock"/>).</param>
        /// <returns>Returns true if this host has actually been disposed, false if it has already been disposed.</returns>
        public bool Dispose( IActivityMonitor monitor, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            lock( _initializeLock )
            {
                Interlocked.Increment( ref _configurationAttemptCount );
                Monitor.PulseAll( _initializeLock );
                if( _disposed ) return false;
                _disposed = true;
                _futureRoot = _emptyHost;
                _allActionsDying = _allActions;
                _futureAllActions = _emptyHost.Actions;
                // Let outside world know that current routes must be released.
                var h1 = ConfigurationClosing;
                if( h1 != null )
                {
                    using( monitor.OpenInfo().Send( "Raising last ConfigurationClosing event." ) )
                    {
                        h1( this, new ConfigurationClosingEventArgs( monitor, null ) );
                    }
                }
                using( monitor.OpenInfo().Send( "Waiting for current routes to terminate." ) )
                {
                    if( _root != _emptyHost ) _configLock.Signal();
                    if( !_configLock.Wait( millisecondsBeforeForceClose ) ) monitor.Warn().Send( "Timeout expired. Force the termination." );
                }
                if( _allActionsDying != null )
                {
                    CloseActions( monitor, _allActionsDying );
                    _allActionsDying = null;
                }
                _configLock.Dispose();
                _root = _emptyHost;
                // When disposed, _futureRoot is set to _emptyHost so that ObtainRoute returns null.
                _futureRoot = _emptyHost;
                _futureAllActions = null;
                lock( _configuredLock ) Monitor.PulseAll( _configuredLock );
            }
            return true;
        }

        /// <summary>
        /// Calls <see cref="Dispose(IActivityMonitor,int)"/> with a <see cref="SystemActivityMonitor"/> and no closing time limit.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
        }

        /// <summary>
        /// Calls <see cref="Dispose(IActivityMonitor,int)"/> with a <see cref="SystemActivityMonitor"/> and no closing time limit.
        /// </summary>
        /// <param name="disposing">True when called from code (managed and unmanaged resources must be disposed), false when called from the Garbage collector (only unmanaged resources should be closed in such case).</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                Dispose( new SystemActivityMonitor( false, null ), Timeout.Infinite );
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Standard Dispose/Finalizer pattern (calls <see cref="Dispose(bool)">Dispose(false)</see>. 
        /// Here to support release of unmanaged resources by specialization.
        /// </summary>
        ~ConfiguredRouteHost()
        {
            Dispose( false );
        }

        void CloseActions( IActivityMonitor monitor, TAction[] toClose )
        {
            Debug.Assert( toClose != null );
            if( _closer != null )
            {
                using( monitor.OpenInfo().Send( "Closing {0} previous action(s).", toClose.Length ) )
                {
                    foreach( var d in toClose )
                    {
                        try
                        {
                            _closer( monitor, d );
                        }
                        catch( Exception ex )
                        {
                            monitor.Warn().Send( ex );
                        }
                    }
                }
            }
        }

    }

}
