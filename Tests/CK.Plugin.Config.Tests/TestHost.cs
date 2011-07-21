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
