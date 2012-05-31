#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\IsDirty.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugin.Hosting;
using NUnit.Framework;

namespace CK.Plugin.Runner
{
    [TestFixture]
    public class IsDirty
    {
        MiniContext _ctx;

        PluginRunner PluginRunner { get { return _ctx.PluginRunner; } }
        IConfigManager ConfigManager { get { return _ctx.ConfigManager; } }

        [SetUp]
        public void Setup()
        {
            _ctx = MiniContext.CreateMiniContext( "IsDirty" );
        }

        [SetUp]
        [TearDown]
        public void Teardown()
        {
            TestBase.CleanupTestDir();
        }


        [Test]
        public void SimpleWithoutDiscoverer()
        {
            Guid id = Guid.NewGuid();

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );
            
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            Assert.That( !PluginRunner.IsDirty, "Not dirty since the plugin is not found in the discoverer: it is Disabled." );
            PluginRunner.Remove( layer );
            Assert.That( !PluginRunner.IsDirty, "Not dirty because the PluginRunner doesn't contains any requirement" );
        }

        [Test]
        public void SimpleRequirementLayer()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            Assert.That( PluginRunner.IsDirty, "Dirty because the plugin has been found in the discoverer." );

            Assert.IsTrue( PluginRunner.Remove( layer ) );
            Assert.That( PluginRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the PluginRunner doesn't contains any requirement" );
        }

        [Test]
        public void RequirementLayerOptionals()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            layer.PluginRequirements.AddOrSet( id_plugin2, RunningRequirement.Optional );
            Assert.That( !PluginRunner.IsDirty, "Not dirty because plugin's are optional ... so we have nothing to change." );

            Assert.IsTrue( PluginRunner.Remove( layer ) );
            Assert.That( PluginRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the PluginRunner doesn't contains any requirement" );
        }

        [Test]
        public void RequirementLayerUnknownService()
        {
            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.ServiceRequirements.AddOrSet( "UnknownAQN!", RunningRequirement.Optional );
            Assert.That( !PluginRunner.IsDirty, "Should not be dirty because the service is unknown and the requirement is optional" );
        }

        /// <summary>
        /// First way : add the requirement layer then insert requirement into it.
        /// </summary>
        [Test]
        public void RequirementLayerService_AddThenInsert()
        {
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.ServiceRequirements.AddOrSet( "CK.Tests.Plugin.IServiceA, ServiceA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", RunningRequirement.MustExistAndRun );
            Assert.That( PluginRunner.IsDirty, "Should be dirty" );
        }

        /// <summary>
        /// Second way : add the requirement layer already filled.
        /// </summary>
        [Test]
        public void RequirementLayerService_AddFilled()
        {
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.ServiceRequirements.AddOrSet( "CK.Tests.Plugin.IServiceA, ServiceA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", RunningRequirement.MustExistAndRun );
            PluginRunner.Add( layer );
            Assert.That( PluginRunner.IsDirty, "Should be dirty" );
        }

        [Test]
        public void SimpleLiveUserConfig()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            Assert.That( !PluginRunner.IsDirty, "Still not dirty because plugin's are optional ... so we have nothing to change." );

            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id_plugin2, ConfigUserAction.Started );
            Assert.That( PluginRunner.IsDirty );

            Assert.IsTrue( PluginRunner.Remove( layer ) );
            Assert.That( PluginRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( PluginRunner.IsDirty, "Still dirty because of the live user" );
        }

        [Test]
        public void LiveUserConfigOptional()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            PluginRunner.Add( layer );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            Assert.That( !PluginRunner.IsDirty, "Still not dirty because plugin's are optional ... so we have nothing to change." );

            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id_plugin2, ConfigUserAction.None );
            Assert.That( !PluginRunner.IsDirty );

            Assert.IsTrue( PluginRunner.Remove( layer ) );
            Assert.That( PluginRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( !PluginRunner.IsDirty, "Not dirty because of the live user" );
        }        

        [Test]
        public void UserConfigPluginStatus()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.Disabled );
            Assert.That( PluginRunner.IsDirty, "Even if the plugin is not running, the disabled configuration is a change." );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
            Assert.That( PluginRunner.IsDirty );
            Assert.That( PluginRunner.Apply() );
            Assert.That( !PluginRunner.IsDirty );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.Disabled );
            Assert.That( PluginRunner.IsDirty );
        }

        [Test]
        public void SystemConfigDisabled()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.Disabled );

            Assert.That( PluginRunner.IsDirty, "Even if the plugin is not running, the disabled configuration is a change." );
        }

        [Test]
        public void SystemConfigCleared()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            // Specific clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( !PluginRunner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear();
                Assert.That( !PluginRunner.IsDirty );
            }
        }

        [Test]
        public void UserConfigCleared()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            // Specific clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( !PluginRunner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear();
                Assert.That( !PluginRunner.IsDirty );
            }
        }

        [Test]
        public void UserAndSystemConfigCleared()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            // Specific clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );
                TestBase.CopyPluginToTestDir( "ServiceB.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin2, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( PluginRunner.IsDirty, "Still dirty because of the system configuration" );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear( id_plugin2 );
                Assert.That( !PluginRunner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );
                TestBase.CopyPluginToTestDir( "ServiceB.dll" );

                PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, ConfigPluginStatus.AutomaticStart );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin2, ConfigPluginStatus.AutomaticStart );
                Assert.That( PluginRunner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear();
                Assert.That( PluginRunner.IsDirty, "Still dirty because of the system configuration" );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear();
                Assert.That( !PluginRunner.IsDirty );
            }
        }
    }
}
