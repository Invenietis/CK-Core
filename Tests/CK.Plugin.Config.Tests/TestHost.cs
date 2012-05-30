#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\TestHost.cs) is part of CiviKey. 
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
using CK.Context;
using CK.Plugin.Config;

namespace PluginConfig
{
    public class TestHost : StandardContextHost
    {
        public TestHost( string appName )
            : base( appName, "2.5" )
        {
        }

        public new IObjectPluginConfig UserConfig { get { return base.UserConfig; } }

        public new IObjectPluginConfig SystemConfig { get { return base.SystemConfig; } }

        public string CustomSystemConfigPath { get; set; }

        public override string DefaultSystemConfigPath
        {
            get { return CustomSystemConfigPath; }
        }

        public new void SaveSystemConfig()
        {
            base.SaveSystemConfig();
        }

        public new void SaveUserConfig()
        {
            base.SaveUserConfig();
        }

        public new void SaveContext()
        {
            base.SaveContext();
        }

        public new bool LoadUserConfigFromFile( IUserProfile profile )
        {
            return base.LoadUserConfigFromFile( profile );
        }
    }
}
