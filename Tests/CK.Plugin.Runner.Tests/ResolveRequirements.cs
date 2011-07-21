using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Context;
using CK.Plugin.Hosting;
using CK.Plugin.Config;

namespace CK.Plugin.Runner.ConfigurationAndRequirements
{
    /// <summary>
    /// Basic tests that just test aggregation and transtyping between ConfigUserAction/ConfigPluginStatus to SolvedConfigStatus
    /// </summary>
    [TestFixture]
    public class ResolveRequirements : TestBase
    {
        IContext _ctx;

        [SetUp]
        public void Setup()
        {
            _ctx = CK.Context.Context.CreateInstance();
            Assert.That( _ctx.GetService( typeof( ISimplePluginRunner ) ) != null );
        }

        [Test]
        public void MergeOnlyRequirements()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );
            RequirementLayer layer3 = new RequirementLayer( "MyLayer3" );
            layer3.PluginRequirements.AddOrSet( id, RunningRequirement.OptionalTryStart );

            runner.Add( layer );
            runner.Add( layer2 );
            runner.Add( layer3 );

            Assert.That( implRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfig()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to AutomaticStart
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );

            runner.Add( layer );
            runner.Add( layer2 );

            Assert.That( implRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfigAndLiveUserActions()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to Manual
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.Manual );
            // and we set the LiveUserAction to started
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );

            runner.Add( layer );
            runner.Add( layer2 );

            Assert.That( implRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfigDisabledAndLiveUserActions()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            ISimplePluginRunner runner = _ctx.GetService<ISimplePluginRunner>();
            PluginRunner implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to Disabled
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.Disabled );
            // and we set the LiveUserAction to started
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );

            runner.Add( layer );
            runner.Add( layer2 );

            Assert.That( implRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.Disabled );
        }
    }
}
