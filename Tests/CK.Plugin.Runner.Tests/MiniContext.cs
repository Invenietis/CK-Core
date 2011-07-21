using CK.Core;
using CK.Plugin.Config;
using CK.SharedDic;

namespace CK.Plugin.Hosting
{

    public class MiniContext
    {
        MiniContext( string name )
        {
            ServiceContainer = new SimpleServiceContainer();

            ContextObject = new object();
            ConfigContainer = SharedDictionary.Create( ServiceContainer );
            ConfigManager = ConfigurationManager.Create( ConfigContainer ).ConfigManager;
            PluginRunner = new PluginRunner( ServiceContainer, ConfigManager );
            PluginRunner.Initialize( ContextObject );
            ServiceContainer.Add<IConfigContainer>( ConfigContainer );
        }

        static public MiniContext CreateMiniContext( string name )
        {
            return new MiniContext( name );
        }

        public object ContextObject { get; private set; }
        public ISimpleServiceContainer ServiceContainer { get; private set; }
        public ISharedDictionary ConfigContainer { get; private set; }
        public IConfigManager ConfigManager { get; private set; }
        public PluginRunner PluginRunner { get; private set; }

        public IObjectPluginConfig HostUserConfig { get { return ConfigManager.Extended.HostUserConfig; } }
        public IObjectPluginConfig HostSystemConfig { get { return ConfigManager.Extended.HostSystemConfig; } }

    }
}
