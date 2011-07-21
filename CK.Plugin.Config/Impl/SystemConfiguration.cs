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
using CK.Core;
using CK.Storage;

namespace CK.Plugin.Config
{
    internal class SystemConfiguration : ConfigurationBase, ISystemConfiguration, IStructuredSerializable
    {
        UserProfileCollection _profiles;

        public SystemConfiguration( ConfigManagerImpl configManager )
            : base( configManager )
        {
           _profiles = new UserProfileCollection( this );
        }

        internal override void OnCollectionChanged()
        {
            ConfigManager.IsSystemConfigDirty = true;
            base.OnCollectionChanged();
        }

        public UserProfileCollection UserProfiles
        {
            get { return _profiles; }
        }

        IUserProfileCollection ISystemConfiguration.UserProfiles
        {
            get { return _profiles; }
        }

        IPluginStatusCollection ISystemConfiguration.PluginsStatus
        {
            get { return PluginStatusCollection; }
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            sr.Xml.Read();
            sr.ReadInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sr.ReadInlineObjectStructuredElement( "UserProfileCollection", _profiles );
            sr.GetService<ISharedDictionaryReader>( true ).ReadPluginsDataElement( "Plugins", this );
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            sw.Xml.WriteAttributeString( "Version", "1.0.0.0" );
            sw.WriteInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sw.WriteInlineObjectStructuredElement( "UserProfileCollection", _profiles );
            sw.GetService<ISharedDictionaryWriter>( true ).WritePluginsDataElement( "Plugins", this );
        }

        public IObjectPluginConfig HostConfig
        {
            get { return ConfigManager.HostSystemConfig; }
        }

    }
}
