using System;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Holds the UserActions for each plugin
    /// </summary>
    public interface ILiveUserConfiguration : IReadOnlyCollection<ILiveUserAction>
    {
        event EventHandler<LiveUserConfigurationChangingEventArgs> Changing;

        event EventHandler<LiveUserConfigurationChangedEventArgs> Changed;

        /// <summary>
        /// Sets a <see cref="ConfigUserAction"/> to a given plugin.
        /// </summary>
        ILiveUserAction SetAction( Guid pluginId, ConfigUserAction actionType );

        /// <summary>
        /// Gets the <see cref="ConfigUserAction"/> related to the given plugin.
        /// </summary>
        ConfigUserAction GetAction( Guid pluginId );

        /// <summary>
        /// Remove the <see cref="ConfigUserAction"/> for the given plugin.
        /// </summary>
        void ResetAction( Guid pluginId );
    }
}
