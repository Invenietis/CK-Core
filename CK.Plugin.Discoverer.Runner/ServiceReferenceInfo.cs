#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\ServiceReferenceInfo.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin.Discoverer.Runner
{
    [Serializable]
    public sealed class ServiceReferenceInfo : DiscoveredInfo, IComparable<ServiceReferenceInfo>
    {
        string _propertyName;
        RunningRequirement	_requirements;
        ServiceInfo _reference;
        bool _isIServiceWrapped;

        public PluginInfo Owner { get; internal set; }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public RunningRequirement Requirements
        {
            get { return _requirements; }
        }

        public ServiceInfo Reference
        {
            get { return _reference; }
        }

        public bool IsIServiceWrapped
        {
            get { return _isIServiceWrapped; }
        }

        internal ServiceReferenceInfo( PluginInfo owner, string propertyName, ServiceRefInfo reference, RunningRequirement requires )
        {
            Owner = owner;
            _propertyName = propertyName;
            _reference = reference.Reference;
            _isIServiceWrapped = reference.IsIServiceWrapped;
            _requirements = requires;
        }

        internal ServiceReferenceInfo( PluginInfo owner, string propertyName, ServiceRefInfo reference, IList<CustomAttributeNamedArgument> attrArgs )
            : this( owner, propertyName, reference, RunningRequirement.Optional )
        {
            foreach( CustomAttributeNamedArgument a in attrArgs )
            {
                if( a.MemberInfo.Name == "Requires" ) _requirements = (RunningRequirement)a.TypedValue.Value;
            }
        }

        #region IComparable<ServiceReferenceInfo> Membres

        public int CompareTo( ServiceReferenceInfo other )
        {
            if( this == other ) return 0;
            int cmp = Owner.CompareTo( other.Owner );
            if( cmp == 0 ) cmp = _propertyName.CompareTo( other.PropertyName );
            return cmp;
        }

        #endregion
    }
}
