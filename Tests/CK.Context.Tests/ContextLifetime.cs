#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Context.Tests\ContextLifetime.cs) is part of CiviKey. 
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
using CK.Plugin;
using NUnit.Framework;

namespace CK.Context.Tests
{
    [TestFixture]
    public class ContextLifetime
    {
        [Test]
        public void ApplicationExitEvents()
        {
            {
                TestContextHost host = new TestContextHost("TestContexts");

                IContext c = host.CreateContext();

                bool before = false, after = false;
                c.ApplicationExiting += ( o, e ) => { Assert.That( e.HostShouldExit ); before = true; };
                c.ApplicationExited += ( o, e ) => { Assert.That( e.HostShouldExit ); after = true; };

                Assert.That( c.RaiseExitApplication( true ) );
                Assert.That( before && after );
            }
            {
                TestContextHost host = new TestContextHost("TestContexts");

                IContext c = host.CreateContext();

                bool before = false, after = false;
                c.ApplicationExiting += ( o, e ) => { e.Cancel = true; before = true; };
                c.ApplicationExited += ( o, e ) => after = true;

                Assert.That( c.RaiseExitApplication( false ), Is.False );
                Assert.That( before && after == false );
            }
        }

        [Test]
        public void ApplicationExitDisableRunner()
        {
            Guid simplePluginId = new Guid( "{EEAEC976-2AFC-4A68-BFAD-68E169677D52}" );

            TestContextHost host = new TestContextHost("TestContexts");

            IContext c = host.CreateContext();

            TestBase.CopyPluginToTestDir( "SimplePlugin.dll" );
            c.PluginRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            var pluginId = c.PluginRunner.Discoverer.FindPlugin( simplePluginId );
            Assert.That( pluginId, Is.Not.Null );

            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId ), Is.False );
            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId.PluginId ), Is.False );

            var req = new RequirementLayer( "Start SimplePlugin" );
            req.PluginRequirements.AddOrSet( simplePluginId, RunningRequirement.MustExistAndRun );
            c.PluginRunner.Add( req );
            c.PluginRunner.Apply();

            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId ), Is.True, "SimplePlugin is running." );
            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId.PluginId ), Is.True, "SimplePlugin is running." );

            int eventPhasis = 0;
            c.PluginRunner.PluginHost.StatusChanged += ( o, e ) =>
            {
                Assert.That( eventPhasis >= 0 && eventPhasis < 3 );
                Assert.That( e.PluginProxy.PluginKey.PluginId, Is.EqualTo( simplePluginId ) );
                if( eventPhasis == 0 )
                {
                    Assert.That( e.Previous, Is.EqualTo( RunningStatus.Started ) );
                    Assert.That( e.PluginProxy.Status, Is.EqualTo( RunningStatus.Stopping ) );
                    eventPhasis = 1;
                }
                else if( eventPhasis == 1 )
                {
                    Assert.That( e.Previous, Is.EqualTo( RunningStatus.Stopping ) );
                    Assert.That( e.PluginProxy.Status, Is.EqualTo( RunningStatus.Stopped ) );
                    eventPhasis = 2;
                }
                else if( eventPhasis == 2 )
                {
                    Assert.That( e.Previous, Is.EqualTo( RunningStatus.Stopped ) );
                    Assert.That( e.PluginProxy.Status, Is.EqualTo( RunningStatus.Disabled ) );
                    eventPhasis = 3;
                }
            };

            Assert.That( c.RaiseExitApplication( true ) );

            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId ), Is.False, "SimplePlugin is no more running." );
            Assert.That( c.PluginRunner.PluginHost.IsPluginRunning( pluginId.PluginId ), Is.False, "SimplePlugin is no more running." );
        }

    }
}

