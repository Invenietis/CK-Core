#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Plugin\PluginHost.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Diagnostics;
using CK.Core;
using Common.Logging;

namespace CK.Plugin.Hosting
{
    public class PluginHost : IPluginHost
    {
        static ILog _log = LogManager.GetLogger( typeof( PluginHost ) );
        readonly ServiceHost _serviceHost;
        readonly Dictionary<IPluginInfo, PluginProxy> _plugins;
        readonly Dictionary<Guid, PluginProxy> _loadedPlugins;
        readonly IReadOnlyCollection<IPluginProxy> _loadedPluginsEx;
        readonly List<PluginProxy> _newlyLoadedPlugins;

        public PluginHost()
            : this( CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
        }

        internal PluginHost( CatchExceptionGeneration catchMode )
        {
            _plugins = new Dictionary<IPluginInfo, PluginProxy>();
            _loadedPlugins = new Dictionary<Guid, PluginProxy>();
            _loadedPluginsEx = new ReadOnlyCollectionOnICollection<PluginProxy>( _loadedPlugins.Values );
            _serviceHost = new ServiceHost( catchMode );
            _newlyLoadedPlugins = new List<PluginProxy>();
        }

        /// <summary>
        /// Gets or sets a function that is in charge of obtaining concrete plugin instances.
        /// Only the default constructor of the plugin must be called by this action.
        /// </summary>
        public Func<IPluginInfo, IPlugin> PluginCreator { get; set; }

        /// <summary>
        /// Gets or sets a function called before the setup of each plugins to fill their edition properties.
        /// </summary>
        public Action<IPluginProxy> PluginConfigurator { get; set; }

        /// <summary>
        /// Gets or sets a function called after plugins that must stop or be disabled have actually been stopped or disabled
        /// and before start of (potentially newly loaded) plugins.
        /// </summary>
        public Action<IReadOnlyCollection<IPluginProxy>> ServiceReferencesBinder { get; set; }

        public IPluginProxy FindLoadedPlugin( Guid pluginId, bool checkCurrentlyLoading )
        {
            var p = _loadedPlugins.GetValueWithDefault( pluginId, null );
            if( p == null && checkCurrentlyLoading ) p = _newlyLoadedPlugins.FirstOrDefault( n => n.PluginKey.UniqueId == pluginId );
            return p;
        }

        /// <summary>
        /// Gets the loaded plugins. This contains also the plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        public IReadOnlyCollection<IPluginProxy> LoadedPlugins { get { return _loadedPluginsEx; } }

        /// <summary>
        /// Used for white tests only.
        /// </summary>
        internal PluginProxy FindPluginProxy( IPluginInfo key )
        {
            PluginProxy result;
            _plugins.TryGetValue( key, out result );
            return result;
        }

        public bool IsPluginRunning( IPluginInfo key )
        {
            PluginProxy result;
            if( !_plugins.TryGetValue( key, out result ) ) return false;
            return result.Status == RunningStatus.Started;
        }

        /// <summary>
        /// Attempts to execute a plan.
        /// </summary>
        /// <param name="disabledPluginKeys">Plugins that must be disabled.</param>
        /// <param name="stoppedPluginKeys">Plugins that must be stopped.</param>
        /// <param name="runningPluginKeys">Plugins that must be running.</param>
        /// <returns>A <see cref="IExecutionPlanError"/> that details the error if any.</returns>
        public IExecutionPlanResult Execute( IEnumerable<IPluginInfo> disabledPluginKeys, IEnumerable<IPluginInfo> stoppedPluginKeys, IEnumerable<IPluginInfo> runningPluginKeys )
        {
            if( PluginCreator == null ) throw new InvalidOperationException( R.PluginCreatorIsNull );
            if( ServiceReferencesBinder == null ) throw new InvalidOperationException( R.PluginConfiguratorIsNull );

            int nbIntersect;
            nbIntersect = disabledPluginKeys.Intersect( stoppedPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.DisabledAndStoppedPluginsIntersect, nbIntersect );
            nbIntersect = disabledPluginKeys.Intersect( runningPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.DisabledAndRunningPluginsIntersect, nbIntersect );
            nbIntersect = stoppedPluginKeys.Intersect( runningPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.StoppedAndRunningPluginsIntersect, nbIntersect );

            List<PluginProxy> toDisable = new List<PluginProxy>();
            List<PluginProxy> toStop = new List<PluginProxy>();
            List<PluginProxy> toStart = new List<PluginProxy>();

            foreach( IPluginInfo k in disabledPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status != RunningStatus.Disabled )
                {
                    toDisable.Add( p );
                    if( p.Status != RunningStatus.Stopped )
                    {
                        toStop.Add( p );
                    }
                }
            }
            foreach( IPluginInfo k in stoppedPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status != RunningStatus.Stopped )
                {
                    toStop.Add( p );
                }
            }
            // The lists toDisable and toStop are correctly filled.
            // A plugin can be in both lists if it must be stopped and then disabled.

            // Now, we attempt to activate the plugins that must run: if an error occurs,
            // we leave and return the error since we did not change anything.
            foreach( IPluginInfo k in runningPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( !p.IsLoaded )
                {
                    if( !p.TryLoad( _serviceHost, PluginCreator ) )
                    {
                        Debug.Assert( p.LoadError != null );
                        _serviceHost.LogMethodError( PluginCreator.Method, p.LoadError );
                        // Unable to load the plugin: leave now.
                        return new ExecutionPlanResult() { Culprit = p.PluginKey, Status = ExecutionPlanResultStatus.LoadError };
                    }
                    Debug.Assert( p.LoadError == null );
                    Debug.Assert( p.Status == RunningStatus.Disabled );
                    _newlyLoadedPlugins.Add( p );
                }
                if( p.Status != RunningStatus.Started )
                {
                    toStart.Add( p );
                }
            }
            // The toStart list is ready: plugins inside are loaded without error.

            // We stop all "toStop" plugin.
            // Their "stop" methods will be called.
            foreach( PluginProxy p in toStop )
            {
                if( p.Status > RunningStatus.Stopped )
                {
                    try
                    {
                        SetPluginStatus( p, RunningStatus.Stopping );
                        p.RealPlugin.Stop();
                        _log.Debug( String.Format( "The {0} plugin has been successfully stopped.", p.PublicName ) );
                    }
                    catch( Exception ex )
                    {
#if DEBUG
                    //Helps the developper identify the culprit of exception
                    Debugger.Break();
#endif 
                        _log.ErrorFormat( "There has been a problem when stopping the {0} plugin.", ex, p.PublicName );
                        _serviceHost.LogMethodError( p.GetImplMethodInfoStop(), ex );
                    }
                }
            }

            // We un-initialize all "toStop" plugin.
            // Their "Teardown" methods will be called.
            // After that, they are all "stopped".
            foreach( PluginProxy p in toStop )
            {
                try
                {
                    if( p.Status > RunningStatus.Stopped )
                    {
                        SetPluginStatus( p, RunningStatus.Stopped );
                        p.RealPlugin.Teardown();
                        _log.Debug( String.Format( "The {0} plugin has been successfully torn down.", p.PublicName ) );
                    }
                }
                catch( Exception ex )
                {
#if DEBUG
                    //Helps the developper identify the culprit of exceptions
                    Debugger.Break();
#endif
                    _log.ErrorFormat( "There has been a problem when tearing down the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), ex );
                }
            }
            Debug.Assert( toStop.All( p => p.Status <= RunningStatus.Stopped ) );


            // Prepares the plugins to start so that they become the implementation
            // of their Service and are at least stopped (instead of disabled).
            foreach( PluginProxy p in toStart )
            {
                ServiceProxyBase service = p.Service;
                // The call to service.SetImplementation, sets the implementation and takes
                // the _status of the service into account: this status is at most Stopped
                // since we necessarily stopped the previous implementation (if any) above.
                if( service != null )
                {
                    Debug.Assert( service.Status <= RunningStatus.Stopped );
                    service.SetPluginImplementation( p );
                }
                // This call will trigger an update of the service status.
                if( p.Status == RunningStatus.Disabled ) SetPluginStatus( p, RunningStatus.Stopped );
            }

            // Now that services have been associated to their new implementation (in Stopped status), we
            // can disable the plugins that must be disabled.
            foreach( PluginProxy p in toDisable )
            {
                SetPluginStatus( p, RunningStatus.Disabled );
                try
                {
                    p.DisposeIfDisposable();
                }
                catch( Exception ex )
                {
#if DEBUG
                    //Helps the developper identify the culprit of exceptions
                    Debugger.Break();
#endif
                    _log.ErrorFormat( "There has been a problem when disposing the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoDispose(), ex );
                }
            }

            // Before starting 
            for( int i = 0; i < toStart.Count; i++ )
            {
                PluginProxy p = toStart[i];
                // We configure plugin's edition properties.
                if( PluginConfigurator != null ) PluginConfigurator( p );

                SetPluginStatus( p, RunningStatus.Starting );
                IPluginSetupInfo info = new IPluginSetupInfo();
                try
                {
                    p.RealPlugin.Setup( info );
                    info.Clear();
                    _log.Debug( String.Format( "The {0} plugin has been successfully set up.", p.PublicName ) );
                }
                catch( Exception ex )
                {
#if DEBUG
                    //Helps the developper identify the culprit of exceptions
                    Debugger.Break();
#endif
                    _log.ErrorFormat( "There has been a problem when setting up the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoSetup(), ex );

                    // Revoking the call to Setup for all plugins that haven't been started yet.
                    //Will pass the plugin to states : Stopping and then Stopped
                    for( int j = 0; j <= i; j++ )
                    {
                        RevokeSetupCall( toStart[j] );
                    }

                    info.Error = ex;
                    return new ExecutionPlanResult() { Culprit = p.PluginKey, Status = ExecutionPlanResultStatus.SetupError, SetupInfo = info };
                }
            }

            // Since we are now ready to start new plugins, it is now time to make the external world
            // aware of the existence of any new plugins and configure them to run.
            foreach( PluginProxy p in _newlyLoadedPlugins )
            {
                _loadedPlugins.Add( p.PluginKey.UniqueId, p );
            }
            Debug.Assert( ServiceReferencesBinder != null );
            try
            {
                var listNew = new ReadOnlyCollectionOnICollection<PluginProxy>( _newlyLoadedPlugins );
                //var disabled = new ReadOnlyCollectionAdapter<IPluginProxy, PluginProxy>( toDisable );
                ServiceReferencesBinder( listNew );
            }
            catch( Exception ex )
            {
                _serviceHost.LogMethodError( ServiceReferencesBinder.Method, ex );
            }
            _newlyLoadedPlugins.Clear();

            for( int i = 0; i < toStart.Count; i++ )
            {
                PluginProxy p = toStart[i];
                try
                {
                    SetPluginStatus( p, RunningStatus.Started );
                    p.RealPlugin.Start();
                    _log.Debug( String.Format( "The {0} plugin has been successfully started.", p.PublicName ) );
                }
                catch( Exception ex )
                {
#if DEBUG
                    //Helps the developper identify the culprit of exceptions
                    Debugger.Break();
#endif
                    // Emitted as low level log.
                    _log.ErrorFormat( "There has been a problem when starting the {0} plugin.", ex, p.PublicName );

                    // Emitted as a log event.
                    _serviceHost.LogMethodError( p.GetImplMethodInfoStart(), ex );

                    //All the plugins already started  when the exception was thrown have to be stopped + teardown (including this one in exception)
                    for( int j = 0; j <= i; j++ )
                    {
                        RevokeStartCall( toStart[j] );
                    }

                    // Revoking the call to Setup for all plugins that hadn't been started when the exception occured.
                    for( int j = i + 1; j < toStart.Count; j++ )
                    {
                        RevokeSetupCall( toStart[j] );
                    }

                    return new ExecutionPlanResult() { Culprit = p.PluginKey, Status = ExecutionPlanResultStatus.LoadError, Error = ex };
                }
            }
            return new ExecutionPlanResult();
        }

        private void RevokeStartCall( PluginProxy p )
        {
            try
            {
                p.RealPlugin.Stop();
            }
            catch( Exception exStop )
            {
                // 2.1 - Should be emitted as an external log event.
                _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), exStop );
            }
            RevokeSetupCall( p );
        }

        private void RevokeSetupCall( PluginProxy p )
        {
            // 2 - Stops the plugin status.
            SetPluginStatus( p, RunningStatus.Stopping, true );
            // 3 - Safe call to TearDown.
            try
            {
                p.RealPlugin.Teardown();
            }
            catch( Exception exTeardown )
            {
                // 2.1 - Should be emitted as an external log event.
                _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), exTeardown );
            }
            SetPluginStatus( p, RunningStatus.Stopped, true );
        }

        /// <summary>
        /// Gets or sets the object that sends <see cref="IServiceHost.EventCreating"/> and <see cref="IServiceHost.EventCreated"/>.
        /// </summary>
        public object EventSender
        {
            get { return _serviceHost.EventSender; }
            set { _serviceHost.EventSender = value; }
        }

        PluginProxy EnsureProxy( IPluginInfo key )
        {
            PluginProxy result;
            if( !_plugins.TryGetValue( key, out result ) )
            {
                result = new PluginProxy( key );
                _plugins.Add( key, result );
            }
            return result;
        }

        public event EventHandler<PluginStatusChangedEventArgs> StatusChanged;

        void SetPluginStatus( PluginProxy plugin, RunningStatus newOne )
        {
            SetPluginStatus( plugin, newOne, false );
        }

        void SetPluginStatus( PluginProxy plugin, RunningStatus newOne, bool allowErrorTransition )
        {
            RunningStatus previous = plugin.Status;
            Debug.Assert( previous != newOne );
            if( newOne > previous )
            {
                // New status is greater than the previous one.
                // We first set the plugin (and raise the event) and then raise the service event (if any).
                DoSetPluginStatus( plugin, newOne, previous );
                if( plugin.IsCurrentServiceImplementation )
                {
                    if( newOne == RunningStatus.Stopped && plugin.Service.Status == RunningStatus.Stopped )
                    {
                        // This is an consequence of the fact that we disable plugins after 
                        // starting the new ones.
                        // When pA (stopping) implements sA and pB implements sA (starting), sA remains "Stopped".
                    }
                    else plugin.Service.SetStatusChanged( newOne, allowErrorTransition );
                }
            }
            else
            {
                // New status is lower than the previous one.
                // We first raise the service event (if any) and then the plugin event.
                if( plugin.IsCurrentServiceImplementation ) plugin.Service.SetStatusChanged( newOne, allowErrorTransition );
                DoSetPluginStatus( plugin, newOne, previous );
            }
        }

        private void DoSetPluginStatus( PluginProxy plugin, RunningStatus newOne, RunningStatus previous )
        {
            plugin.Status = newOne;
            var h = StatusChanged;
            if( h != null )
            {
                h( this, new PluginStatusChangedEventArgs( previous, plugin ) );
            }
        }

        public IServiceHost ServiceHost
        {
            get { return _serviceHost; }
        }

        public ILogCenter LogCenter
        {
            get { return _serviceHost; }
        }


    }
}
