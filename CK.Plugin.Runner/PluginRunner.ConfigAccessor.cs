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
            
            // Switch from whatever INamedVersionedUniqueId is to IPluginProxy... if it is loaded.
            if( !(idEdited is IPluginProxy) )
            {
                if( _host.FindLoadedPlugin( idEdited.UniqueId, true ) == null )
                {
                    _configAccessors.Remove( idEdited );
                    return null;
                }
                else
                {
                    idEdited = _host.FindLoadedPlugin( idEdited.UniqueId, true );
                }
            }
            Debug.Assert( idEdited is IPluginProxy );
            
            PluginConfigAccessor result;
            if( !_configAccessors.TryGetValue( idEdited, out result ) )
            {
                result = new PluginConfigAccessor( idEdited, _config.Extended, _contextObject );
                _configAccessors.Add( idEdited, result );
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
