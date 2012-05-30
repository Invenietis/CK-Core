#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\PluginConfigAccessorInfo.cs) is part of CiviKey. 
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
using System.Reflection;
using CK.Core;
using System.Collections.Generic;

namespace CK.Plugin.Discoverer.Runner
{
    [Serializable]
    public sealed class PluginConfigAccessorInfo : DiscoveredInfo, IComparable<PluginConfigAccessorInfo>
    {
        Guid _source;
        PluginInfo _plugin;

        internal PluginConfigAccessorInfo( string errorMessage )
        {
            AddErrorLine( errorMessage );
        }

        internal PluginConfigAccessorInfo( CustomAttributeData attribute, PluginInfo plugin, bool selfEdited )
        {
            _plugin = plugin;
            if( ErrorMessage == null )
            {
                if( !selfEdited )
                {
                    try
                    {
                        _source = new Guid( (string)attribute.ConstructorArguments[0].Value );
                    }
                    catch( Exception ex )
                    {
                        AddErrorLine( ex.Message );
                    }
                }
                else
                {
                    _source = plugin.PluginId;
                }
            }
        }

        public PluginInfo Plugin { get { return _plugin; } }

        public Guid Source
        {
            get { return _source; }
        }

        public string ConfigurationPropertyName { get; set; }

        public bool IsConfigurationPropertyValid { get { return !HasError && ConfigurationPropertyName != null; } }

        #region IComparable<PluginEditorAssociationInfo> Membres

        public int CompareTo( PluginConfigAccessorInfo other )
        {
            if( this == other ) return 0;
            int cmp = _plugin.CompareTo( other.Plugin );
            if( cmp == 0 ) cmp = _source.CompareTo( other.Source );
            return cmp;
        }

        #endregion
    }
}
