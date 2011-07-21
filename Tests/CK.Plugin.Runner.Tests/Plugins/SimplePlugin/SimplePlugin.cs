using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace SimplePlugin
{
    /// <summary>
    /// Simple plugin without any config accessor, service reference, or service implementation.
    /// Used by :
    ///     * CK.Plugin.Runner.Tests.Apply
    /// </summary>
    [Plugin( pluginId, Version=version )]
    public class SimplePlugin : IPlugin
    {
        const string pluginId = "{EEAEC976-2AFC-4A68-BFAD-68E169677D52}";
        const string version = "1.0.0";

        public bool HasBeenSarted { get; private set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            HasBeenSarted = true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        public void Stop()
        {
            HasBeenSarted = false;
        }
    }
}
