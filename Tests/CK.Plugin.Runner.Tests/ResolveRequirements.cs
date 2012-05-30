#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\ResolveRequirements.cs) is part of CiviKey. 
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
