using System;
using CK.Plugin.Config;
using CK.Plugin.Hosting;
using NUnit.Framework;

namespace CK.Plugin.Runner
{
    [TestFixture]
    public class RunningErrors : TestBase
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

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, bool startSucceed, bool stopSucceed, params Guid[] idToStart )
        {
            // Set a new user action --> start plugins
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Started );

            if( beforeStart != null ) beforeStart();

            // So apply the change
            Assert.That( PluginRunner.Apply() == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( PluginRunner.Apply() == stopSucceed );

            if( afterStop != null ) afterStop();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, params Guid[] idToStart )
        {
            CheckStartStop( beforeStart, afterStart, beforeStop, afterStop, true, true, idToStart );
        }

        [Test]
        /// <summary>
        /// Just try to start BuggyPluginC that throw an exception in its setup method.
        /// </summary>
        public void TrySetupBuggyPlugin()
        {
            Guid id = new Guid( "{73FC9CFD-213C-4EC6-B002-452646B9D225}" );

            TestBase.CopyPluginToTestDir( "BuggyServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action check = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, check, null, check, false, true, id );
        }

        [Test]
        /// <summary>
        /// Just try to start BuggyPluginC that throw an exception in its start method.
        /// </summary>
        public void TryStartBuggyPlugin()
        {
            Guid id = new Guid( "{FFB94881-4F59-4B97-B16E-CF3081A6E668}" );

            TestBase.CopyPluginToTestDir( "BuggyServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action check = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, check, null, check, false, true, id );
        }
    }
}