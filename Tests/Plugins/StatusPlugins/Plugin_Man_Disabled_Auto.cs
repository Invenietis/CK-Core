using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace DisabledStatusPlugins
{
    /// <summary>
    /// Default : Man
    /// SystemConf : Disabled
    /// UserConf : Auto
    /// </summary>
    [Plugin( Categories = new string[] { "Test" }, 
    DefaultPluginStatus = ConfigPluginStatus.Manual,
    PublicName = "Plugin_Man_Disabled_Auto", Version = "2.5.0",
    Id = "{0A5844EE-9795-496b-A67E-B0CBCDF02D0E}")]
    public class Plugin_Man_Disabled_Auto : IPlugin
    {

        public bool CanStart(out string lastError)
        {
            lastError = "";
            return true;
        }

        public bool Setup(ISetupInfo info)
        {
            return true;
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}
