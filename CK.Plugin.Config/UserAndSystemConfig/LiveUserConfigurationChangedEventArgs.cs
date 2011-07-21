using System;
using CK.Core;

namespace CK.Plugin.Config
{
    public class LiveUserConfigurationChangedEventArgs : EventArgs
    {
        public ChangeStatus ChangeAction { get; private set; }

        public Guid PluginID { get; private set; }

        public ConfigUserAction Action { get; private set; }

        public LiveUserConfigurationChangedEventArgs( ChangeStatus changeAction, Guid pluginID, ConfigUserAction action )
        {
            ChangeAction = changeAction;
            PluginID = pluginID;
            Action = action;
        }
    }
}
