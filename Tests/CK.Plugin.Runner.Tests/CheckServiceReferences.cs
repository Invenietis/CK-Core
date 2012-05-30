#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\CheckServiceReferences.cs) is part of CiviKey. 
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
    [TestFixture]
    public class CheckServiceReferences
    {
        MiniContext _ctx;

        PluginRunner PluginRunner { get { return _ctx.PluginRunner; } }
        IConfigManager ConfigManager { get { return _ctx.ConfigManager; } }

        static Guid _implService = new Guid( "{C24EE3EA-F078-4974-A346-B34208221B35}" );

        [SetUp]
        public void Setup()
        {
            _ctx = MiniContext.CreateMiniContext( "BasicStartStop" );
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
                ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], ConfigUserAction.Started );

            if( beforeStart != null ) beforeStart();

            // So apply the change
            Assert.That( PluginRunner.Apply() == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            for( int i = 0; i < idToStart.Length; i++ )
                ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( PluginRunner.Apply() == stopSucceed );

            if( afterStop != null ) afterStop();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, params Guid[] idToStart )
        {
            CheckStartStop( beforeStart, afterStart, beforeStop, afterStop, true, true, idToStart );
        }

        void CheckStartAnotherStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, bool startSucceed, bool stopSucceed, Guid idToStart, Guid idToStop )
        {
            // Set a new user action --> start plugins
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart, Config.ConfigUserAction.Started );

            if( beforeStart != null ) beforeStart();

            // So apply the change
            Assert.That( PluginRunner.Apply() == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStop, Config.ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( PluginRunner.Apply() == stopSucceed );

            if( afterStop != null ) afterStop();
        }

        #region Check all types of service references with fully implemented service.

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistAndRun()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// Start the that needs the service. And then stop the service. Check that the plugin is stopped.
        /// </summary>
        public void ServiceReference_Normal_MustExistAndRun_ThenStopService()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartAnotherStop( null, afterStart, null, afterStart, true, false, id, _implService );

            // Then we try to stop the plugin (the one that needs the service)
            _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id, Config.ConfigUserAction.Stopped );
            Assert.That( PluginRunner.Apply() );
            Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistAndRun()
        {
            Guid id = new Guid( "{457E357D-102D-447D-89B8-DA9C849910C8}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// Start the that needs the service. And then stop the service. Check that the plugin is stopped.
        /// </summary>
        public void ServiceReference_IService_MustExistAndRun_ThenStopService()
        {
            Guid id = new Guid( "{457E357D-102D-447D-89B8-DA9C849910C8}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartAnotherStop( null, afterStart, null, afterStop, true, false, id, _implService );
        }


        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistTryStart()
        {
            #region Init
            
            Guid id = new Guid( "{58C00B79-D882-4C11-BD90-1F25AD664C67}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin is available ... so we tried to start it.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistTryStart()
        {
            #region Init
            Guid id = new Guid( "{9BBCFE92-7465-4B3B-88D0-3CEF1E2E5580}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true ); 
            #endregion

            #region Asserts
            Action afterStart = () =>
                {
                    // Check if the plugin is started, and if the plugin that implement the required service is started too.
                    Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                    // The plugin is available ... so we tried to start it.
                    Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
                };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            }; 
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExist()
        {
            #region Init

            Guid id = new Guid( "{317B5D34-BA84-4A15-92F4-4E791E737EF0}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin is available but we don't need it started.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service was not running.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExist()
        {
            #region Init
            Guid id = new Guid( "{973B4050-280F-43B0-A9E3-0C4DC9BC2C5F}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_OptionalTryStart()
        {
            #region Init

            Guid id = new Guid( "{ABD53A18-4549-49B8-82C0-9977200F47E9}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_OptionalTryStart()
        {
            #region Init
            Guid id = new Guid( "{CDCE6413-038D-4020-A3E0-51FA755C5E72}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_Optional()
        {
            #region Init

            Guid id = new Guid( "{C78FCB4F-6925-4587-AC98-DA0AE1A977D1}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_Optional()
        {
            #region Init
            Guid id = new Guid( "{FF896081-A15D-4A5C-8030-13544EF09673}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        #endregion

        #region Check all types of service references with not implemented service.

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistAndRun_Error()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistAndRun_Error()
        {
            Guid id = new Guid( "{457E357D-102D-447D-89B8-DA9C849910C8}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistTryStart_Error()
        {
            #region Init

            Guid id = new Guid( "{58C00B79-D882-4C11-BD90-1F25AD664C67}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistTryStart_Error()
        {
            #region Init
            Guid id = new Guid( "{9BBCFE92-7465-4B3B-88D0-3CEF1E2E5580}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExist_Error()
        {
            #region Init

            Guid id = new Guid( "{317B5D34-BA84-4A15-92F4-4E791E737EF0}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExist_Error()
        {
            #region Init
            Guid id = new Guid( "{973B4050-280F-43B0-A9E3-0C4DC9BC2C5F}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action after = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, after, null, after, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_OptionalTryStart_Error()
        {
            #region Init

            Guid id = new Guid( "{ABD53A18-4549-49B8-82C0-9977200F47E9}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_OptionalTryStart_Error()
        {
            #region Init
            Guid id = new Guid( "{CDCE6413-038D-4020-A3E0-51FA755C5E72}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_Optional_Error()
        {
            #region Init

            Guid id = new Guid( "{C78FCB4F-6925-4587-AC98-DA0AE1A977D1}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_Optional_Error()
        {
            #region Init
            Guid id = new Guid( "{FF896081-A15D-4A5C-8030-13544EF09673}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        #endregion
    }
}