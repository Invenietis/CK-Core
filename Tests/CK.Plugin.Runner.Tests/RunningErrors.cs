using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Context;
using CK.Plugin.Hosting;
using System.Reflection;

namespace CK.Plugin.Runner.Apply
{
    [TestFixture]
    public class RunningErrors : TestBase
    {
        IContext _ctx;
        ISimplePluginRunner _runner;
        PluginRunner _implRunner;

        [SetUp]
        public void Setup()
        {
            _ctx = CK.Context.Context.CreateInstance();
            _runner = _ctx.GetService<ISimplePluginRunner>();
            _implRunner = (PluginRunner)_ctx.GetService<ISimplePluginRunner>();

            Assert.NotNull( _runner );
            Assert.NotNull( _implRunner );
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
            Assert.That( _runner.Apply() == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( _runner.Apply() == stopSucceed );

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

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action check = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
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

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action check = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, check, null, check, false, true, id );
        }
    }
}