using System;
using System.Diagnostics;
using CK.Core;
using CK.Storage;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Holds a <see cref="PluginStatusCollection"/>, the <see cref="LiveUserConfiguration"/> and the historic
    /// of the contexts. 
    /// </summary>
    internal class UserConfiguration : ConfigurationBase, IUserConfiguration
    {
        LiveUserConfiguration _live;

        public UserConfiguration( ConfigManagerImpl configManager )
            : base( configManager, "ContextProfile" )
        {
            _live = new LiveUserConfiguration();
        }

        internal override void OnCollectionChanged()
        {
            ConfigManager.IsUserConfigDirty = true;
            base.OnCollectionChanged();
        }


        public ILiveUserConfiguration LiveUserConfiguration
        {
            get { return _live; }
        }

        IPluginStatusCollection IUserConfiguration.PluginsStatus
        {
            get { return base.PluginStatusCollection; }
        }

        public IUriHistory CurrentContextProfile
        {
            get { return base.UriHistoryCollection.Current; }
            set { base.UriHistoryCollection.Current = value; }
        }

        IUriHistoryCollection IUserConfiguration.ContextProfiles
        {
            get { return base.UriHistoryCollection; }
        }

        public IObjectPluginConfig HostConfig
        {
            get { return ConfigManager.HostUserConfig; }
        }


    }
}
