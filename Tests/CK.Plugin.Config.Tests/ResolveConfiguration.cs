#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\ResolveConfiguration.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Plugin.Config;
using CK.Plugin;

namespace PluginConfig
{
    /// <summary>
    /// Basic tests that just test aggregation and transtyping between ConfigUserAction/ConfigPluginStatus to SolvedConfigStatus
    /// </summary>
    [TestFixture]
    public class ResolveConfiguration
    {
        MiniContext _ctx;

        IConfigManager ConfigManager { get { return _ctx.ConfigManager; } }

        [SetUp]
        public void Setup()
        {
            _ctx = MiniContext.CreateMiniContext( "ResolveConfiguration" );
        }

        [Test]
        public void LiveUserActions()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
        }

        [Test]
        public void UserPluginStatus()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );           
        }

        [Test]
        public void SystemPluginStatus()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
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
            
            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            
            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            
            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // Disabled + AutomaticStart = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Manual = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Disabled = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region System AutomaticStart

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // AutomaticStart + AutomaticStart = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Manual = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Disabled = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region System Manual

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );

            ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );

            // Manual + AutomaticStart = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // Manual + Manual = Optional
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Optional ) );
            // Manual + Disabled = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

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

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Disabled );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // Disabled + Started = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + Stopped = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Disabled + None = Disabled
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );

            #endregion

            #region User AutomaticStart

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.AutomaticStart );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // AutomaticStart + Started = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // AutomaticStart + Stopped = Optional
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // AutomaticStart + None = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );

            #endregion

            #region User Manual

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id1, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id1, ConfigUserAction.Started );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id2, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id2, ConfigUserAction.Stopped );

            ConfigManager.UserConfiguration.PluginsStatus.SetStatus( id3, ConfigPluginStatus.Manual );
            ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( id3, ConfigUserAction.None );

            // Manual + Started = MustExistAndRun
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id1 ), Is.EqualTo( SolvedConfigStatus.MustExistAndRun ) );
            // Manual + Stopped = Optional
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id2 ), Is.EqualTo( SolvedConfigStatus.Disabled ) );
            // Manual + None = Optional
            Assert.That( ConfigManager.SolvedPluginConfiguration.GetStatus( id3 ), Is.EqualTo( SolvedConfigStatus.Optional ) );

            #endregion
        }
    }
}
