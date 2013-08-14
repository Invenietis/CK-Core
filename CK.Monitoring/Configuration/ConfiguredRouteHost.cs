using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

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
        readonly object _initializeLock = new Object();
        readonly object _waitLock = new Object();

        class RouteHost
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

            internal RouteHost( IActivityMonitor monitor, RouteActionFactory<TAction,TRoute> factory, RouteConfigurationResolved c )
            {
                using( monitor.OpenGroup( LogLevel.Info, "Initializing compiled route '{0}'.", c.Name ) )
                {
                    try
                    {
                        Actions = c.ActionsResolved.Select( r => factory.Create( monitor, r.ActionConfiguration ) ).ToArray();
                        FinalRoute = factory.DoCreateFinalRoute( monitor, Actions, c.Name );
                        Routes = c.SubRoutes.Where( r => r.ActionsResolved.Any() ).Select( r => new SubRouteHost( monitor, factory, r ) ).ToArray();
                    }
                    catch( Exception ex )
                    {
                        monitor.Fatal( ex );
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
        }

        class SubRouteHost : RouteHost
        {
            public readonly Func<string,bool> Filter;

            internal SubRouteHost( IActivityMonitor monitor, RouteActionFactory<TAction,TRoute> factory, SubRouteConfigurationResolved c )
                : base( monitor, factory, c )
            {
                Filter = c.RoutePredicate;
            }
        }

        readonly RouteHost _emptyHost;

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
        /// <param name="actionFactory">Factory for <typeparamref name="TAction"/> based on an <see cref="ActionConfiguration"/>.</param>
        /// <param name="starter">Optional starter function for a <typeparamref name="TAction"/>.</param>
        /// <param name="closer">Optional closer function.</param>
        public ConfiguredRouteHost( RouteActionFactory<TAction,TRoute> actionFactory, Action<IActivityMonitor, TAction> starter = null, Action<IActivityMonitor, TAction> closer = null )
        {
            _actionFactory = actionFactory;
            _starter = starter;
            _closer = closer;
            _emptyHost = new RouteHost( actionFactory );
            _root = _emptyHost;
            _allActions = Util.EmptyArray<TAction>.Empty;
        }

        /// <summary>
        /// Returns the <typeparamref name="TAction"/> to apply to the route.
        /// When null, it means that <see cref="ApplyPendingConfiguration"/> must be called
        /// to close the previous actions and start the new ones.
        /// </summary>
        /// <param name="route">The full route that will be matched.</param>
        /// <returns>The final route to apply.</returns>
        public TRoute FindRoute( string route )
        {
            var r = _root;
            if( _futureRoot != null ) return null;
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
        /// Sets a new <see cref="RouteConfiguration"/>. Once called, <see cref="FindRoute"/> returns null 
        /// until <see cref="ApplyPendingConfiguration"/> is called.
        /// </summary>
        /// <param name="monitor">Monitor that wil receive explanations and errors.</param>
        /// <param name="configuration">The configuration to achieve.</param>
        /// <returns>True if the new configuration is ready to be applied, false if an error occured while preparing the configuration.</returns>
        public bool SetConfiguration( IActivityMonitor monitor, RouteConfiguration configuration )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            if( configuration == null ) throw new ArgumentNullException( "configuration" );
            lock( _initializeLock )
            {
                using( monitor.OpenGroup( LogLevel.Info, "New route configuration initialization." ) )
                {
                    Interlocked.Increment( ref _configurationAttemptCount );
                    RouteConfigurationResult result = configuration.Resolve( monitor );
                    if( result == null ) return false;
                    _actionFactory.Initialize();
                    RouteHost newRoot = new RouteHost( monitor, _actionFactory, result.Root );
                    if( newRoot.Actions == null )
                    {
                        _actionFactory.GetAllActionsAndUnitialize( false );
                        return false;
                    }
                    if( newRoot.Actions.Length == 0 && newRoot.Routes.Length == 0 )
                    {
                        newRoot = _emptyHost;
                        monitor.Info( "New route configuration is empty." );
                    }
                    _futureRoot = newRoot;
                    _allActionsDying = _allActions;
                    _futureAllActions = _actionFactory.GetAllActionsAndUnitialize( true );
                }
            }
            return true;
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
        /// Must be called each time <see cref="FindRoute"/> returned a null list of actions.
        /// It is up to the caller to ensure that no more work has to be executed by the 
        /// previously configured actions before calling this method.
        /// </summary>
        /// <param name="monitor">Monitor that will be used. Must not be null.</param>
        /// <returns>True on success. False if an error occured during the start of any new action: this host is <see cref="Close"/>d.</returns>
        public bool ApplyPendingConfiguration( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException();
            lock( _initializeLock )
            {
                if( _futureRoot == null ) return true;
                if( _allActionsDying != null )
                {
                    CloseActions( monitor, _allActionsDying );
                    _allActionsDying = null;
                }
                bool newConfigIsValid = true;
                if( _starter != null )
                {
                    using( monitor.OpenGroup( LogLevel.Info, "Starting {0} new actions.", _futureAllActions.Length ) )
                    {
                        foreach( var d in _futureAllActions )
                        {
                            try
                            {
                                _starter( monitor, d );
                            }
                            catch( Exception ex )
                            {
                                newConfigIsValid = false;
                                monitor.Fatal( ex );
                            }
                        }
                    }
                }
                if( newConfigIsValid ) Interlocked.Increment( ref _succesfulConfigurationCount );
                else 
                {
                    _futureRoot = _emptyHost;
                    _futureAllActions = Util.EmptyArray<TAction>.Empty;
                }
                _allActions = _futureAllActions;
                _root = _futureRoot;
                _futureAllActions = null;
                _futureRoot = null;
                lock( _waitLock ) Monitor.PulseAll( _waitLock );
                return newConfigIsValid;
            }
        }

        /// <summary>
        /// Closes the current configuration. 
        /// Current actions are closed and any route is associated to an empty set of actions.
        /// </summary>
        /// <param name="monitor">Monitor that will be used. Must not be null.</param>
        public void Close( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException();
            lock( _initializeLock )
            {
                if( _root != _emptyHost )
                {
                    Debug.Assert( _allActions != null );
                    CloseActions( monitor, _allActions );
                    _root = _emptyHost;
                    _allActions = Util.EmptyArray<TAction>.Empty;
                    _futureAllActions = null;
                    _futureRoot = null;
                    lock( _waitLock ) Monitor.PulseAll( _waitLock );
                }
            }
        }

        /// <summary>
        /// Calls <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close( new ActivityMonitor( false ) );
        }

        void CloseActions( IActivityMonitor monitor, TAction[] toClose )
        {
            Debug.Assert( toClose != null );
            if( _closer != null )
            {
                using( monitor.OpenGroup( LogLevel.Info, "Closing {0} previous actions.", toClose.Length ) )
                {
                    foreach( var d in toClose )
                    {
                        try
                        {
                            _closer( monitor, d );
                        }
                        catch( Exception ex )
                        {
                            monitor.Warn( ex );
                        }
                    }
                }
            }
        }

    }

}
