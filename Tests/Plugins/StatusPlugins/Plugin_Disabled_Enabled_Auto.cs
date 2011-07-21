using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace DisabledStatusPlugins
{
    /// <summary>
    /// Default : Disabled
    /// SystemConf : Enabled
    /// UserConf : Auto
    /// </summary>
    [Plugin( Categories = new string[] { "Test" }, 
    DefaultPluginStatus = ConfigPluginStatus.Disabled,
    PublicName = "Plugin_Disabled_Enabled_Auto", Version = "2.5.0",
    Id = "{56A823EC-103C-41b5-AF2E-A39868866ECA}")]
    public class Plugin_Disabled_Enabled_Auto : IPlugin
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
