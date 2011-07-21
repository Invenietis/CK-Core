using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Plugin.Config;

namespace CK.Global.Tests
{
    public class TestHost : StandardContextHost
    {
        public TestHost(string appName)
            : base(appName, "2.5")
        {
        }

        public new IObjectPluginConfig UserConfig { get { return base.UserConfig; } }

        public new IObjectPluginConfig SystemConfig { get { return base.SystemConfig; } }
    }
}
