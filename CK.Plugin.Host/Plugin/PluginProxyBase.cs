#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Plugin\PluginProxyBase.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using System.Reflection;

namespace CK.Plugin.Hosting
{
    class PluginProxyBase
    {
        IPlugin _instance;
        Exception  _loadError;

        internal PluginProxyBase()
        {
            Status = RunningStatus.Disabled;
        }

        public RunningStatus Status { get; set; }

        /// <summary>
        /// Gets the implemented service.
        /// </summary>
        internal ServiceProxyBase Service { get; private set; }

        internal bool IsCurrentServiceImplementation
        {
            get { return Service != null && Service.Implementation == this; }
        }

        public bool IsLoaded { get { return _instance != null; } }

        public Exception LoadError 
        { 
            get 
            {
                Debug.Assert( _instance != null || Status == RunningStatus.Disabled, "_instance == null ==> Status == Disabled" );
                return _loadError; 
            } 
        }

        public object RealPluginObject { get { return RealPlugin; } }

        internal MethodInfo GetImplMethodInfoSetup() { return GetImplMethodInfo( typeof( IPlugin ), "Setup" ); }

        internal MethodInfo GetImplMethodInfoStart() { return GetImplMethodInfo( typeof( IPlugin ), "Start" ); }

        internal MethodInfo GetImplMethodInfoStop() { return GetImplMethodInfo( typeof( IPlugin ), "Stop" ); }

        internal MethodInfo GetImplMethodInfoTeardown() { return GetImplMethodInfo( typeof( IPlugin ), "Teardown" ); }
        
        internal MethodInfo GetImplMethodInfoDispose() { return GetImplMethodInfo( typeof( IDisposable ), "Dispose" ); }

        internal MethodInfo GetImplMethodInfo( Type interfaceType, string methodName ) 
        {
            Debug.Assert( RealPlugin != null );
            MethodInfo m = RealPlugin.GetType().GetMethod( interfaceType.FullName + '.' + methodName );
            if( m == null ) m = RealPlugin.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
            return m;
        }

        internal IPlugin RealPlugin 
        { 
            get 
            { 
                Debug.Assert( _instance != null || Status == RunningStatus.Disabled, "_instance == null ==> Status == Disabled" ); 
                return _instance; 
            } 
        }

        /// <summary>
        /// Supports <see cref="IDisposable"/> implementation. If the real plugin does not implement it, nothing is done and 
        /// the current reference instance is kept (it will be reused).
        /// If IDisposable is implemented, a call to Dispose may throw an exception (it should be handled above), but the _instance 
        /// reference is set to null: a new object will always have to be created if the plugin needs to be started again.
        /// </summary>
        internal void DisposeIfDisposable()
        {
            Debug.Assert( Status == RunningStatus.Disabled, "Status has already been set to Disabled." );
            if( _instance != null )
            {
                IDisposable di = _instance as IDisposable;
                if( di != null )
                {
                    _instance = null;
                    di.Dispose();
                }
            }
        }

        internal bool TryLoad( ServiceHost serviceHost, Func<IPlugin> pluginCreator, object pluginKey )
        {
            if( _instance == null )
            {
                try
                {
                    _instance = pluginCreator();
                    if( _instance == null )
                    {
                        _loadError = new CKException( R.PluginCreatorReturnedNull, pluginKey );
                        return false;
                    }
                    Type t = _instance.GetType();
                    if( typeof( IDynamicService ).IsAssignableFrom( t ) )
                    {
                        Type iType = t.GetInterfaces().FirstOrDefault( i => i != typeof( IDynamicService ) && typeof( IDynamicService ).IsAssignableFrom( i ) );
                        if( iType != null )
                        {
                            Service = serviceHost.EnsureProxy( iType );
                        }
                    }
                }
                catch( Exception ex )
                {
                    _loadError = ex;
                    return false;
                }
            }
            return true;
        }


    }
}
