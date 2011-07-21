using System;
using System.Diagnostics;
using System.Collections.Generic;
using CK.Core;

namespace CK.Plugin.Config
{
    internal class SolvedPluginConfiguration : ISolvedPluginConfiguration
    {

        Dictionary<Guid, SolvedPluginConfigElement> _dic;
        ConfigManagerImpl _cfg;

        public event EventHandler<SolvedPluginConfigurationChangedEventArs> Changed;

        public int Count
        {
            get { return _dic.Count; }
        }

        public System.Collections.Generic.IEnumerator<SolvedPluginConfigElement> GetEnumerator()
        {
            return _dic.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SolvedPluginConfiguration( ConfigManagerImpl cfg )
        {
            _cfg = cfg;
            _dic = new Dictionary<Guid, SolvedPluginConfigElement>();

            _cfg.UserConfiguration.PluginStatusCollection.Changed += ( o, e ) => OnPluginConfigurationChanged( e.PluginID );
            _cfg.SystemConfiguration.PluginStatusCollection.Changed += ( o, e ) => OnPluginConfigurationChanged( e.PluginID );
            _cfg.UserConfiguration.LiveUserConfiguration.Changed += ( o, e ) => OnPluginConfigurationChanged( e.PluginID );
            
            ResolveConfiguration();
        }

        void OnPluginConfigurationChanged( Guid pluginId )
        {
            // If the change is made on a single plugin:
            if( pluginId != Guid.Empty )
            {
                SolvedPluginConfigElement changed = SetStatus( pluginId );
                if( changed != null && Changed != null ) Changed( this, new SolvedPluginConfigurationChangedEventArs( changed ) );
            }
            else
            {
                // If this is a GlobalChange:
                ResolveConfiguration();
                if( Changed != null ) Changed( this, new SolvedPluginConfigurationChangedEventArs( null ) );
            }
        }

        private SolvedPluginConfigElement SetStatus( Guid pluginId )
        {
            SolvedConfigStatus newSolvedStatus = SolveStatus( pluginId );
            SolvedPluginConfigElement currentElement;
            if( _dic.TryGetValue( pluginId, out currentElement ) )
            {
                if( currentElement.Status == newSolvedStatus ) return null;
                currentElement.Status = newSolvedStatus;
             }
            else
            {
                currentElement = new SolvedPluginConfigElement( pluginId, newSolvedStatus );
                _dic.Add( pluginId, currentElement );
            }
            return currentElement;
       }

        // Resolves all plugins' status, regarding User, system and LiveUser configuration        
        void ResolveConfiguration()
        {
            HashSet<Guid> toProcess = new HashSet<Guid>();
            foreach( IPluginStatus s in _cfg.SystemConfiguration.PluginStatusCollection ) toProcess.Add( s.PluginId );
            foreach( IPluginStatus s in _cfg.UserConfiguration.PluginStatusCollection ) toProcess.Add( s.PluginId );
            foreach( ILiveUserAction s in _cfg.UserConfiguration.LiveUserConfiguration ) toProcess.Add( s.PluginId );
            foreach( Guid g in _dic.Keys ) toProcess.Add( g );

            foreach( Guid g in toProcess )
            {
                SetStatus( g );
            }
        }

        SolvedConfigStatus SolveStatus( Guid pluginId )
        {
            // Set default status/actions
            ConfigPluginStatus finalStatus = ConfigPluginStatus.Manual;
            ConfigUserAction userAction = ConfigUserAction.None;

            if( finalStatus != ConfigPluginStatus.Disabled )
            {
                // Gets the systemStatus, if any.
                ConfigPluginStatus systemStatus = _cfg.SystemConfiguration.PluginStatusCollection.GetStatus( pluginId, finalStatus );
                // Sets it if more restrictive
                if( systemStatus > finalStatus || systemStatus == ConfigPluginStatus.Disabled )
                {
                    finalStatus = systemStatus;
                }

                if( finalStatus != ConfigPluginStatus.Disabled )
                {
                    // Gets the user status, if any.
                    ConfigPluginStatus userStatus = _cfg.UserConfiguration.PluginStatusCollection.GetStatus( pluginId, finalStatus );
                    // Sets it if more restrictive.
                    if( userStatus > finalStatus || userStatus == ConfigPluginStatus.Disabled )
                    {
                        finalStatus = userStatus;
                    }

                    if( finalStatus != ConfigPluginStatus.Disabled )
                    {
                        // Gets the UserAction, if any.
                        userAction = _cfg.UserConfiguration.LiveUserConfiguration.GetAction( pluginId );
                    }
                }
            }
            // Solves UserAction and finalStatus
            SolvedConfigStatus solvedStatus = finalStatus == ConfigPluginStatus.Disabled ? SolvedConfigStatus.Disabled : SolvedConfigStatus.Optional;

            if( userAction == ConfigUserAction.Started || (finalStatus == ConfigPluginStatus.AutomaticStart && userAction != ConfigUserAction.Stopped) )
            {
                solvedStatus = SolvedConfigStatus.MustExistAndRun;
            }
            else if( userAction == ConfigUserAction.Stopped )
            {
                solvedStatus = SolvedConfigStatus.Disabled;
            }
            return solvedStatus;
        }

        public SolvedConfigStatus GetStatus( Guid pluginId )
        {
            SolvedPluginConfigElement e;
            _dic.TryGetValue( pluginId, out e );
            return e != null ? e.Status : SolvedConfigStatus.Optional;
        }

        public SolvedPluginConfigElement Find( Guid pluginId )
        {
            return _dic.GetValueWithDefault( pluginId, null );
        }

        public bool Contains( object item )
        {
            SolvedPluginConfigElement e = item as SolvedPluginConfigElement;
            return e != null ? _dic.ContainsKey( e.PluginId ) : false;
        }

    }
}
