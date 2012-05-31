#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\TestContext.cs) is part of CiviKey. 
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

using System.Reflection;
using CK.Core;
using CK.Plugin.Hosting;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CK.Plugin.Host.Tests
{

    class PluginInfoMock : IPluginInfo
    {
        public Guid PluginId { get; set; }

        public string PublicName { get; set; }

        public string Description { get; set; }

        public bool IsOldVersion { get; set; }

        public Uri RefUrl { get; set; }

        public IReadOnlyList<string> Categories { get; set; }

        public Uri IconUri { get; set; }

        public string PluginFullName { get; set; }

        public IReadOnlyList<IPluginConfigAccessorInfo> EditorsInfo { get; set; }

        public IReadOnlyList<IPluginConfigAccessorInfo> EditableBy { get; set; }

        public IAssemblyInfo AssemblyInfo { get; set; }

        public IReadOnlyList<IServiceReferenceInfo> ServiceReferences { get; set; }

        public IServiceInfo Service { get; set; }

        public bool HasError { get; set; }

        public string ErrorMessage { get; set; }

        public Version Version { get; set; }

        public Guid UniqueId { get; set; }

        public int CompareTo( IPluginInfo other )
        {
            throw new NotImplementedException();
        }

    }

    class TestContext
    {
        public static readonly IPluginInfo PluginPluginId;
        //public static readonly INamedVersionedUniqueId PluginPluginId;
        public static readonly IPluginInfo PluginConsumerId;

        static Dictionary<Guid,IUniqueId> _plugins;
        
        static TestContext()
        {
            _plugins = new Dictionary<Guid, IUniqueId>();

            PluginPluginId = new PluginInfoMock() { UniqueId = Guid.NewGuid(), Version = new Version( 1, 1, 0, 0 ), PublicName = "Plugin" };
            PluginConsumerId = new PluginInfoMock() { UniqueId = Guid.NewGuid(), Version = new Version( 2, 0, 0, 0 ), PublicName = "ConsumerPlugin" };

            _plugins.Add( PluginPluginId.UniqueId, PluginPluginId );
            _plugins.Add( PluginConsumerId.UniqueId, PluginConsumerId );
        }

        public TestContext( bool handMadeProxy, bool startService )
            : this( handMadeProxy, startService, CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
        }

        public TestContext( bool handMadeProxy, bool startPlugin, CatchExceptionGeneration catchExceptionsForRealProxy )
        {
            PluginHost = new PluginHost( catchExceptionsForRealProxy );
            RealPlugin = new ChoucroutePlugin();
            ConsumerPlugin = new ConsumerPlugin( this );

            PluginHost.PluginCreator = key => key == PluginPluginId ? (IPlugin)RealPlugin : ConsumerPlugin;
            PluginHost.ServiceReferencesBinder = newPluginsLoaded => { };
            
            if( handMadeProxy )
            {
                ServiceHost.SetManualProxy( typeof(IChoucrouteService), IChoucrouteServiceHandMadeProxy.CreateProxy() );
            }

            // The proxy is itself a IChoucrouteService.
            Assert.That( Service is IChoucrouteService );
            Assert.That( Service is ServiceProxyBase );
            // We check the Running attribute.
            Assert.That( Service.Status == RunningStatus.Disabled );

            Assert.That( PluginProxy == null );

            if( startPlugin )
            {
                PluginHost.Execute( ReadOnlyListEmpty<IPluginInfo>.Empty, ReadOnlyListEmpty<IPluginInfo>.Empty, new[] { PluginPluginId } );
                Assert.That( PluginProxy.Status == RunningStatus.Started );
                Assert.That( Service.Status == RunningStatus.Started );
            }
        }

        public ServiceHost ServiceHost { get { return (ServiceHost)PluginHost.ServiceHost; } }
        
        public IServiceHost IServiceHost { get { return PluginHost.ServiceHost; } }

        public PluginHost PluginHost { get; private set; }

        public PluginProxyBase PluginProxy { get { return PluginHost.FindPluginProxy( PluginPluginId ); } }
        
        public ServiceProxyBase ServiceProxyBase { get { return (ServiceProxyBase)Service; } }
        
        public ChoucroutePlugin RealPlugin { get; private set; }

        public IService<IChoucrouteService> Service { get { return (IService<IChoucrouteService>)ServiceHost.EnsureProxy( typeof(IChoucrouteService) ); } }

        public PluginProxyBase ConsumerPluginProxy { get { return PluginHost.FindPluginProxy( PluginConsumerId ); } }
        
        public ConsumerPlugin ConsumerPlugin { get; private set; }

        public void SetLogOptions( string methodName, ServiceLogMethodOptions opt )
        {
            MethodInfo m = typeof( IChoucrouteService ).GetMethod( methodName );
            Assert.That( m, Is.Not.Null );
            SetLogOptions( m, opt );
        }

        public void SetLogOptions( string eventName, ServiceLogEventOptions opt )
        {
            EventInfo e = typeof(IChoucrouteService).GetEvent( eventName );
            Assert.That( e, Is.Not.Null );
            SetLogOptions( e, opt );
        }

        public void SetLogOptions( MethodInfo method, ServiceLogMethodOptions opt )
        {
            MEntry[] _mRefs = ServiceProxyBase.MethodEntries;
            for( int i = 0; i < _mRefs.Length; ++i )
                if( _mRefs[i].Method == method )
                {
                    _mRefs[i].LogOptions = opt;
                    return;
                }
            Assert.Fail( "Unknown Method" );
        }

        public void SetLogOptions( EventInfo ev, ServiceLogEventOptions opt )
        {
            EEntry[] _eRefs = ServiceProxyBase.EventEntries;
            for( int i = 0; i < _eRefs.Length; ++i )
                if( _eRefs[i].Event == ev )
                {
                    _eRefs[i].LogOptions = opt;
                    return;
                }
            Assert.Fail( "Unknown Event" );
        }

        public ServiceLogEventOptions GetEventLogOptions( string eventName )
        {
            EventInfo e = typeof( IChoucrouteService ).GetEvent( eventName );
            Assert.That( e, Is.Not.Null );
            return ServiceProxyBase.EventEntries.First( ee => ee.Event == e ).LogOptions;
        }
        
        public ServiceLogMethodOptions GetMethodLogOptions( string methodName )
        {
            MethodInfo m = typeof( IChoucrouteService ).GetMethod( methodName );
            Assert.That( m, Is.Not.Null );
            return ServiceProxyBase.MethodEntries.First( em => em.Method == m ).LogOptions;
        }

        internal void EnsureStoppedService()
        {
            if( ServiceProxyBase.Status == RunningStatus.Disabled )
            {
                PluginHost.Execute( ReadOnlyListEmpty<IPluginInfo>.Empty, ReadOnlyListEmpty<IPluginInfo>.Empty, new[] { PluginPluginId } );
                PluginHost.Execute( ReadOnlyListEmpty<IPluginInfo>.Empty, new[] { PluginPluginId }, ReadOnlyListEmpty<IPluginInfo>.Empty );
            }
            Assert.That( ServiceProxyBase.Status == RunningStatus.Stopped );
        }
    }
}
