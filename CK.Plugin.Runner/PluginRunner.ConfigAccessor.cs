#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\PluginRunner.ConfigAccessor.cs) is part of CiviKey. 
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
using CK.Plugin.Discoverer;
using System.Reflection;
using CK.Plugin.Config;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    public partial class PluginRunner : ISimplePluginRunner
    {
        IConfigManager _config;
        Dictionary<INamedVersionedUniqueId,PluginConfigAccessor> _configAccessors;

        public IConfigManager ConfigManager
        {
            get { return _config; }
        }

        PluginConfigAccessor GetConfigAccessor( INamedVersionedUniqueId idEdited )
        {
            Debug.Assert( idEdited != null );
            Debug.Assert( _contextObject != null );
            
            // Switch from whatever INamedVersionedUniqueId is to IPluginProxy... if it is loaded.
            IPluginProxy p = idEdited as IPluginProxy;
            if( p == null )
            {
                p = (IPluginProxy)_host.FindLoadedPlugin( idEdited.UniqueId, true );
                if( p == null )
                {
                    _configAccessors.Remove( idEdited );
                    return null;
                }
            }            
            PluginConfigAccessor result;
            if( !_configAccessors.TryGetValue( p, out result ) )
            {
                result = new PluginConfigAccessor( p, _config.Extended, _contextObject );
                _configAccessors.Add( p, result );
            }
            return result;
        }

        void OnConfigContainerChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.IsAllPluginsConcerned )
            {
                foreach( PluginConfigAccessor p in _configAccessors.Values ) p.RaiseConfigChanged( e );
            }
            else
            {
                PluginConfigAccessor result;
                foreach( INamedVersionedUniqueId pId in e.MultiPluginId )
                {
                    if( _configAccessors.TryGetValue( pId, out result ) ) result.RaiseConfigChanged( e );
                }
            }
        }

        private void ConfigureConfigAccessors( IPluginProxy p )
        {
            Type pType = p.RealPluginObject.GetType();
            foreach( IPluginConfigAccessorInfo e in p.PluginKey.EditorsInfo )
            {
                Debug.Assert( e.Plugin == p.PluginKey );
                if( e.IsConfigurationPropertyValid )
                {
                    // The PluginConfigAccessor may be null.
                    PluginConfigAccessor a = GetConfigAccessor( e.EditedSource );
                    PropertyInfo pEdited = pType.GetProperty( e.ConfigurationPropertyName );
                    Debug.Assert( pEdited != null );
                    pEdited.SetValue( p.RealPluginObject, a, null );
                }
            }
        }
    }
}
