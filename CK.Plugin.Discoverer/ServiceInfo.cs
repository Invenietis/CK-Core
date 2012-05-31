#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\ServiceInfo.cs) is part of CiviKey. 
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
using System.Text;
using CK.Core;
using System.Linq;
using System.Diagnostics;

namespace CK.Plugin.Discoverer
{
    internal sealed class ServiceInfo : DiscoveredInfo, IServiceInfo, IComparable<ServiceInfo>
    {
        string _assemblyQualifiedName;
        PluginAssemblyInfo _assembly;
        string _serviceFullName;
        bool _isDynamicService;

        List<PluginInfo> _implCollection;
        IReadOnlyList<IPluginInfo> _impl;

        IReadOnlyList<ISimplePropertyInfo> _propertiesInfoCollectionEx;
        IReadOnlyList<ISimpleMethodInfo> _methodsInfoCollectionEx;
        IReadOnlyList<ISimpleEventInfo> _eventsInfoCollectionEx;

        IList<SimplePropertyInfo> _propertiesInfoCollection;
        IList<SimpleEventInfo> _eventsInfoCollection;
        IList<SimpleMethodInfo> _methodsInfoCollection;

        /// <summary>
        /// Gets a collection containing infos about the properties exposed by the service
        /// </summary>
        IReadOnlyCollection<ISimplePropertyInfo> IServiceInfo.PropertiesInfoCollection
        {
            get { return _propertiesInfoCollectionEx; }
        }
        /// <summary>
        /// Gets a collection containing infos about the methods exposed by the service
        /// </summary>
        IReadOnlyCollection<ISimpleMethodInfo> IServiceInfo.MethodsInfoCollection
        {
            get { return _methodsInfoCollectionEx; }
        }
        /// <summary>
        /// Gets a collection containing infos about the events exposed by the service
        /// </summary>
        IReadOnlyCollection<ISimpleEventInfo> IServiceInfo.EventsInfoCollection
        {
            get { return _eventsInfoCollectionEx; }
        }

        public string AssemblyQualifiedName
        {
            get { return _assemblyQualifiedName; }
        }

        public string ServiceFullName { get { return _serviceFullName; } }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assembly; }
        }

        public IReadOnlyList<IPluginInfo> Implementations
        {
            get { return _impl; }
        }

        public bool IsDynamicService
        {
            get { return _isDynamicService; }
        }

        internal ServiceInfo( PluginDiscoverer discoverer )
            : base( discoverer )
        {
        }

        internal void Initialize( PluginDiscoverer.Merger merger, Runner.ServiceInfo r )
        {
            _assemblyQualifiedName = r.AssemblyQualifiedName;
            _serviceFullName = r.ServiceFullName;
            _isDynamicService = r.IsDynamicService;

            Debug.Assert( !_isDynamicService || (r.HasError || r.AssemblyInfo != null), "If we are a DynamicService, we must have an assembly or be in error." );
            if( r.AssemblyInfo != null ) _assembly = merger.FindOrCreate( r.AssemblyInfo );

            base.Initialize( r );

            _implCollection = new List<PluginInfo>();
            foreach( Runner.PluginInfo plugin in r.Implementations )
                _implCollection.Add( merger.FindOrCreate( plugin ) );

            _impl = new ReadOnlyListOnIList<PluginInfo>( _implCollection );

            _propertiesInfoCollection = new List<SimplePropertyInfo>();
            foreach( Runner.SimplePropertyInfo rP in r.PropertiesInfoCollection )
            {
                SimplePropertyInfo p = new SimplePropertyInfo();
                p.Initialize( rP );
                _propertiesInfoCollection.Add( p );
            }
            _propertiesInfoCollectionEx = new ReadOnlyListOnIList<SimplePropertyInfo>( _propertiesInfoCollection );

            _methodsInfoCollection = new List<SimpleMethodInfo>();
            foreach( Runner.SimpleMethodInfo rM in r.MethodsInfoCollection )
            {
                SimpleMethodInfo m = new SimpleMethodInfo();
                m.Initialize( rM );
                _methodsInfoCollection.Add( m );
            }

            _methodsInfoCollectionEx = new ReadOnlyListOnIList<SimpleMethodInfo>( _methodsInfoCollection );

            _eventsInfoCollection = new List<SimpleEventInfo>();
            foreach( Runner.SimpleEventInfo rE in r.EventsInfoCollection )
            {
                SimpleEventInfo e = new SimpleEventInfo();
                e.Initialize( rE );
                _eventsInfoCollection.Add( e );
            }

            _eventsInfoCollectionEx = new ReadOnlyListOnIList<SimpleEventInfo>( _eventsInfoCollection );
        }

        internal bool Merge( PluginDiscoverer.Merger merger, Runner.ServiceInfo r )
        {
            bool hasChanged = false;

            if( _assemblyQualifiedName != r.AssemblyQualifiedName )
            {
                _assemblyQualifiedName = r.AssemblyQualifiedName;
                hasChanged = true;
            }
            if( _serviceFullName != r.ServiceFullName )
            {
                _serviceFullName = r.ServiceFullName;
                hasChanged = true;
            }
            if( _isDynamicService != r.IsDynamicService )
            {
                _isDynamicService = r.IsDynamicService;
                hasChanged = true;
            }

            PluginAssemblyInfo newAI = r.AssemblyInfo != null ? merger.FindOrCreate( r.AssemblyInfo ) : null;
            if( _assembly != newAI )
            {
                _assembly = newAI;
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _implCollection, r.Implementations, merger.FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _methodsInfoCollection, r.MethodsInfoCollection, FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _eventsInfoCollection, r.EventsInfoCollection, FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _propertiesInfoCollection, r.PropertiesInfoCollection, FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            return Merge( r, hasChanged );
        }

        /// <summary>
        /// Called by the GenericMergeList method
        /// </summary>
        SimpleMethodInfo FindOrCreate( Runner.SimpleMethodInfo fromRunner )
        {
            SimpleMethodInfo foundM = null;
            foreach( SimpleMethodInfo m in _methodsInfoCollection )
            {
                if( m.GetSimpleSignature() == fromRunner.GetSimpleSignature() )
                {
                    foundM = m;
                }
            }

            if( foundM != null )
            {
                // Updates the parameter names.
                foundM.Merge( fromRunner );
            }
            else
            {
                foundM = new SimpleMethodInfo();
                foundM.Initialize( fromRunner );
            }
            return foundM;
        }

        /// <summary>
        ///  Called by the GenericMergeList method
        /// </summary>
        /// <param name="rP"></param>
        /// <returns></returns>
        SimplePropertyInfo FindOrCreate( Runner.SimplePropertyInfo rP )
        {
            SimplePropertyInfo foundProp = null;
            foreach( SimplePropertyInfo p in this._propertiesInfoCollection )
            {
                if( p.Name == rP.Name )
                {
                    foundProp = p;
                }
            }

            if( foundProp != null )
            {
                foundProp.Merge( rP );
            }
            else
            {
                foundProp = new SimplePropertyInfo();
                foundProp.Initialize( rP );
            }
            return foundProp;
        }

        /// <summary>
        ///  Called by the GenericMergeList method
        /// </summary>
        /// <param name="rE"></param>
        /// <returns></returns>
        SimpleEventInfo FindOrCreate( Runner.SimpleEventInfo rE )
        {
            SimpleEventInfo foundE = null;
            foreach( SimpleEventInfo p in this._eventsInfoCollection )
            {
                if( p.Name == rE.Name )
                {
                    foundE = p;
                }
            }

            if( foundE != null )
            {
                foundE.Merge( rE );
            }
            else
            {
                foundE = new SimpleEventInfo();
                foundE.Initialize( rE );
            }
            return foundE;
        }

        int IComparable<ServiceInfo>.CompareTo( ServiceInfo other )
        {
            return CompareTo( other );
        }

        public int CompareTo( IServiceInfo other )
        {
            if( this == other ) return 0;
            return _assemblyQualifiedName.CompareTo( other.AssemblyQualifiedName );
        }

    }
}
