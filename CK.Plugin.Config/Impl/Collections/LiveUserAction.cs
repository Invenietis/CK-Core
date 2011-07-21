using System;
namespace CK.Plugin.Config
{
    internal class LiveUserAction : ILiveUserAction
    {
        public Guid PluginId { get; private set; }

        public ConfigUserAction Action { get; internal set; }

        public LiveUserAction( Guid pluginID, ConfigUserAction action )
        {
            PluginId = pluginID;
            Action = action;
        }
    }
}
