using System;
using System.Diagnostics;
using CK.Core;
using CK.Storage;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Holds a <see cref="PluginStatusCollection"/>, as well as a <see cref="LiveUserConfiguration"/>. 
    /// </summary>
    internal class UserConfiguration : ConfigurationBase, IUserConfiguration, IStructuredSerializable
    {
        LiveUserConfiguration _live;

        public UserConfiguration( ConfigManagerImpl configManager )
            : base( configManager )
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
            get { return PluginStatusCollection; }
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            sr.Xml.Read();
            sr.ReadInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sr.GetService<ISharedDictionaryReader>( true ).ReadPluginsDataElement( "Plugins", this );
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            sw.Xml.WriteAttributeString( "Version", "1.0.0.0" );
            sw.WriteInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sw.GetService<ISharedDictionaryWriter>( true ).WritePluginsDataElement( "Plugins", this );
        }

        public IObjectPluginConfig HostConfig
        {
            get { return ConfigManager.HostUserConfig; }
        }


    }
}
