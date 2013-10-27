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
        readonly object _waitLock = new Object();

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

        /// <summary>
        /// Initializes a new <see cref="ConfiguredRouteHost"/> initially <see cref="IsClosed">closed</see>.
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
        /// This obtention locks the configuration: it must be unlocked when not used anymore.
        /// When null, it means that a configuration is waiting to be applied: a route that buffers its work should be substituded.
        /// </summary>
        /// <param name="route">The full route that will be matched.</param>
        /// <returns>The final route to apply or null if a configuration is applying.</returns>
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
        /// Gets the total number of calls to <see cref="SetConfiguration"/>.
        /// </summary>
        public int ConfigurationAttemptCount
        {
            get { return _configurationAttemptCount; }
        }

        /// <summary>
        /// Gets the total number of succesful calls to <see cref="ApplyPendingConfiguration"/>.
        /// </summary>
        public int SuccessfulConfigurationCount
        {
            get { return _succesfulConfigurationCount; }
        }

        /// <summary>
        /// Gets whether this host is closed.
        /// When closed, all routes are empty.
        /// </summary>
        public bool IsClosed
        {
            get { return _root == _emptyHost; }
        }

        /// <summary>
        /// Event argument raised by <see cref="ConfiguredRouteHost{TAction,TRoute}.ConfigurationClosing"/>.
        /// </summary>
        public class ConfigurationClosingEventArgs : EventArgs
        {
            /// <summary>
            /// The configuration waiting to be applied.
            /// </summary>
            public readonly RouteConfigurationResult NewConfiguration;

            /// <summary>
            /// The <see cref="IActivityMonitor"/> that monitors the change of the configuration.
            /// </summary>
            public readonly IActivityMonitor Monitor;

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
            /// Gets whether the new route is the empty one.
            /// </summary>
            public bool IsClosed
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
            /// If this method is not called during the call back, it is automaticcaly called before leaving <see cref="ConfiguredRouteHost{TAction,TRoute}.SetConfiguration">SetConfiguration</see>.
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
        /// <item>Waiting for previous routes termination (unititialization).</item>
        /// <item>Calling ConfigurationReady callback: new routes must be initialized.</item>
        /// <item>If the new configuration has not been applied by the ConfigurationReady callback, it is automaticaly applied.</item>
        /// </list>
        /// </summary>
        /// <param name="monitor">Monitor that wil receive explanations and errors.</param>
        /// <param name="configuration">The configuration to achieve.</param>
        /// <returns>True if the new configuration has been succesfully applied, false if an error occured.</returns>
        public bool SetConfiguration( IActivityMonitor monitor, RouteConfiguration configuration, int millisecondsBeforeForceClose = Timeout.Infinite )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            if( configuration == null ) throw new ArgumentNullException( "configuration" );
            lock( _initializeLock )
            {
                using( monitor.OpenInfo().Send( "New route configuration initialization." ) )
                {
                    Interlocked.Increment( ref _configurationAttemptCount );
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
                        if( !_configLock.Wait( millisecondsBeforeForceClose ) ) monitor.Warn( "Timeout expired. Force the termination." );
                    }
                    if( !CloseCurrentRoutesAndStartNewOnes( monitor ) ) return false;
                    // The new routes are ready.
                    Debug.Assert( _configLock.CurrentCount == 1 );
                    shellLock.Open();
                    ConfigurationReady ready = new ConfigurationReady( monitor, this );
                    if( _readyCallback != null )
                    {
                        using( monitor.OpenInfo().Send( "Calling ConfigurationReady callback." ) )
                        {
                            _readyCallback( ready );
                        }
                    }
                    ready.ApplyConfiguration();
                }
            }
            return true;
        }

        void DoApplyConfiguration()
        {
            if( _futureRoot != null )
            {
                _allActions = _futureAllActions;
                _root = _futureRoot;
                _futureAllActions = null;
                _futureRoot = null;
                lock( _waitLock ) Monitor.PulseAll( _waitLock );
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
                        try
                        {
                            _starter( monitor, d );
                        }
                        catch( Exception ex )
                        {
                            if( failed == null ) failed = new List<int>();
                            failed.Add( i );
                            monitor.Fatal().Send( ex );
                        }
                    }
                }
            }
            if( failed == null )
            {
                Interlocked.Increment( ref _succesfulConfigurationCount );
                _configLock.Reset( 1 );
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
            lock( _waitLock )
            {
                while( _futureAllActions != null )
                    if( !Monitor.Wait( _waitLock, millisecondsTimeout ) ) return false;
            }
            return true;
        }

        /// <summary>
        /// Closes the current configuration regardless of any potential work of the current actions. 
        /// Current actions are closed and any route is associated to an empty set of actions.
        /// </summary>
        /// <param name="monitor">Monitor that will be used. Must not be null.</param>
        public void DirectClose( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException();
            lock( _initializeLock )
            {
                if( _root != _emptyHost )
                {
                    Debug.Assert( _allActions != null );
                    _root = _emptyHost;
                    _allActions = Util.EmptyArray<TAction>.Empty;
                    _futureAllActions = null;
                    _futureRoot = null;
                    CloseActions( monitor, _allActions );
                    lock( _waitLock ) Monitor.PulseAll( _waitLock );
                }
            }
        }

        /// <summary>
        /// Calls <see cref="DirectClose"/>.
        /// </summary>
        public void Dispose()
        {
            DirectClose( new ActivityMonitor( false ) );
            _configLock.Dispose();
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
