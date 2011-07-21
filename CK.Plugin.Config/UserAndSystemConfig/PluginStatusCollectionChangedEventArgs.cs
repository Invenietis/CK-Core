using System;
using CK.Core;

namespace CK.Plugin.Config
{
    public class PluginStatusCollectionChangedEventArgs : EventArgs
    {
        public ChangeStatus Action { get; private set; }

        public IPluginStatusCollection Collection { get; private set; }

        public Guid PluginID { get; private set; }

        public ConfigPluginStatus Status { get; private set; }

        public PluginStatusCollectionChangedEventArgs( IPluginStatusCollection c, ChangeStatus action, Guid pluginID, ConfigPluginStatus status )
        {
            Collection = c;
            Action = action;
            PluginID = pluginID;
            Status = status;
        }
    }

}
