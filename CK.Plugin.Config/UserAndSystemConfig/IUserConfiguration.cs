
namespace CK.Plugin.Config
{
    /// <summary>
    /// User related configuration. 
    /// This is the second level of configuration that comes above <see cref="ISystemConfiguration"/>.
    /// </summary>
    public interface IUserConfiguration
    {
        /// <summary>
        /// Gets all the <see cref="IUriHistory">contexts</see> previously seeb by the user.
        /// </summary>
        IUriHistoryCollection ContextProfiles { get; }

        /// <summary>
        /// Gets or sets the context that must be considered as the current one.
        /// When setting it, the value must already belong to the profiles in <see cref="ContextProfiles"/> (otherwise an exception is thrown)
        /// and it becomes the first one.
        /// </summary>
        IUriHistory CurrentContextProfile { get; set; }

        /// <summary>
        /// Gets the host dictionary for user configuration.
        /// </summary>
        IObjectPluginConfig HostConfig { get; }
       
        /// <summary>
        /// Gets <see cref="IPluginStatus">plugins status</see> configured at the user level.
        /// </summary>
        IPluginStatusCollection PluginsStatus { get; }

        /// <summary>
        /// Gets the "live" configuration level. 
        /// Live configuration can override <see cref="PluginsStatus"/>.
        /// </summary>
        ILiveUserConfiguration LiveUserConfiguration { get; }

    }
}
