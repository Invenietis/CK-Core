using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin.Hosting
{

    class PluginProxy : PluginProxyBase, IPluginProxy
    {
        public PluginProxy( IPluginInfo pluginKey )
        {
            PluginKey = pluginKey;
        }

        public IPluginInfo PluginKey { get; private set; }

        public Guid UniqueId { get { return PluginKey.UniqueId; } }

        public Version Version { get { return PluginKey.Version; } }

        public string PublicName { get { return PluginKey.PublicName; } }

        //public object RealPluginObject { get { return base.RealPluginObject; } }

        internal bool TryLoad( ServiceHost serviceHost, Func<IPluginInfo, IPlugin> pluginCreator )
        {
            return TryLoad( serviceHost, () => pluginCreator( PluginKey ), PluginKey );
        }

    }
}
