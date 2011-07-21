using System;
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Base class for <see cref="UserConfiguration"/> and <see cref="SystemConfiguration"/>. 
    /// </summary>
    internal abstract class ConfigurationBase
    {
        protected ConfigurationBase( ConfigManagerImpl configManager )
        {
            ConfigManager = configManager;
            PluginStatusCollection = new PluginStatusCollection( this );
        }

        protected readonly ConfigManagerImpl ConfigManager;

        internal readonly PluginStatusCollection PluginStatusCollection;

        internal void OnPluginStatusCollectionChanged( ChangeStatus action, Guid pluginId, ConfigPluginStatus status )
        {
            OnCollectionChanged(); 
        }

        internal virtual void OnCollectionChanged()
        {
        }
    }
}
