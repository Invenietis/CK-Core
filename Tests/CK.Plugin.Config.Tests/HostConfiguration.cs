using System;
using System.IO;
using CK.Context;
using CK.Core;
using CK.Plugin.Config;
using CK.SharedDic;
using CK.Storage;
using NUnit.Framework;

namespace PluginConfig
{
    [TestFixture]
    public class HostConfiguration : TestBase
    {
        [SetUp]
        [TearDown]
        public void TearDown()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void WriteReadUserConfig()
        {
            string path = Path.Combine( TestFolder, "UserConfig.xml" );
            Guid id = new Guid("{6AFBAE01-5CD1-4EDE-BB56-4590C5A253DF}");
            
            // Write ----------------------------------------------------------
            {
                ISharedDictionary dic = SharedDictionary.Create( null );
                IConfigManagerExtended config = ConfigurationManager.Create( dic );
                Assert.That( config, Is.Not.Null );
                Assert.That( config.HostUserConfig, Is.Not.Null );
                Assert.That( config.ConfigManager.UserConfiguration, Is.Not.Null );

                config.HostUserConfig["key1"] = "value1";
                config.HostUserConfig["key2"] = "value2";
                config.HostUserConfig["key3"] = "value3";
                config.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );

                Assert.That( config.IsUserConfigDirty );
                Assert.That( config.IsSystemConfigDirty, Is.False );

                using( Stream wrt = new FileStream( path, FileMode.Create ) )
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, null ) )
                {
                    config.SaveUserConfig( sw );
                }
            }

            // Read ------------------------------------------------------------
            {
                ISimpleServiceContainer container = new SimpleServiceContainer();
                ISharedDictionary dic = SharedDictionary.Create( container );
                IConfigManagerExtended config = ConfigurationManager.Create( dic );

                using( Stream str = new FileStream( path, FileMode.Open ) )
                using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, container ) )
                {
                    config.LoadUserConfig( sr, null );
                }
                Assert.That( config.HostUserConfig["key1"], Is.EqualTo( "value1" ) );
                Assert.That( config.HostUserConfig["key2"], Is.EqualTo( "value2" ) );
                Assert.That( config.HostUserConfig["key3"], Is.EqualTo( "value3" ) );
                Assert.That( config.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id, ConfigPluginStatus.Disabled ) == ConfigPluginStatus.AutomaticStart );
            }
        }

        [Test]
        public void WriteReadSystemConfig()
        {
            string path = Path.Combine( TestFolder, "SystemConfig.xml" );

            // Write ----------------------------------------------------------
            {
                ISharedDictionary dic = SharedDictionary.Create( null );
                IConfigManagerExtended config = ConfigurationManager.Create( dic );

                Assert.That( config.ConfigManager.SystemConfiguration != null );

                config.HostSystemConfig["key1"] = "value1";
                config.HostSystemConfig["key2"] = "value2";
                config.HostSystemConfig["key3"] = "value3";
                config.HostSystemConfig["{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}"] = true;

                Assert.That( config.IsSystemConfigDirty );
                Assert.That( config.IsUserConfigDirty, Is.False );

                using( Stream wrt = new FileStream( path, FileMode.Create ) )
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, null ) )
                {
                    config.SaveSystemConfig( sw );
                }
            }
            TestBase.DumpFileToConsole( path );
            // Read ------------------------------------------------------------
            {
                ISharedDictionary dic = SharedDictionary.Create( null );
                IConfigManagerExtended config = new ConfigManagerImpl( dic );

                using( Stream str = new FileStream( path, FileMode.Open ) )
                using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, null ) )
                {
                    config.LoadSystemConfig( sr );
                }
                Assert.That( config.HostSystemConfig["key1"], Is.EqualTo( "value1" ) );
                Assert.That( config.HostSystemConfig["key2"], Is.EqualTo( "value2" ) );
                Assert.That( config.HostSystemConfig["key3"], Is.EqualTo( "value3" ) );
                Assert.That( config.HostSystemConfig["{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}"], Is.EqualTo( true ) );
            }
        }

        [Test]
        public void DirtyUserFinalDictionnary()
        {
            IContext ctx = CreateContext();
            File.Delete( Host.DefaultUserConfigPath );
            IConfigManagerExtended config = ctx.ConfigManager.Extended;
            Assert.IsNotNull( config );

            Assert.That( config.ConfigManager.UserConfiguration != null );

            Assert.That( Host.UserConfig != null );

            Assert.That( !config.IsUserConfigDirty );
            Host.UserConfig.GetOrSet( "key2", "value2" );
            Assert.That( config.IsUserConfigDirty );

            Assert.That( Host.UserConfig.Count, Is.EqualTo( 1 ) );
        }
       
        [Test]
        public void DirtySystemFinalDictionnary()
        {
            IContext ctx = CreateContext();

            IConfigManagerExtended config = ctx.ConfigManager.Extended;
            Assert.IsNotNull( config );

            Assert.That( config.ConfigManager.SystemConfiguration != null );

            Assert.That( Host.SystemConfig != null );
            Assert.That( !config.IsSystemConfigDirty );
            Assert.That( !config.IsUserConfigDirty );

            Host.SystemConfig.GetOrSet( "key2", "value2" );

            Assert.That( config.IsSystemConfigDirty );
            Assert.That( !config.IsUserConfigDirty );

            Assert.That( Host.SystemConfig.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void CheckSystemConfigurationInstances()
        {
            // Creates system configuration with one user profile.
            {
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "Test", GetTestFilePath( "TestProfile" ), ConfigSupportType.File, false );

                foreach( var profile in ctx.ConfigManager.SystemConfiguration.UserProfiles )
                {
                    Assert.That( ((UserProfile)profile).Holder == ctx.ConfigManager.SystemConfiguration.UserProfiles );
                }

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Reloads it
            {
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                foreach( var profile in ctx.ConfigManager.SystemConfiguration.UserProfiles )
                {
                    Assert.That( ((UserProfile)profile).Holder == ctx.ConfigManager.SystemConfiguration.UserProfiles );
                }
            }
        }

        [Test]
        public void ReloadPreviousContext()
        {
            INamedVersionedUniqueId pluginId = new SimpleNamedVersionedUniqueId( Guid.NewGuid(), Util.EmptyVersion, "JustForTest" );

            // Creates typical user configuration.
            {
                IContext ctx = CreateContext();
                IConfigManager config = ctx.ConfigManager;
                IConfigContainer dic = ctx.ConfigManager.Extended.Container;

                Assert.That( config.SystemConfiguration != null );
                IUserProfile p = ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "Config-" + Guid.NewGuid().ToString(), GetTestFilePath( "UserConfig" ), ConfigSupportType.File, true );
                Assert.That( config.UserConfiguration != null );

                Assert.That( !dic.Contains( pluginId ), "The plugin is not known yet." );
                config.Extended.Container[ctx, pluginId, "testKey"] = "testValue";
                Assert.That( dic.Contains( pluginId ), "Setting a value ensures that the plugin is registered." );

                Host.ContextPath = GetTestFilePath( "Context" );

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile == p );

                Host.SaveContext();
                Host.SaveUserConfig();
                Host.SaveSystemConfig();

                TestBase.DumpFileToConsole( Host.ContextPath );
            }

            // Loads existing configuration, with Keyboards etc.
            {
                IContext ctx = CreateContext();
                IConfigManager config = ctx.ConfigManager;
                IConfigContainer dic = ctx.ConfigManager.Extended.Container;

                dic.Ensure( pluginId );

                Assert.That( config.UserConfiguration != null );
                Host.RestoreLastContext();

                Assert.That( (string)dic[ctx, pluginId, "testKey"] == "testValue" );
            }
        }
    }
}
