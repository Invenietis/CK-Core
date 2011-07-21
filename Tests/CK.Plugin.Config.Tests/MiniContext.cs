using CK.Core;
using CK.Plugin.Config;
using CK.SharedDic;

namespace PluginConfig
{
   public class MiniContext
    {
        MiniContext( string name )
        {
            ServiceContainer = new SimpleServiceContainer();
            ServiceContainer.Add( RequirementLayerSerializer.Instance );
            ServiceContainer.Add( SimpleTypeFinder.Default );

            ConfigContainer = SharedDictionary.Create( ServiceContainer );
            ConfigManager = ConfigurationManager.Create( ConfigContainer ).ConfigManager;

        }

        static public MiniContext CreateMiniContext( string name )
        {
            return new MiniContext( name );
        }

        public ISimpleServiceContainer ServiceContainer { get; private set; }
        public ISharedDictionary ConfigContainer { get; private set; }
        public IConfigManager ConfigManager { get; private set; }

        public IObjectPluginConfig HostUserConfig { get { return ConfigManager.Extended.HostUserConfig; } }
        public IObjectPluginConfig HostSystemConfig { get { return ConfigManager.Extended.HostSystemConfig; } }

    }
}
