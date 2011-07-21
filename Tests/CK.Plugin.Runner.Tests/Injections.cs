using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Context;
using CK.Plugin.Hosting;
using System.Reflection;
using CK.Plugin.Config;
using CK.SharedDic;

namespace CK.Plugin.Runner.Apply
{
    [TestFixture]
    public class Injections : TestBase
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
        public void CheckInjection()
        {
            Guid id = new Guid( "{7E0A35E0-0A49-461A-BDC7-7C0083CC5DC9}" );
            Guid id2 = new Guid( "{0BE75C1D-BDAD-4782-9D47-95D91EF828D4}" );

            TestBase.CopyPluginToTestDir( "Injection.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ) );
            };

            Action beforeStop = () =>
            {
                CheckStartStop( null, null, null, null, id2 );
            };
            Action afterStop = () =>
            {
                Assert.That( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ) );
            };

            CheckStartStop( null, afterStart, beforeStop, null, id );
        }

        /// <summary>
        /// P1 needs S2 implemented by P2, that needs S3 implemented by P3, that needs S1 implemented by P1 :)
        /// </summary>
        [Test]
        public void CheckCircleReferences()
        {
            Guid id = new Guid( "{6DCB0BB5-5843-4F48-9FBD-5A0FAD2C8157}" );
            Guid id2 = new Guid( "{CD20AB53-6D77-41B8-BC8A-5D95519B1094}" );
            Guid id3 = new Guid( "{0AF439FE-1562-4BE4-8AAC-D009D1E75BD0}" );

            TestBase.CopyPluginToTestDir( "Injection.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ) );
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id3 ) ) );
            };

            CheckStartStop( null, afterStart, null, null, id );
        }

        [Test]
        public void ContextInjection()
        {
            Guid id = new Guid( "{87AA1820-6576-4090-AC63-2A165A485AB0}" );

            TestBase.CopyPluginToTestDir( "PluginA.dll" );

            Assert.That( _ctx.GetService<IContext>() != null );
            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, afterStart, null, null, id );
        }
    }
}