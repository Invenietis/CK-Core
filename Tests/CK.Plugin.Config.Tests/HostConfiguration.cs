#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\HostConfiguration.cs) is part of CiviKey. 
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
using System.IO;
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
                    config.LoadUserConfig( sr );
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
            var ctx = MiniContext.CreateMiniContext( "DirtyUserFinalDictionnary" );
            
            IConfigManagerExtended config = ctx.ConfigManager.Extended;
            Assert.IsNotNull( config );
            Assert.That( config.ConfigManager.UserConfiguration != null );

            Assert.That( ctx.HostUserConfig != null );

            Assert.That( !config.IsUserConfigDirty );
            ctx.HostUserConfig.GetOrSet( "key2", "value2" );
            Assert.That( config.IsUserConfigDirty );

            Assert.That( ctx.HostUserConfig.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void DirtySystemFinalDictionnary()
        {
            var ctx = MiniContext.CreateMiniContext( "DirtySystemFinalDictionnary" );

            IConfigManagerExtended config = ctx.ConfigManager.Extended;
            Assert.IsNotNull( config );

            Assert.That( config.ConfigManager.SystemConfiguration != null );

            Assert.That( ctx.HostSystemConfig != null );
            Assert.That( !config.IsSystemConfigDirty );
            Assert.That( !config.IsUserConfigDirty );

            ctx.HostSystemConfig.GetOrSet( "key2", "value2" );

            Assert.That( config.IsSystemConfigDirty );
            Assert.That( !config.IsUserConfigDirty );

            Assert.That( ctx.HostSystemConfig.Count, Is.EqualTo( 1 ) );
        }

    }
}
