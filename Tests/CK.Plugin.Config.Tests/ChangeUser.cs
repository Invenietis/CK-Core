#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\ChangeUser.cs) is part of CiviKey. 
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
using CK.Context;
using CK.Plugin.Config;
using CK.Core;
using CK.SharedDic;
using CK.Storage;

namespace PluginConfig
{
    [TestFixture]
    public class ChangeUser : TestBase
    {
        [TearDown]
        public void TearDown()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void ChangeEmptyToEmpty()
        {
            // Creates empty configuration files.
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;
                File.Delete( Host.DefaultUserConfigPath );
                Assert.That( !File.Exists( Host.DefaultUserConfigPath ) );

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 0 ); // there is no system configuration, so no user configuration.

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "TestProfile", GetTestFilePath( "UserConfiguration" ), ConfigSupportType.File, true );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Creates second user configuration file
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "SecondTestProfile", GetTestFilePath( "UserConfiguration2" ), ConfigSupportType.File, true );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Reload last user profile (userConfiguration2), and change the user to userconfiguration normal
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile.Name == "SecondTestProfile" );
                
                int userConfigHashCode = ctx.ConfigManager.UserConfiguration.GetHashCode();
                int pluginStatusCollectionHashCode = ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode();

                IUserProfile profile1 = ctx.ConfigManager.SystemConfiguration.UserProfiles.Find( GetTestFilePath( "UserConfiguration" ) );
                Assert.That( Host.LoadUserConfigFromFile( profile1 ) );
                Assert.That( userChanged == 2 );

                Assert.That( ctx.ConfigManager.UserConfiguration.GetHashCode() == userConfigHashCode );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode() == pluginStatusCollectionHashCode );
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
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;
                File.Delete( Host.DefaultUserConfigPath );
                Assert.That( !File.Exists( Host.DefaultUserConfigPath ) );

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 0 ); // there is no system configuration, so no user configuration.

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "TestProfile", GetTestFilePath( "UserConfiguration" ), ConfigSupportType.File, true );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Creates second user configuration file
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "SecondTestProfile", GetTestFilePath( "UserConfiguration2" ), ConfigSupportType.File, true );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Reload last user profile (userConfiguration2), and change the user to userconfiguration normal
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile.Name == "SecondTestProfile" );

                int userConfigHashCode = ctx.ConfigManager.UserConfiguration.GetHashCode();
                int pluginStatusCollectionHashCode = ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode();

                IUserProfile profile1 = ctx.ConfigManager.SystemConfiguration.UserProfiles.Find( GetTestFilePath( "UserConfiguration" ) );
                Assert.That( Host.LoadUserConfigFromFile( profile1 ) );
                Assert.That( userChanged == 2 );
                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile == profile1 );

                Assert.That( ctx.ConfigManager.UserConfiguration.GetHashCode() == userConfigHashCode );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode() == pluginStatusCollectionHashCode );

                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.Count == 3 );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.AutomaticStart );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id2, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Disabled );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id3, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );
            }
        }

        [Test]
        public void ChangeFilledToFilled()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            // Creates empty configuration files.
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;
                File.Delete( Host.DefaultUserConfigPath );
                Assert.That( !File.Exists( Host.DefaultUserConfigPath ) );

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 0 ); // there is no system configuration, so no user configuration.

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "TestProfile", GetTestFilePath( "UserConfiguration" ), ConfigSupportType.File, true );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Creates second user configuration file
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "SecondTestProfile", GetTestFilePath( "UserConfiguration2" ), ConfigSupportType.File, true );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
                ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );

                Host.SaveUserConfig();
                Host.SaveSystemConfig();
            }
            // Reload last user profile (userConfiguration2), and change the user to userconfiguration normal
            {
                int userChanged = 0;
                IContext ctx = CreateContext();
                Host.CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration" );
                ctx.ConfigManager.UserChanged += ( o, e ) => userChanged++;

                Assert.That( ctx.ConfigManager.UserConfiguration != null );

                Assert.That( userChanged == 1 );

                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile.Name == "SecondTestProfile" );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.Count == 3 );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id2, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id3, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );

                int userConfigHashCode = ctx.ConfigManager.UserConfiguration.GetHashCode();
                int pluginStatusCollectionHashCode = ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode();

                IUserProfile profile1 = ctx.ConfigManager.SystemConfiguration.UserProfiles.Find( GetTestFilePath( "UserConfiguration" ) );
                Assert.That( Host.LoadUserConfigFromFile( profile1 ) );
                Assert.That( userChanged == 2 );
                Assert.That( ctx.ConfigManager.SystemConfiguration.UserProfiles.LastProfile == profile1 );

                Assert.That( ctx.ConfigManager.UserConfiguration.GetHashCode() == userConfigHashCode );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetHashCode() == pluginStatusCollectionHashCode );

                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.Count == 3 );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id1, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.AutomaticStart );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id2, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Disabled );
                Assert.That( ctx.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( id3, ConfigPluginStatus.ConfigurationMask ) == ConfigPluginStatus.Manual );
            }
        }
    }
}
