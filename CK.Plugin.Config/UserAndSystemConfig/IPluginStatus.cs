using System;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Describes what's a <see cref="IPluginStatus"/>.
    /// </summary>
    public interface IPluginStatus
    {
        /// <summary>
        /// Gets the unique ID of the plugin
        /// </summary>
        Guid PluginId { get; }

        /// <summary>
        /// Gets ConfigPluginStatus.
        /// </summary>
        ConfigPluginStatus Status { get; set; }

        /// <summary>
        /// It will destroy the plugin status, and remove it from its parent collection.
        /// </summary>
        void Destroy();
    }
}
