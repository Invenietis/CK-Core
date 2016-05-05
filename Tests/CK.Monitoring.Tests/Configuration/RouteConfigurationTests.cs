#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Configuration\RouteConfigurationTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.Monitoring;
using CK.Core;
using CK.RouteConfig;
using System.Diagnostics;

namespace CK.Monitoring.Tests.Configuration
{
    [TestFixture]
    [Category("ConsoleMonitor")]
    public class RouteConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        class TestActionConfiguration : ActionConfiguration
        {
            public TestActionConfiguration( string name )
                : base( name )
            {
            }
        }

        [Test]
        public void RoutesAndActions()
        {
            RouteConfiguration c = 
                new RouteConfiguration()
                    .AddAction(
                        new ActionSequenceConfiguration( "FirstGroup" )
                            .AddAction( new TestActionConfiguration( "Sink1" ) )
                            .AddAction( new TestActionConfiguration( "Sink2" ) )
                            .AddAction( new ActionParallelConfiguration( "Parallel n°1" )
                                .AddAction( new TestActionConfiguration( "Sink3" ) )
                                .AddAction( new TestActionConfiguration( "Sink4" ) ) )
                            .AddAction( new ActionParallelConfiguration( "Parallel n°2" )
                                .AddAction( new TestActionConfiguration( "Sink3" ) )
                                .AddAction( new TestActionConfiguration( "Sink4" ) ) ) )
                    .AddAction( new TestActionConfiguration( "SecondGlobal" ) )
                    .DeclareRoute(
                        new SubRouteConfiguration( "CKTask", name => name.StartsWith( "CKTask:" ) )
                            .AddAction( new TestActionConfiguration( "TaskSink" ) ) 
                            .RemoveAction( "SecondGlobal" ) )
                    .AddAction( new TestActionConfiguration( "ForAllExceptCKTask" ) )
                    .DeclareRoute(
                        new SubRouteConfiguration( "Request", name => name.Contains( "/request/" ) )
                            .RemoveAction( "FirstGroup" )
                            .AddAction( new TestActionConfiguration( "RequestSink" ) )
                            .AddAction( new TestActionConfiguration( "AnotherRequestSink" ) )
                            .DeclareRoute(
                                new SubRouteConfiguration( "NoBugInRequest", name => name.Contains( "/BugFree/" ) ) { ImportParentActions = false } ) );
            
            var resolved = c.Resolve( TestHelper.ConsoleMonitor );
            Assert.That( resolved, Is.Not.Null );
            Assert.That( resolved.AllSubRoutes.Count, Is.EqualTo( 3 ) );

            var root = resolved.Root;
            Assert.That( root, Is.Not.Null );
            Assert.That( root.ActionsResolved, Is.Not.Null.And.Count.EqualTo( 3 ) );

            var ckTask = resolved.FindSubRouteByName( "CKTask" );
            Assert.That( ckTask, Is.Not.Null );
            Assert.That( ckTask.ActionsResolved, Is.Not.Null.And.Count.EqualTo( 2 ) );
            
            var request = resolved.FindSubRouteByName( "Request" );
            Assert.That( request, Is.Not.Null );
            Assert.That( request.ActionsResolved, Is.Not.Null.And.Count.EqualTo( 4 ) );

            var noBug = resolved.FindSubRouteByName( "NoBugInRequest" );
            Assert.That( noBug, Is.Not.Null );
            Assert.That( noBug.ActionsResolved, Is.Not.Null.And.Count.EqualTo( 0 ) );

        }

        [Test]
        public void RouteNamesConflict()
        {
            RouteConfiguration c;
            {
                c = new RouteConfiguration()
                        .DeclareRoute( new SubRouteConfiguration( "Name", x => true ) )
                        .DeclareRoute( new SubRouteConfiguration( "Name", x => true ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .DeclareRoute( new SubRouteConfiguration( "Name", x => true )
                            .DeclareRoute( new SubRouteConfiguration( "Name", x => true ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
        }

        [Test]
        public void InvalidNames()
        {
            {
                Assert.Throws<ArgumentNullException>( () => new RouteConfiguration().DeclareRoute( new SubRouteConfiguration( null, x => true ) ) );
                Assert.Throws<ArgumentNullException>( () => new RouteConfiguration().AddAction( new TestActionConfiguration( null ) ) );
            }
            RouteConfiguration c;
            {
                c = new RouteConfiguration()
                        .DeclareRoute( new SubRouteConfiguration( "", x => true )
                            .DeclareRoute( new SubRouteConfiguration( "", x => true ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null, "A route name can be empty but not 2 can be empty at the same time. The name of the root RouteConfiguration is always the empty string." );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new TestActionConfiguration( "" ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new TestActionConfiguration( "/" ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new TestActionConfiguration( "A/B" ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
        }

        [Test]
        public void ActionNamesConflict()
        {
            RouteConfiguration c;
            {
                c = new RouteConfiguration()
                        .AddAction( new TestActionConfiguration( "Name" ) )
                        .AddAction( new TestActionConfiguration( "Name" ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new ActionParallelConfiguration( "Parallel" )
                            .AddAction( new TestActionConfiguration( "Name" ) )
                            .AddAction( new TestActionConfiguration( "Name" ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new ActionSequenceConfiguration( "Sequence" )
                            .AddAction( new TestActionConfiguration( "Name" ) )
                            .AddAction( new TestActionConfiguration( "Name" ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Null );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new TestActionConfiguration( "Name" ) )
                        .AddAction( new ActionSequenceConfiguration( "FirstGroup" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Not.Null, "Sequence acts as a namespace." );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new ActionSequenceConfiguration( "G1" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) )
                        .AddAction( new ActionSequenceConfiguration( "G2" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Not.Null, "Sequence hide names below them." );
            }
            {
                c = new RouteConfiguration()
                        .AddAction( new ActionSequenceConfiguration( "G1" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) )
                        .AddAction( new ActionParallelConfiguration( "P1" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) )
                        .AddAction( new ActionParallelConfiguration( "P2" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) )
                        .AddAction( new ActionSequenceConfiguration( "G2" )
                            .AddAction( new TestActionConfiguration( "Name" ) ) );
                Assert.That( c.Resolve( TestHelper.ConsoleMonitor ), Is.Not.Null, "Parallels also hide names below them." );
            }
        }

    }
}
