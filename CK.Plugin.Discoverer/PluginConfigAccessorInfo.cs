#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\PluginConfigAccessorInfo.cs) is part of CiviKey. 
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
using System.Linq;
using System.Diagnostics;

namespace CK.Plugin.Discoverer
{
    internal sealed class PluginConfigAccessorInfo : DiscoveredInfo, IPluginConfigAccessorInfo, IComparable<PluginConfigAccessorInfo>
    {
        Guid _source;
        string _configPropertyName;
        bool _isConfigurationPropertyValid;
        PluginInfo _plugin;
        IPluginInfo _sourcePlugin;

        public IPluginInfo Plugin
        {
            get { return _plugin; }
        }

        public Guid Source
        {
            get { return _source; }
        }

        public IPluginInfo EditedSource
        {
            get { return _sourcePlugin; }
        }

        public string ConfigurationPropertyName
        {
            get { return _configPropertyName; }
        }

        public bool IsConfigurationPropertyValid
        {
            get { return _isConfigurationPropertyValid && _sourcePlugin != null; }
        }

        internal PluginConfigAccessorInfo( PluginDiscoverer discoverer )
            : base( discoverer )
        {
        }

        internal void Initialize( PluginDiscoverer.Merger merger, Runner.PluginConfigAccessorInfo r )
        {
            base.Initialize( r );
            _source = r.Source;
            _plugin = merger.FindOrCreate( r.Plugin );
            _configPropertyName = r.ConfigurationPropertyName;
            _isConfigurationPropertyValid = r.IsConfigurationPropertyValid;
        }

        internal void BindEditedPlugin( PluginDiscoverer discoverer )
        {
            _sourcePlugin = discoverer.FindPlugin( _source );
        }

        internal bool Merge( PluginDiscoverer.Merger merger, Runner.PluginConfigAccessorInfo r )
        {
            bool hasChanged = false;

            if( _source != r.Source )
            {
                _source = r.Source;
                hasChanged = true;
            }
            if( _configPropertyName != r.ConfigurationPropertyName )
            {
                _configPropertyName = r.ConfigurationPropertyName;
                hasChanged = true;
            }

            if ( _isConfigurationPropertyValid != r.IsConfigurationPropertyValid )
            {
                _isConfigurationPropertyValid = r.IsConfigurationPropertyValid;
                hasChanged = true;
            }

            PluginInfo p = merger.FindOrCreate( r.Plugin ); 
            if ( _plugin != p)
            {
                _plugin = p;
                hasChanged = true;
            }

            return Merge( r, hasChanged );
        }

        #region IComparable<PluginEditorInfo> Membres

        public int CompareTo( PluginConfigAccessorInfo other )
        {
            if( this == other ) return 0;
            int cmp = _plugin.CompareTo( other._plugin );
            if( cmp == 0 ) cmp = _source.CompareTo( other.Source );
            return cmp;
        }

        #endregion
    }
}
