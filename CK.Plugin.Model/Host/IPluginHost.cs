using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Host for <see cref="IPlugin"/> management.
    /// </summary>
    public interface IPluginHost
    {
        /// <summary>
        /// Checks whether a plugin is running or not.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsPluginRunning( IPluginInfo key );

        /// <summary>
        /// Gets the <see cref="IPluginProxy"/> for the plugin identifier. 
        /// It may find plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        /// <param name="pluginId">Plugin identifier.</param>
        /// <param name="checkCurrentlyLoading">True to take into account plugins beeing loaded during an <see cref="Execute"/> phasis.</param>
        /// <returns>Null if not found.</returns>
        IPluginProxy FindLoadedPlugin( Guid pluginId, bool checkCurrentlyLoading );

        /// <summary>
        /// Gets the loaded plugins. This contains also the plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        IReadOnlyCollection<IPluginProxy> LoadedPlugins { get; }

        /// <summary>
        /// Fires whenever a plugin status changed.
        /// </summary>
        event EventHandler<PluginStatusChangedEventArgs> StatusChanged;

    }
}
