#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Injections.cs) is part of CiviKey. 
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
using CK.Plugin.Hosting;
using NUnit.Framework;

namespace CK.Plugin.Runner.Apply
{
    [TestFixture]
    public class Injections : TestBase
    {
        MiniContext _ctx;

        PluginRunner PluginRunner { get { return _ctx.PluginRunner; } }

        [SetUp]
        public void Setup()
        {
            _ctx = MiniContext.CreateMiniContext( "Injections" );
        }

        [SetUp]
        [TearDown]
        public void Teardown()
        {
            TestBase.CleanupTestDir();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, bool startSucceed, bool stopSucceed, bool stopLaunchedOptionals, params Guid[] idToStart )
        {
            // Set a new user action --> start plugins
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Started );

            if( beforeStart != null ) beforeStart();

            // So apply the change
            Assert.That( PluginRunner.Apply( stopLaunchedOptionals ) == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( PluginRunner.Apply(stopLaunchedOptionals) == stopSucceed );

            if( afterStop != null ) afterStop();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, params Guid[] idToStart )
        {
            CheckStartStop( beforeStart, afterStart, beforeStop, afterStop, true, true, true, idToStart );
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, bool stopLaunchedOptionals, params Guid[] idToStart )
        {
            CheckStartStop( beforeStart, afterStart, beforeStop, afterStop, true, true, stopLaunchedOptionals, idToStart );
        }

        [Test]
        public void CheckInjection()
        {
            Guid id = new Guid( "{7E0A35E0-0A49-461A-BDC7-7C0083CC5DC9}" );
            Guid id2 = new Guid( "{0BE75C1D-BDAD-4782-9D47-95D91EF828D4}" );

            TestBase.CopyPluginToTestDir( "Injection.dll" );

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id2 ) ) );
            };

            Action afterStop = () =>
            {
                Assert.That( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( !PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id2 ) ) );
            };

            CheckStartStop( null, afterStart, null, null, true, id );
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

            PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id ) ) );
                Assert.That( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id2 ) ) );
                Assert.That( PluginRunner.IsPluginRunning( PluginRunner.Discoverer.FindPlugin( id3 ) ) );
            };

            CheckStartStop( null, afterStart, null, null, id );
        }

    }
}