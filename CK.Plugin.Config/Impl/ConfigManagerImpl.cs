#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\ConfigManager.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using CK.SharedDic;
using CK.Storage;
using System.ComponentModel;
using CK.Core;
using System.Xml;

namespace CK.Plugin.Config
{
    internal class ConfigManagerImpl : IConfigManager, IConfigManagerExtended
    {
        static readonly SimpleNamedVersionedUniqueId _pluginId = new SimpleNamedVersionedUniqueId( "{1A2DC25C-E357-488A-B2B2-CD2D7E029856}", new Version( 1, 0, 0, 0 ), "ConfigManager" );

		ISharedDictionary _dic;

        UserConfiguration _userConfiguration;
        IObjectPluginConfig _hostUserConfig;
        
        SystemConfiguration _systemConfiguration;
        IObjectPluginConfig _hostSystemConfig;

        ISolvedPluginConfiguration _solvedPluginConfiguration;

        public event EventHandler SaveUserConfigRequired;

        public event EventHandler SaveSystemConfigRequired;

        public event EventHandler LoadUserConfigRequired;

        public event EventHandler LoadSystemConfigRequired;

        public event EventHandler UserChanged;

		bool _systemConfigLoaded;
        bool _userConfigLoaded;

        public ConfigManagerImpl( ISharedDictionary dic )
        {
			_dic = dic;
            _dic.Changed += new EventHandler<ConfigChangedEventArgs>( ObjectConfigurationChanged );
            _systemConfiguration = new SystemConfiguration( this );
            _userConfiguration = new UserConfiguration( this );
            _solvedPluginConfiguration = new SolvedPluginConfiguration( this );
		}

        IConfigManagerExtended IConfigManager.Extended
        {
            get { return this; }
        }

        IConfigManager IConfigManagerExtended.ConfigManager
        {
            get { return this; }
        }

        public INamedVersionedUniqueId ConfigPluginId
        {
            get { return _pluginId; }
        }

        public ISolvedPluginConfiguration SolvedPluginConfiguration
        {
            get { return _solvedPluginConfiguration; }
        }

        public IConfigContainer Container
        {
            get { return _dic; }
        }

        public ISharedDictionary SharedDictionary
        {
            get { return _dic; }
        }

        public bool IsUserConfigDirty
        {
            get;
            internal set;
        }

        public bool IsSystemConfigDirty
        {
            get;
            internal set;
        }

        void ObjectConfigurationChanged( object source, ConfigChangedEventArgs e )
		{
			Debug.Assert( source == _dic );
			if( e.Obj == _userConfiguration )
			{
				IsUserConfigDirty = true;
           }
            else if( e.Obj == _systemConfiguration )
            {
                IsSystemConfigDirty = true;
            }
		}

		#region  Objects that hold different configurations informations.

        internal SystemConfiguration SystemConfiguration
        {
            get 
            {
                if( !_systemConfigLoaded )
                {
                    if( LoadSystemConfigRequired != null )
                    {
                        _systemConfigLoaded = true;
                        LoadSystemConfigRequired( this, EventArgs.Empty );
                    }
                }
                return _systemConfiguration; 
            }
        }

        ISystemConfiguration IConfigManager.SystemConfiguration
        {
            get { return SystemConfiguration; }
        }

        internal UserConfiguration UserConfiguration
		{
			get 
            {
                if( !_userConfigLoaded )
                {
                    if( LoadUserConfigRequired != null )
                    {
                        _userConfigLoaded = true;
                        LoadUserConfigRequired( this, EventArgs.Empty );
                    }
                }
                return _userConfiguration; 
            }
		}

        IUserConfiguration IConfigManager.UserConfiguration
        {
            get { return UserConfiguration; }
        }

        public IObjectPluginConfig HostSystemConfig
        {
            get { return _hostSystemConfig ?? (_hostSystemConfig = _dic.GetObjectPluginConfig( SystemConfiguration, _pluginId, true ) ); }
        }

        public IObjectPluginConfig HostUserConfig
        {
            get { return _hostUserConfig ?? (_hostUserConfig = _dic.GetObjectPluginConfig( UserConfiguration, _pluginId, true )); }
        }

		#endregion

        public void FireSaveUserConfigRequired()
        {
            var h = SaveUserConfigRequired;
            if( h != null ) h( this, EventArgs.Empty );
        }

        public void FireSaveSystemConfigRequired()
        {
            var h = SaveSystemConfigRequired;
            if( h != null ) h( this, EventArgs.Empty );
        }

        public void SaveUserConfig( IStructuredWriter writer )
		{
            using( var dw = _dic.RegisterWriter( writer ) )
            {
                writer.WriteInlineObjectStructuredElement( "User", _userConfiguration );
            }
            IsUserConfigDirty = false;
		}

		public IList<ReadElementObjectInfo> LoadUserConfig( IStructuredReader reader, IUserProfile setLastProfile )
		{
            if( reader == null ) throw new ArgumentNullException( "reader" );

            IList<ReadElementObjectInfo> objs;
            using( var dr = _dic.RegisterReader( reader, MergeMode.ReplaceExistingTryMerge ) )
            {
                reader.ReadInlineObjectStructuredElement( "User", _userConfiguration );
                objs = dr.ErrorCollector;
            }
            IsUserConfigDirty = false;
            if( setLastProfile != null ) _systemConfiguration.UserProfiles.LastProfile = setLastProfile;
            if( UserChanged != null ) UserChanged( this, EventArgs.Empty );
            
            return objs;        
		}

        public void SaveSystemConfig( IStructuredWriter writer )
        {
            using( var dw = _dic.RegisterWriter( writer ) )
            {
                writer.WriteInlineObjectStructuredElement( "System", _systemConfiguration );
            }
            IsSystemConfigDirty = false;
        }

        public IList<ReadElementObjectInfo> LoadSystemConfig( IStructuredReader reader )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );

            // Creates reader.
            IList<ReadElementObjectInfo> objs;
            using( var dr = _dic.RegisterReader( reader, MergeMode.ReplaceExistingTryMerge ) )
            {
                reader.ReadInlineObjectStructuredElement( "System", _systemConfiguration );                
                objs = dr.ErrorCollector;
            }
            IsSystemConfigDirty = false;
            return objs;
        }

    }
}
