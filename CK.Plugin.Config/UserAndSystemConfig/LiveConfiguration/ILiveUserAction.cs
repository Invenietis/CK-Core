
using System;

namespace CK.Plugin.Config
{
    public interface ILiveUserAction
    {
        Guid PluginId { get; }

        ConfigUserAction Action { get; }
    }
}
