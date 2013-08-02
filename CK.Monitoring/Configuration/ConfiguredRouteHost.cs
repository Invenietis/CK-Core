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
    /// <typeparam name="T">Any reference type.</typeparam>
    public class ConfiguredRouteHost<T> : IDisposable
        where T : class
    {
        readonly Func<IActivityMonitor,ActionConfiguration,T> _actionFactory;
        readonly Action<IActivityMonitor,T> _starter;
        readonly Action<IActivityMonitor,T> _closer;
        readonly object _initializeLock = new Object();
        readonly object _waitLock = new Object();

        class RouteHost
        {
            public readonly T[] Actions;
            public readonly SubRouteHost[] Routes;

            public static readonly RouteHost Empty = new RouteHost();

            RouteHost()
            {
                Actions = Util.EmptyArray<T>.Empty;
                Routes = Util.EmptyArray<SubRouteHost>.Empty;
            }

            internal RouteHost( IActivityMonitor monitor, Dictionary<ActionConfiguration, T> created, Func<IActivityMonitor, ActionConfiguration, T> actionFactory, RouteConfigurationResolved c )
            {

                using( monitor.OpenGroup( LogLevel.Info, "Initializing compiled route '{0}'.", c.Name ) )
                {
                    try
                    {
                        Actions = c.ActionsResolved.Select( r => created.GetOrSet( r.ActionConfiguration, a => actionFactory( monitor, a ) ) ).ToArray();
                        Routes = c.SubRoutes.Where( r => r.ActionsResolved.Any() ).Select( r => new SubRouteHost( monitor, created, actionFactory, r ) ).ToArray();
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
            public readonly string Name;
            public readonly Func<string,bool> Filter;

            internal SubRouteHost( IActivityMonitor monitor, Dictionary<ActionConfiguration, T> created, Func<IActivityMonitor, ActionConfiguration, T> actionFactory, SubRouteConfigurationResolved c )
                : base( monitor, created, actionFactory, c )
            {
                Name = c.Name;
                Filter = c.RoutePredicate;
            }

        }

        RouteHost _root;
        T[] _allActions;
        T[] _allActionsDying;

        RouteHost _futureRoot;
        T[] _futureAllActions;
        int _configurationAttemptCount;
        int _succesfulConfigurationCount;

        /// <summary>
        /// Initializes a new <see cref="ConfiguredRouteHost"/>.
        /// </summary>
        /// <param name="actionFactory">
        /// Factory function for <typeparamref name="T"/> based on an <see cref="ActionConfiguration"/>.
        /// This factory may return null if, for any reason, the action can not be created.
        /// </param>
        /// <param name="starter">Optional starter function for a <typeparamref name="T"/>.</param>
        /// <param name="closer">Optional closer function.</param>
        public ConfiguredRouteHost( Func<IActivityMonitor, ActionConfiguration, T> actionFactory, Action<IActivityMonitor, T> starter = null, Action<IActivityMonitor, T> closer = null )
        {
            _actionFactory = actionFactory;
            _starter = starter;
            _closer = closer;
            _root = RouteHost.Empty;
            _allActions = Util.EmptyArray<T>.Empty;
        }

        /// <summary>
        /// Returns the <typeparamref name="T"/> to apply to the route.
        /// When null, it means that <see cref="ApplyPendingConfiguration"/> must be called
        /// to close the previous actions and start the new ones.
        /// </summary>
        /// <param name="route">The full route that will be matched.</param>
        /// <returns>The actions to apply.</returns>
        public IEnumerable<T> FindRoute( string route )
        {
            var r = _root;
            if( _futureRoot != null ) return null;
            return r.FindRoute( route ?? String.Empty ).Actions;
        }

        /// <summary>
        /// Gets the total number of calls to <see cref="SetConfiguration"/>.
        /// </summary>
        public int ConfigurationAttemptCount
        {
            get { return _configurationAttemptCount; }
        }

        /// <summary>
        /// Gets the total number of succesful <see cref="ApplyPendingConfiguration"/>.
        /// </summary>
        public int SuccessfulConfigurationCount
        {
            get { return _succesfulConfigurationCount; }
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
                    Dictionary<ActionConfiguration,T> created = new Dictionary<ActionConfiguration, T>();
                    RouteHost newRoot = new RouteHost( monitor, created, _actionFactory, result.Root );
                    if( newRoot.Actions == null ) return false;
                    if( newRoot.Actions.Length == 0 && newRoot.Routes.Length == 0 )
                    {
                        newRoot = RouteHost.Empty;
                        monitor.Info( "New route configuration is empty." );
                    }
                    _futureRoot = newRoot;
                    _allActionsDying = _allActions;
                    _futureAllActions = created.Values.ToArray();
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
        /// <returns>True on success. False if an error occured during the start of any new action: the previous configuration still applies.</returns>
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
                if( !newConfigIsValid ) return false;
                _allActions = _futureAllActions;
                _root = _futureRoot;
                _futureAllActions = null;
                _futureRoot = null;
                Interlocked.Increment( ref _succesfulConfigurationCount );
                lock( _waitLock ) Monitor.PulseAll( _waitLock );
                return true;
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
                if( _root != RouteHost.Empty )
                {
                    Debug.Assert( _allActions != null );
                    CloseActions( monitor, _allActions );
                    _root = RouteHost.Empty;
                    _allActions = Util.EmptyArray<T>.Empty;
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

        void CloseActions( IActivityMonitor monitor, T[] toClose )
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
