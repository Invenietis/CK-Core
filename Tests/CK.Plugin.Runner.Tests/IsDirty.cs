using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Context;
using CK.Plugin.Hosting;

namespace CK.Plugin.Runner
{
    [TestFixture]
    public class IsDirty : TestBase
    {
        IContext _ctx;

        [SetUp]
        public void Setup()
        {
            _ctx = CK.Context.Context.CreateInstance();
            Assert.That( _ctx.GetService( typeof( ISimplePluginRunner ) ) != null );
        }
        
        [Test]
        public void SimpleWithoutDiscoverer()
        {
            Guid id = Guid.NewGuid();

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );
            
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            Assert.That( !runner.IsDirty, "Not dirty since the plugin is not found in the discoverer: it is Disabled." );
            runner.Remove( layer );
            Assert.That( !runner.IsDirty, "Not dirty because the runner doesn't contains any requirement" );
        }

        [Test]
        public void SimpleRequirementLayer()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            Assert.That( runner.IsDirty, "Dirty because the plugin has been found in the discoverer." );
            
            Assert.IsTrue( runner.Remove( layer ) );
            Assert.That( implRunner.RunnerRequirements.Count, Is.EqualTo(0) );
            
            Assert.That( !runner.IsDirty, "Not dirty because the runner doesn't contains any requirement" );
        }

        [Test]
        public void RequirementLayerOptionals()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            layer.PluginRequirements.AddOrSet( id_plugin2, RunningRequirement.Optional );
            Assert.That( !runner.IsDirty, "Not dirty because plugin's are optional ... so we have nothing to change." );

            Assert.IsTrue( runner.Remove( layer ) );
            Assert.That( implRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( !runner.IsDirty, "Not dirty because the runner doesn't contains any requirement" );
        }

        [Test]
        public void RequirementLayerUnknownService()
        {
            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.ServiceRequirements.AddOrSet( "UnknownAQN!", RunningRequirement.Optional );
            Assert.That( !runner.IsDirty, "Should not be dirty because the service is unknown and the requirement is optional" );
        }

        /// <summary>
        /// First way : add the requirement layer then insert requirement into it.
        /// </summary>
        [Test]
        public void RequirementLayerService_AddThenInsert()
        {
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.ServiceRequirements.AddOrSet( "CK.Tests.Plugin.IServiceA, ServiceA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", RunningRequirement.MustExistAndRun );
            Assert.That( runner.IsDirty, "Should be dirty" );
        }

        /// <summary>
        /// Second way : add the requirement layer already filled.
        /// </summary>
        [Test]
        public void RequirementLayerService_AddFilled()
        {
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.ServiceRequirements.AddOrSet( "CK.Tests.Plugin.IServiceA, ServiceA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", RunningRequirement.MustExistAndRun );
            runner.Add( layer );
            Assert.That( runner.IsDirty, "Should be dirty" );
        }

        [Test]
        public void SimpleLiveUserConfig()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            Assert.That( !runner.IsDirty, "Still not dirty because plugin's are optional ... so we have nothing to change." );

            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id_plugin2, Config.ConfigUserAction.Started );
            Assert.That( runner.IsDirty );

            Assert.IsTrue( runner.Remove( layer ) );
            Assert.That( implRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( runner.IsDirty, "Still dirty because of the live user" );
        }

        [Test]
        public void LiveUserConfigOptional()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );
            Guid id_plugin2 = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            runner.Add( layer );

            Assert.That( !runner.IsDirty, "Not dirty because the layer is empty" );

            layer.PluginRequirements.AddOrSet( id_plugin1, RunningRequirement.Optional );
            Assert.That( !runner.IsDirty, "Still not dirty because plugin's are optional ... so we have nothing to change." );

            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id_plugin2, Config.ConfigUserAction.None );
            Assert.That( !runner.IsDirty );

            Assert.IsTrue( runner.Remove( layer ) );
            Assert.That( implRunner.RunnerRequirements.Count, Is.EqualTo( 0 ) );

            Assert.That( !runner.IsDirty, "Not dirty because of the live user" );
        }        

        [Test]
        public void UserConfigPluginStatus()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.Disabled );
            Assert.That( runner.IsDirty, "Even if the plugin is not running, the disabled configuration is a change." );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
            Assert.That( runner.IsDirty );
            Assert.That( runner.Apply() );
            Assert.That( !runner.IsDirty );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.Disabled );
            Assert.That( runner.IsDirty );
        }

        [Test]
        public void SystemConfigDisabled()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.Disabled );

            Assert.That( runner.IsDirty, "Even if the plugin is not running, the disabled configuration is a change." );
        }

        [Test]
        public void SystemConfigCleared()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            // Specific clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( !runner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear();
                Assert.That( !runner.IsDirty );
            }
        }

        [Test]
        public void UserConfigCleared()
        {
            Guid id_plugin1 = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            // Specific clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( !runner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear();
                Assert.That( !runner.IsDirty );
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

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin2, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear( id_plugin1 );
                Assert.That( runner.IsDirty, "Still dirty because of the system configuration" );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear( id_plugin2 );
                Assert.That( !runner.IsDirty );
            }
            // Global clear
            {
                TestBase.CopyPluginToTestDir( "ServiceA.dll" );
                TestBase.CopyPluginToTestDir( "ServiceB.dll" );

                ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
                PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

                implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id_plugin1, Config.ConfigPluginStatus.AutomaticStart );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id_plugin2, Config.ConfigPluginStatus.AutomaticStart );
                Assert.That( runner.IsDirty );

                _ctx.ConfigManager.UserConfiguration.PluginsStatus.Clear();
                Assert.That( runner.IsDirty, "Still dirty because of the system configuration" );
                _ctx.ConfigManager.SystemConfiguration.PluginsStatus.Clear();
                Assert.That( !runner.IsDirty );
            }
        }
    }
}
