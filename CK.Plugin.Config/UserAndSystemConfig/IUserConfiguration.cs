
namespace CK.Plugin.Config
{
    /// <summary>
    /// User related configuration. 
    /// This is the second level of configuration that comes above <see cref="ISystemConfiguration"/>.
    /// </summary>
    public interface IUserConfiguration
    {
        /// <summary>
        /// Gets <see cref="IPluginStatus">plugins status</see> configured at the user level.
        /// </summary>
        IPluginStatusCollection PluginsStatus { get; }

        /// <summary>
        /// Gets the "live" configuration level. 
        /// Live configuration can override <see cref="PluginsStatus"/>.
        /// </summary>
        ILiveUserConfiguration LiveUserConfiguration { get; }

        /// <summary>
        /// Gets the host dictionary for user configuration.
        /// </summary>
        IObjectPluginConfig HostConfig { get; }
       
    }
}
