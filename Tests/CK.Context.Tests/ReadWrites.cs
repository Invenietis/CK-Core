#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Context.Tests\ReadWrites.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using CK.Core;
using CK.Plugin.Config;

namespace CK.Context.Tests
{
    [TestFixture]
    public class ReadWrites
    {

        [Test]
        public void CheckSystemConfigurationInstances()
        {
            // Creates system configuration with one user profile.
            {
                var host = new TestContextHost( "CheckSystemConfigurationInstances" );
                File.Delete( host.SystemConfigAddress.LocalPath );
                File.Delete( host.DefaultUserConfigAddress.LocalPath );
                var ctx = host.CreateContext();
                
                Assert.That( ctx.ConfigManager.UserConfiguration != null, "Creation of a default User profile." );
                Assert.That( ctx.ConfigManager.SystemConfiguration.CurrentUserProfile.DisplayName == Environment.UserName, "Default became automatically active." );

                var p2 = ctx.ConfigManager.SystemConfiguration.UserProfiles.FindOrCreate( GetTestFileUri( "UserProfile" ) );
                p2.DisplayName = "2nd profile...";
                
                host.UserConfig["Toto"] = 3;

                Assert.That( ctx.ConfigManager.SystemConfiguration.CurrentUserProfile.DisplayName == Environment.UserName, "The last active is still the default one." );

                host.SaveUserConfig();
                host.SaveSystemConfig();

                TestBase.DumpFileToConsole( host.SystemConfigAddress.LocalPath );
                TestBase.DumpFileToConsole( host.DefaultUserConfigAddress.LocalPath );
            }
            // Reloads it.
            {
                var host = new TestContextHost( "CheckSystemConfigurationInstances" );
                var ctx = host.CreateContext();

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.Count, Is.EqualTo( 2 ), "Accessing SystemConfiguration triggers the load." );
                Assert.That( host.UserConfig["Toto"], Is.EqualTo( 3 ), "Accessing UserConfiguration triggers the load." );
            }
        }

        [Test]
        public void ReloadPreviousContext()
        {
            INamedVersionedUniqueId pluginId = new SimpleNamedVersionedUniqueId( Guid.NewGuid(), Util.EmptyVersion, "JustForTest" );

            // Creates typical user configuration.
            {
                var host = new TestContextHost( "ReloadPreviousContext" );
                File.Delete( host.SystemConfigAddress.LocalPath );
                File.Delete( host.DefaultUserConfigAddress.LocalPath );
                
                var ctx = host.CreateContext();

                Assert.That( ctx.ConfigManager.SystemConfiguration != null );
                Assert.That( ctx.ConfigManager.UserConfiguration != null, "Accessing it, creates it." );
                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.Current, Is.Not.Null, "It becomes the current one." );

                Assert.That( !ctx.ConfigManager.Extended.Container.Contains( pluginId ), "The plugin is not known yet." );
                ctx.ConfigManager.Extended.Container[ctx, pluginId, "testKey"] = "testValue";
                Assert.That( ctx.ConfigManager.Extended.Container.Contains( pluginId ), "Setting a value ensures that the plugin is registered." );


                host.SaveContext( GetTestFileUri( "Context" ) );
                host.SaveUserConfig();
                host.SaveSystemConfig();

                TestBase.DumpFileToConsole( GetTestFileUri( "Context" ).LocalPath );
            }

            // Loads existing configuration.
            {
                var host = new TestContextHost( "ReloadPreviousContext" );
                var ctx = host.CreateContext();
                
                ctx.ConfigManager.Extended.Container.Ensure( pluginId );
                host.LoadContext();

                Assert.That( ctx.ConfigManager.Extended.Container[ctx, pluginId, "testKey"], Is.EqualTo( "testValue" ) );
            }
        }

        [Test]
        public void ChangeEmptyToFilled()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            // Creates empty configuration files.
            {
                var host = new TestContextHost( "ChangeEmptyToFilled" );
                var ctx = host.CreateContext();
                File.Delete( host.SystemConfigAddress.LocalPath );
                File.Delete( host.DefaultUserConfigAddress.LocalPath );

                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );

                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ), Is.EqualTo( ConfigPluginStatus.AutomaticStart ) );

                host.SaveUserConfig();
                host.SaveSystemConfig();
            }
            // Creates second user configuration file
            {
                var host = new TestContextHost( "ChangeEmptyToFilled" );
                var ctx = host.CreateContext();

                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ), Is.EqualTo( ConfigPluginStatus.AutomaticStart ),
                    "Accessing the UserConfiguration triggers the load of last active profile: we find back the good configuration." );

                // Creates a second profile.
                var p2 = ctx.ConfigManager.SystemConfiguration.UserProfiles.FindOrCreate( GetTestFileUri( "UsrConf2" ) );
                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.Count, Is.EqualTo( 2 ), "Our System configuration MUST now contains 2 profiles." );
                // Consider it as the last active one: it will be used by SaveUserConfig() without parameter.
                ctx.ConfigManager.SystemConfiguration.CurrentUserProfile = p2;
                
                // Change the plugin configuration.
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );

                host.SaveUserConfig();
                
                // Change the plugin configuration again and use the SaveUserConfig( u ) that creates and
                // activates a 3rd profile.
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Disabled );
                host.SaveUserConfig( GetTestFileUri( "UsrConf3" ), true );

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.Count, Is.EqualTo( 3 ), "Our System configuration MUST now contains 3 profiles." );
                // - The original, default one, where plugin id1 is AutomaticStart.
                // - The UsrConf2 where plugin id1 is Manual.
                // - The UsrConf3 where plugin id1 is Disabled.
                host.SaveSystemConfig();
            }

            // Reloading...
            {
                var host = new TestContextHost( "ChangeEmptyToFilled" );
                TestBase.DumpFileToConsole( host.SystemConfigAddress.LocalPath );
                var ctx = host.CreateContext();

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.Count, Is.EqualTo( 3 ), "The 3 profiles are here!" );
                Assert.That( ctx.ConfigManager.SystemConfiguration.CurrentUserProfile.Address, Is.EqualTo( GetTestFileUri( "UsrConf3" ) ), "The 3rd one is active." );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ), Is.EqualTo( ConfigPluginStatus.Disabled ), "Where id1 is disabled." );

                // To check that underlying objects instances do not change when we load another profile.
                int userConfigHashCode = ctx.ConfigManager.UserConfiguration.GetHashCode();
                int pluginStatusCollectionHashCode = ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode();

                IUriHistory p2 = ctx.ConfigManager.SystemConfiguration.UserProfiles.Find( GetTestFileUri( "UsrConf2" ) );
                host.LoadUserConfig( p2.Address );
                
                Assert.That( ctx.ConfigManager.SystemConfiguration.CurrentUserProfile == p2, "The last active profile has been updated by the Load." );

                Assert.That( ctx.ConfigManager.UserConfiguration.GetHashCode() == userConfigHashCode 
                    && ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode() == pluginStatusCollectionHashCode, "Instances are preserved (their content have changed)." );

                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.Count, Is.EqualTo( 3 ), "We have 3 plugins configured in profile n°2." );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual, "Plugin id1 is Manual in profile n°2." );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id2, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Disabled );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id3, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );
            }
        }


        Uri GetTestFileUri( string name )
        {
            return new Uri( Path.Combine( TestBase.AppFolder, "TestFileUri-" + name ) );
        }
    }
}
