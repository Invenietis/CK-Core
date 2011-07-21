using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace DisabledStatusPlugins
{
    /// <summary>
    /// Default : Man
    /// SystemConf : Enabled
    /// UserConf : Disabled
    /// </summary>
    [Plugin( Categories = new string[] { "Test" }, 
    DefaultPluginStatus = ConfigPluginStatus.Manual,
    PublicName = "Plugin_Man_Enabled_Disabled", Version = "2.5.0",
    Id = "{FF395F7D-A4DE-4f92-AE6A-898BD020598D}")]
    public class Plugin_Man_Enabled_Disabled : IPlugin
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
