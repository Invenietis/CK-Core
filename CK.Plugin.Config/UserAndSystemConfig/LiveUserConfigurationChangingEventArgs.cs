using System;
using CK.Core;
using System.ComponentModel;

namespace CK.Plugin.Config
{
    public class LiveUserConfigurationChangingEventArgs : CancelEventArgs
    {
        public ChangeStatus ChangeAction { get; private set; }

        public Guid PluginID { get; private set; }

        public ConfigUserAction Action { get; private set; }

        public LiveUserConfigurationChangingEventArgs( ChangeStatus changeAction, Guid pluginID, ConfigUserAction action )
        {
            ChangeAction = changeAction;
            PluginID = pluginID;
            Action = action;
        }
    }
}
