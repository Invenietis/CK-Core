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

namespace CK.Plugin.Runner.Apply
{
    [TestFixture]
    public class BasicStartStop : TestBase
    {
        IContext _ctx;
        ISimplePluginRunner _runner;
        PluginRunner _implRunner;
        Guid _implService = new Guid( "{C24EE3EA-F078-4974-A346-B34208221B35}" );

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
        /// Start stop a very simple plugin.
        /// Just check if the plugin can be started and stopped.
        /// </summary>
        public void SimplePlugin()
        {
            Guid id = new Guid( "{EEAEC976-2AFC-4A68-BFAD-68E169677D52}" );

            TestBase.CopyPluginToTestDir( "SimplePlugin.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () => Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            Action afterStop = () => Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            CheckStartStop( null, afterStart, null, afterStop, id );

            // Second start/stop to try live user actions.
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// Start a simple plugin With SystemConf set to AutomaticStart
        /// Change its pluginStatus to Manual
        /// Apply
        /// Check that is has not been stopped
        /// </summary>
        public void SimplePluginStatusSwitching()
        {
            Guid id = new Guid( "{EEAEC976-2AFC-4A68-BFAD-68E169677D52}" );

            TestBase.CopyPluginToTestDir( "SimplePlugin.dll" );
            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            _implRunner.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id, Config.ConfigPluginStatus.AutomaticStart );
            _implRunner.Apply();
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            _implRunner.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id, Config.ConfigPluginStatus.Manual );
            _implRunner.Apply();
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
        }

         [Test]
        /// <summary>
        /// Start a plugin that has a serviceRef With SystemConf set to  AutomaticStart
        /// Change its pluginStatus to Manual
        /// Apply
        /// Check that is has not been stopped
        /// </summary>
        public void RefPluginStatusSwitching()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );
            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            _implRunner.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id, Config.ConfigPluginStatus.AutomaticStart );
            _implRunner.Apply();
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            _implRunner.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id, Config.ConfigPluginStatus.Manual );
            _implRunner.Apply();
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
        }

         [Test]
         // Two plugins : 1, that requires 2 as MustExistAndRun
         // 1 is launched (SetAction("Started")) therefor, 2 is launched automatically
         // 1 is stopped (SetAction("Started")) 2 stays launched
         // 2 is stopped SetAction("stopped"). It should stop
         public void RefPluginLiveAction()
         {
             Guid id1 = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" ); //called 1 in the commentary
             Guid id2 = new Guid( "{C24EE3EA-F078-4974-A346-B34208221B35}" ); //called 2 in the commentary

             TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
             TestBase.CopyPluginToTestDir( "ServiceC.dll" );
             TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );
             _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id1 ) ) || _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ), Is.False, "Nothing started yet." );

             _implRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );
             Assert.That( _implRunner.IsDirty );
             _implRunner.Apply();
             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id1 ) ), "Both should be launched as we launch 1 that needs 2" );
             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ), "Both should be launched as we launch 1 that needs 2" );

             _implRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Stopped );
             Assert.That( _implRunner.IsDirty );
             _implRunner.Apply();
             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id1 ) ), Is.False, "The plugin should be stopped" );
             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ), Is.True, "The plugin should still be running" );

             _implRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );
             Assert.That( _implRunner.IsDirty );
             _implRunner.Apply();             
             Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id2 ) ), Is.False, "The plugin should now be stopped" );

         }

        [Test]
        /// <summary>
        /// Check if a plugin that implement a service can be started/stopped and if the service is available
        /// </summary>
        public void SimpleImplementService()
        {
            Guid id = new Guid( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}" );

            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // Check that the service is available.
                //Assert.NotNull( _implRunner.ServiceHost.GetRunningProxy( Type.GetType( "CK.Tests.Plugin.IServiceB, ServiceB, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true ) ) );
            };
            Action afterStop = () => Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        public void StartKeyboardPlugin()
        {
            Guid id = new Guid( "{2ED1562F-2416-45cb-9FC8-EEF941E3EDBC}" );

            TestBase.CopyPluginToTestDir( TestBase.AppFolderDir.GetFiles( "CK.Keyboard.*" ) );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () => Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            CheckStartStop( null, afterStart, null, null, id );
        }

        [Test]
        public void TestComplexInterfaces()
        {
            Guid id = new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () => Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );

            CheckStartStop( null, afterStart, null, null, id );
        }

        [Test]
        public void RediscoverImplementationOfAService()
        {
            Guid serviceC = new Guid( "{C24EE3EA-F078-4974-A346-B34208221B35}" );
            Guid serviceC2 = new Guid( "{1EC4980D-17F0-4DDC-86C6-631CDB69A6AD}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );   // Service
            TestBase.CopyPluginToTestDir( "ServiceC.dll" );         // Implementation

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            RequirementLayer layer = new RequirementLayer( "RunnerTest" );
            layer.ServiceRequirements.AddOrSet( "CK.Tests.Plugin.IServiceC, ServiceC.Model", RunningRequirement.MustExistAndRun );
            _implRunner.RunnerRequirements.Add( layer );

            Assert.That( _implRunner.Apply() );
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( serviceC ) ) );

            TestBase.CopyPluginToTestDir( "ServiceC.2.dll" );  // Second Implementation
            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( _implRunner.IsDirty, Is.False );
            _implRunner.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( serviceC, ConfigPluginStatus.Disabled );
            Assert.That( _implRunner.IsDirty );
            
            Assert.That( _implRunner.Apply() );
            Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( serviceC2 ) ) );
        }
    }
}