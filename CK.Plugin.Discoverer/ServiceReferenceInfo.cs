#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\ServiceReferenceInfo.cs) is part of CiviKey. 
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
using System.Reflection;

namespace CK.Plugin.Discoverer
{
    internal sealed class ServiceReferenceInfo : DiscoveredInfo, IServiceReferenceInfo, IComparable<ServiceReferenceInfo>
    {
        string _propertyName;
        RunningRequirement	_requirements;
        ServiceInfo _reference;
        PluginInfo _owner;

        public IPluginInfo Owner { get { return _owner; } }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public RunningRequirement Requirements
        {
            get { return _requirements; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        internal ServiceReferenceInfo( PluginDiscoverer discoverer )
            : base( discoverer )
        {
        }

        internal void Initialize( PluginDiscoverer.Merger merger, Runner.ServiceReferenceInfo r )
        {
            base.Initialize( r );
            _propertyName = r.PropertyName;
            _requirements = r.Requirements;
            _reference = merger.FindOrCreate( r.Reference );
            _owner = merger.FindOrCreate( r.Owner );
        }

        internal bool Merge( PluginDiscoverer.Merger merger, Runner.ServiceReferenceInfo r )
        {
            bool hasChanged = false;
            
            if( _propertyName != r.PropertyName )
            {
                _propertyName = r.PropertyName;
                hasChanged = true;
            }
            if( _requirements != r.Requirements )
            {
                _requirements = r.Requirements;
                hasChanged = true;
            }

            ServiceInfo newService = merger.FindOrCreate( r.Reference );
            if( _reference != newService )
            {
                _reference = newService;
                hasChanged = true;
            }

            PluginInfo newPlugin = merger.FindOrCreate( r.Owner );
            if( _owner != newPlugin )
            {
                _owner = newPlugin;
                hasChanged = true;
            }

            return Merge( r, hasChanged );
        }

        #region IComparable<ServiceReferenceInfo> Membres

        public int CompareTo( ServiceReferenceInfo other )
        {
            if( this == other ) return 0;
            int cmp = _owner.CompareTo( other._owner );
            if( cmp == 0 ) cmp = _propertyName.CompareTo( other.PropertyName );
            return cmp;
        }

        #endregion
    }
}
