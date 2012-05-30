#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\SystemConfiguration.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Storage;
using System.ComponentModel;

namespace CK.Plugin.Config
{
    internal class SystemConfiguration : ConfigurationBase, ISystemConfiguration
    {
        public SystemConfiguration( ConfigManagerImpl configManager )
            : base( configManager, "UserProfile" )
        {
        }

        internal override void OnCollectionChanged()
        {
            ConfigManager.IsSystemConfigDirty = true;
            base.OnCollectionChanged();
        }

        public IUriHistory CurrentUserProfile
        {
            get { return base.UriHistoryCollection.Current; }
            set { base.UriHistoryCollection.Current = value; }
        }

        public IUriHistory PreviousUserProfile
        {
            get { return base.UriHistoryCollection.Previous; }
        }

        IUriHistoryCollection ISystemConfiguration.UserProfiles
        {
            get { return base.UriHistoryCollection; }
        }

        IPluginStatusCollection ISystemConfiguration.PluginsStatus
        {
            get { return base.PluginStatusCollection; }
        }

        public IObjectPluginConfig HostConfig
        {
            get { return ConfigManager.HostSystemConfig; }
        }

    }
}
