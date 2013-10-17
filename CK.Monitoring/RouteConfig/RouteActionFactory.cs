using System;
using System.Linq;
using System.Collections.Generic;
using CK.Core;
using CK.RouteConfig.Impl;

namespace CK.RouteConfig
{
    /// <summary>
    /// Factory for actual actions from <see cref="ActionConfiguration"/> objects that enables
    /// the <see cref="ConfiguredRouteHost"/> to create new actions and new final routes whenever its configuration changed.
    /// </summary>
    /// <typeparam name="TAction">Actual type of the actions. The only constraint is that it must be a reference type.</typeparam>
    /// <typeparam name="TRoute">Route class that encapsulates actions.</typeparam>
    public abstract class RouteActionFactory<TAction, TRoute>
        where TAction : class
        where TRoute : class
    {
        readonly Dictionary<ActionConfiguration, TAction> _cache;
        RouteConfigurationLockShell _configLock;

        /// <summary>
        /// Initializes a new factory.
        /// </summary>
        protected RouteActionFactory()
        {
            _cache = new Dictionary<ActionConfiguration, TAction>();
        }

        internal void Initialize( RouteConfigurationLockShell configLock )
        {
            _cache.Clear();
            _configLock = configLock;
            DoInitialize();
        }

        internal TAction[] GetAllActionsAndUnitialize( bool success )
        {
            TAction[] all = _cache.Values.ToArray();
            _cache.Clear();
            DoUninitialize( success );
            return all;
        }

        internal TAction Create( IActivityMonitor monitor, ActionConfiguration c )
        {
            TAction a;
            if( !_cache.TryGetValue( c, out a ) )
            {
                if( c is Impl.ActionCompositeConfiguration )
                {
                    var seq = c as ActionSequenceConfiguration;
                    if( seq != null )
                    {
                        a = DoCreateSequence( monitor, _configLock, seq, Create( monitor, seq.Children ) );
                    }
                    else
                    {
                        var par = c as ActionParallelConfiguration;
                        if( par != null )
                        {
                            a = DoCreateParallel( monitor, _configLock, par, Create( monitor, seq.Children ) );
                        }
                        else throw new InvalidOperationException( "Only Sequence or Parallel composites are supported." );
                    }
                }
                else a = DoCreate( monitor, _configLock, c );
                _cache.Add( c, a );
            }
            return a;
        }

        internal TAction[] Create( IActivityMonitor monitor, IReadOnlyList<ActionConfiguration> c )
        {
            TAction[] result = new TAction[c.Count];
            for( int i = 0; i < result.Length; ++i ) result[i] = Create( monitor, c[i] );
            return result;
        }

        /// <summary>
        /// Must be implemented to return an empty final route.
        /// This empty final route is used when no configuration exists or if an error occured while 
        /// setting a new configuration.
        /// </summary>
        /// <returns>An empty route. Can be a static shared (immutable) object.</returns>
        internal protected abstract TRoute DoCreateEmptyFinalRoute();

        /// <summary>
        /// Must be implemented to initialize any required shared objects for building new actions and routes.
        /// This is called once prior to any call to other methods of this factory.
        /// Default implementation does nothing.
        /// </summary>
        protected virtual void DoInitialize()
        {
        }

        /// <summary>
        /// Must be implemented to create a <typeparamref name="TAction"/> from a <see cref="ActionConfiguration"/> object
        /// that is guaranteed to not be a composite (a parallel or a sequence).
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="configLock">
        /// Configuration lock. It must not be sollicited during the creation of the action: an action that delay
        /// its work can keep a reference to it and use it when needed.
        /// </param>
        /// <param name="c">Configuration of the action.</param>
        /// <returns>The created action.</returns>
        protected abstract TAction DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c );

        /// <summary>
        /// Must me implemented to create a parallel action.
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="configLock">
        /// Configuration lock. It must not be sollicited during the creation of the parallel: if the parallel delays
        /// its work, it can keep a reference to it and use it as needed.
        /// </param>
        /// <param name="c">Configuration of the parallel action.</param>
        /// <param name="children">Array of already created children action.</param>
        /// <returns>A parallel action.</returns>
        protected abstract TAction DoCreateParallel( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionParallelConfiguration c, TAction[] children );

        /// <summary>
        /// Must me implemented to create a sequence action.
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="configLock">
        /// Configuration lock. It must not be sollicited during the creation of the sequence: a sequence that delays
        /// its work can keep a reference to it and use it as needed.
        /// </param>
        /// <param name="c">Configuration of the sequence action.</param>
        /// <param name="children">Array of already created children action.</param>
        /// <returns>A sequence action.</returns>
        protected abstract TAction DoCreateSequence( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionSequenceConfiguration c, TAction[] children );

        /// <summary>
        /// Must be implemented to create the final route class that encapsulates the array of actions of a route. 
        /// </summary>
        /// <param name="monitor">Monitor to use if needed to comment route creation.</param>
        /// <param name="configLock">
        /// Configuration lock. It must not be sollicited during the creation of the route: a route that delays
        /// its work can keep a reference to it and use it as needed.
        /// </param>
        /// <param name="actions">Array of actions for the route.</param>
        /// <param name="configurationName">The <see cref="RouteConfiguration"/> name.</param>
        /// <param name="configData">Configuration data of the route.</param>
        /// <param name="routePath">Path to this route: parent route objects are already created.</param>
        /// <returns>Final route actions encapsulation.</returns>
        internal protected abstract TRoute DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, TAction[] actions, string configurationName, object configData, IReadOnlyList<TRoute> routePath );

        /// <summary>
        /// Must be implemented to cleanup any resources (if any) once new actions and routes have been created.
        /// This is always called (even if an error occcured). 
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="success">True on success, false if creation of routes has failed.</param>
        protected virtual void DoUninitialize( bool success )
        {
        }

    }

}
