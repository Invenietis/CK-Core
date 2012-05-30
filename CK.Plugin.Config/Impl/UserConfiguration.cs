#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\UserConfiguration.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using CK.Storage;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Holds a <see cref="PluginStatusCollection"/>, the <see cref="LiveUserConfiguration"/> and the historic
    /// of the contexts. 
    /// </summary>
    internal class UserConfiguration : ConfigurationBase, IUserConfiguration
    {
        LiveUserConfiguration _live;

        public UserConfiguration( ConfigManagerImpl configManager )
            : base( configManager, "ContextProfile" )
        {
            _live = new LiveUserConfiguration();
        }

        internal override void OnCollectionChanged()
        {
            ConfigManager.IsUserConfigDirty = true;
            base.OnCollectionChanged();
        }


        public ILiveUserConfiguration LiveUserConfiguration
        {
            get { return _live; }
        }

        IPluginStatusCollection IUserConfiguration.PluginsStatus
        {
            get { return base.PluginStatusCollection; }
        }

        public IUriHistory CurrentContextProfile
        {
            get { return base.UriHistoryCollection.Current; }
            set { base.UriHistoryCollection.Current = value; }
        }

        IUriHistoryCollection IUserConfiguration.ContextProfiles
        {
            get { return base.UriHistoryCollection; }
        }

        public IObjectPluginConfig HostConfig
        {
            get { return ConfigManager.HostUserConfig; }
        }


    }
}
