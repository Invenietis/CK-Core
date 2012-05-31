#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\MiniContext.cs) is part of CiviKey. 
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

using CK.Core;
using CK.Plugin.Config;
using CK.SharedDic;

namespace PluginConfig
{
   public class MiniContext
    {
        MiniContext( string name )
        {
            ServiceContainer = new SimpleServiceContainer();
            ServiceContainer.Add( RequirementLayerSerializer.Instance );
            ServiceContainer.Add( SimpleTypeFinder.Default );

            ConfigContainer = SharedDictionary.Create( ServiceContainer );
            ConfigManager = ConfigurationManager.Create( ConfigContainer ).ConfigManager;

        }

        static public MiniContext CreateMiniContext( string name )
        {
            return new MiniContext( name );
        }

        public ISimpleServiceContainer ServiceContainer { get; private set; }
        public ISharedDictionary ConfigContainer { get; private set; }
        public IConfigManager ConfigManager { get; private set; }

        public IObjectPluginConfig HostUserConfig { get { return ConfigManager.Extended.HostUserConfig; } }
        public IObjectPluginConfig HostSystemConfig { get { return ConfigManager.Extended.HostSystemConfig; } }

    }
}
