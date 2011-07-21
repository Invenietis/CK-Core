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
    public class ResolveConfiguration : TestBase
    {
        IContext _ctx;

        [SetUp]
        public void Setup()
        {
            _ctx = CK.Context.Context.CreateInstance();
            Assert.That( _ctx.GetService( typeof( ISimplePluginRunner ) ) != null );
        }

        [Test]
        public void LiveUserActions()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
        }

        [Test]
        public void UserPluginStatus()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );           
        }

        [Test]
        public void SystemPluginStatus()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
        }

        [Test]
        public void AggregateUserAndSystem()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            // Note : Comment pattern is like this
            //  ----> pattern : [system] + [user] = [solved status]

            #region System disabled
            
            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            
            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            
            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // Disabled + AutomaticStart = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Manual = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Disabled = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region System AutomaticStart

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // AutomaticStart + AutomaticStart = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Manual = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Disabled = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region System Manual

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );

            _ctx.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // Manual + AutomaticStart = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // Manual + Manual = Optional
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            // Manual + Disabled = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion
        }

        [Test]
        public void AggregateUserAndLiveUser()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            // Note : Comment pattern is like this
            //  ----> pattern : [user] + [liveUser] = [solved status]

            #region User disabled

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // Disabled + Started = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Stopped = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + None = Disabled
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region User AutomaticStart

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.AutomaticStart );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // AutomaticStart + Started = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Stopped = Optional
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // AutomaticStart + None = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );

            #endregion

            #region User Manual

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            _ctx.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // Manual + Started = MustExistAndRun
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // Manual + Stopped = Optional
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Manual + None = Optional
            Assert.That( _ctx.ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Optional ) );

            #endregion
        }
    }
}
