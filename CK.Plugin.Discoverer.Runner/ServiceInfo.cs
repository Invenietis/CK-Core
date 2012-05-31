#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\ServiceInfo.cs) is part of CiviKey. 
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CK.Core;

namespace CK.Plugin.Discoverer.Runner
{
    [Serializable]
    public sealed class ServiceInfo : DiscoveredInfo, IComparable<ServiceInfo>
	{
        string _assemblyQualifiedName;
        readonly string _serviceFullName;
        PluginAssemblyInfo _assembly;
        List<PluginInfo> _impl;
        bool _isDynamicService;

        List<SimpleEventInfo> _eventsInfoCollection;
        List<SimpleMethodInfo> _methodsInfoCollection;
        List<SimplePropertyInfo> _propertiesInfoCollection;

        internal ServiceInfo( PluginAssemblyInfo a, Type t )
        {
            _assembly = a;
            _assemblyQualifiedName = t.AssemblyQualifiedName;
            _serviceFullName = t.FullName;
            _isDynamicService = PluginDiscoverer.IsIDynamicService( t );
            _impl = new List<PluginInfo>();
            _eventsInfoCollection = new List<SimpleEventInfo>();
            _methodsInfoCollection = new List<SimpleMethodInfo>();
            _propertiesInfoCollection = new List<SimplePropertyInfo>();
        }

        public string AssemblyQualifiedName 
        { 
            get { return _assemblyQualifiedName; } 
            internal set { _assemblyQualifiedName = value; }
        }

        public string ServiceFullName { get { return _serviceFullName; } }

        public PluginAssemblyInfo AssemblyInfo
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        public List<PluginInfo> Implementations
        {
            get { return _impl; }
        }

        public bool IsDynamicService
        {
            get { return _isDynamicService; }
        }

        public IList<SimpleEventInfo> EventsInfoCollection { get { return _eventsInfoCollection; } }
        public IList<SimpleMethodInfo> MethodsInfoCollection { get { return _methodsInfoCollection; } }
        public IList<SimplePropertyInfo> PropertiesInfoCollection { get { return _propertiesInfoCollection; } }

        internal void NormalizeCollections()
        {
            _impl.Sort();

            _eventsInfoCollection.Sort();
            _methodsInfoCollection.Sort();
            _propertiesInfoCollection.Sort();
        }

        #region IComparable<ServiceInfo> Membres

        public int CompareTo( ServiceInfo other )
        {
            if( this == other ) return 0;
            return _assemblyQualifiedName.CompareTo( other.AssemblyQualifiedName );
        }

        #endregion
    }
}
