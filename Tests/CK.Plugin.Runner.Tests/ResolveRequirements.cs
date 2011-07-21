using System;
using CK.Plugin.Config;
using CK.Plugin.Hosting;
using NUnit.Framework;

namespace CK.Plugin.Runner
{
    /// <summary>
    /// Basic tests that just test aggregation and transtyping 
    /// between ConfigUserAction/ConfigPluginStatus to SolvedConfigStatus
    /// </summary>
    [TestFixture]
    public class ResolveRequirements
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
        public void MergeOnlyRequirements()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistAndRun );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );
            RequirementLayer layer3 = new RequirementLayer( "MyLayer3" );
            layer3.PluginRequirements.AddOrSet( id, RunningRequirement.OptionalTryStart );

            PluginRunner.Add( layer );
            PluginRunner.Add( layer2 );
            PluginRunner.Add( layer3 );

            Assert.That( PluginRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfig()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to AutomaticStart
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );

            PluginRunner.Add( layer );
            PluginRunner.Add( layer2 );

            Assert.That( PluginRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfigAndLiveUserActions()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to Manual
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.Manual );
            // and we set the LiveUserAction to started
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );

            PluginRunner.Add( layer );
            PluginRunner.Add( layer2 );

            Assert.That( PluginRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.MustExistAndRun );
        }

        [Test]
        public void MergeRequirementsWithConfigDisabledAndLiveUserActions()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "MyLayer" );
            layer.PluginRequirements.AddOrSet( id, RunningRequirement.MustExistTryStart );
            RequirementLayer layer2 = new RequirementLayer( "MyLayer2" );
            layer2.PluginRequirements.AddOrSet( id, RunningRequirement.Optional );

            // the requirements needs a MustExistTryStart, we set the status of the plugin to Disabled
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id, ConfigPluginStatus.Disabled );
            // and we set the LiveUserAction to started
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );

            PluginRunner.Add( layer );
            PluginRunner.Add( layer2 );

            Assert.That( PluginRunner.RunnerRequirements.FinalRequirement( id ) == SolvedConfigStatus.Disabled );
        }
    }
}
