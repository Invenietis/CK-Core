#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\PluginRequirements.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;

namespace PluginConfig
{
    [TestFixture]
    public class PluginRequirements
    {
        [Test]
        public void TestPluginRequirementCollectionEvents()
        {
            int createdEventFired = 0;
            int removedEventFired = 0;
            int updatedEventFired = 0;
            int clearedEventFired = 0;

            int creatingEventFired = 0;
            int removingEventFired = 0;
            int updatingEventFired = 0;
            int clearingEventFired = 0;

            bool cancel = false;

            Guid changingPluginId = Guid.Empty;
            RunningRequirement previousRequirement = RunningRequirement.OptionalTryStart;
            RunningRequirement newRequirement = RunningRequirement.OptionalTryStart;
            PluginRequirement lastRequirementCreated = null;
            PluginRequirement lastRequirementRemoved = null;

            IPluginRequirementCollection reqs = new PluginRequirementCollection();

            reqs.Changing += ( sender, args ) =>
            {
                switch( args.Action )
                {
                    case CK.Core.ChangeStatus.Update:
                        updatingEventFired++;
                        if( !cancel )                        
                            previousRequirement = args.Collection.Find( args.PluginId ).Requirement;                                                
                        break;
                    case CK.Core.ChangeStatus.Add:
                        creatingEventFired++;
                        break;
                    case CK.Core.ChangeStatus.Delete:
                        removingEventFired++;
                        break;
                    case CK.Core.ChangeStatus.ContainerClear:
                        clearingEventFired++;
                        break;                       
                }
                if( cancel )
                    args.Cancel = true;
            };

            reqs.Changed += ( sender, args ) =>
            {
                switch( args.Action )
                {
                    case CK.Core.ChangeStatus.Update:
                        updatedEventFired++;
                        newRequirement = args.Requirement;
                        break;
                    case CK.Core.ChangeStatus.Add:
                        lastRequirementCreated = args.Collection.Find( args.PluginId );
                        createdEventFired++;
                        break;
                    case CK.Core.ChangeStatus.Delete:
                        lastRequirementRemoved = args.Collection.Find( args.PluginId );
                        removedEventFired++;
                        break;
                    case CK.Core.ChangeStatus.ContainerClear:
                        clearedEventFired++;
                        break;
                }
            };

            PluginRequirement req = reqs.AddOrSet( Guid.NewGuid(), RunningRequirement.MustExistAndRun );
            Assert.That( lastRequirementCreated == req );
            Assert.That( createdEventFired == 1 );
            Assert.That( creatingEventFired == 1 );

            PluginRequirement req1 = reqs.AddOrSet( Guid.NewGuid(), RunningRequirement.MustExistAndRun );
            Assert.That( lastRequirementCreated == req1 );
            Assert.That( createdEventFired == 2 );
            Assert.That( creatingEventFired == 2 );

            PluginRequirement req2 =  reqs.AddOrSet( Guid.NewGuid(), RunningRequirement.MustExistAndRun );
            Assert.That( lastRequirementCreated == req2 );
            Assert.That( createdEventFired == 3 );
            Assert.That( creatingEventFired == 3 );

            reqs.AddOrSet( req2.PluginId, RunningRequirement.MustExist );

            Assert.That( previousRequirement == RunningRequirement.MustExistAndRun );
            Assert.That( newRequirement == RunningRequirement.MustExist );
            Assert.That( updatingEventFired == 1 );
            Assert.That( updatedEventFired == 1 );

            // Now, cancel is true, nothing should be done
            cancel = true;
            reqs.AddOrSet( req2.PluginId, RunningRequirement.Optional );

            Assert.That( previousRequirement == RunningRequirement.MustExistAndRun );
            Assert.That( newRequirement == RunningRequirement.MustExist );
            Assert.That( updatingEventFired == 2 );
            Assert.That( updatedEventFired == 1 );

            cancel = false;

            reqs.Clear();

            Assert.That( clearedEventFired, Is.EqualTo( 1 ) );
        }

        [Test]
        public void TestPluginRequirementCollectionEnumerable()
        {
            Guid guid0 = Guid.NewGuid();
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();

            IPluginRequirementCollection reqs = new PluginRequirementCollection( );

            PluginRequirement req0 = reqs.AddOrSet( guid0, RunningRequirement.MustExistAndRun );
            Assert.That( reqs.Count == 1 );

            PluginRequirement req1 = reqs.AddOrSet( guid1, RunningRequirement.Optional );
            Assert.That( reqs.Count == 2 );

            PluginRequirement req2 =  reqs.AddOrSet( guid2, RunningRequirement.OptionalTryStart );
            Assert.That( reqs.Count == 3 );

            Assert.That( reqs.Contains( req2 ) && reqs.Contains( req1 ) && reqs.Contains( req0 ) );

            reqs.Remove( guid2 );

            Assert.That( reqs.Count == 2 );
            Assert.That( !reqs.Contains( req2 ) );

            int passed = 0;
            foreach( PluginRequirement o in reqs )
            {
                if( o.PluginId == guid0 )
                {
                    Assert.That( o.Requirement == RunningRequirement.MustExistAndRun );
                    passed++;
                }
                if( o.PluginId == guid1 )
                {
                    Assert.That( o.Requirement == RunningRequirement.Optional );
                    passed++;
                }
            }
            Assert.That( passed, Is.EqualTo( 2 ) );

            reqs.Clear();

            Assert.That( reqs.Count == 0 );
            Assert.That( ((PluginRequirement)req0).Holder == null );
            Assert.That( ((PluginRequirement)req1).Holder == null );
            Assert.That( ((PluginRequirement)req2).Holder == null );

            passed = 0;
            foreach( PluginRequirement o in reqs )
            {
                passed++;
            }
            Assert.That( passed == 0 );
        }

        [Test]
        public void TestPluginRequirementCollectionUpdate()
        {             

            bool updated = false;
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            IPluginRequirementCollection reqs = new PluginRequirementCollection( );
            reqs.Changed += ( o, e ) =>  {if (e.Action == CK.Core.ChangeStatus.Update)
                                                        updated = true;
                                                    };

            PluginRequirement req = reqs.AddOrSet( guid1, RunningRequirement.MustExistAndRun );
            Assert.That( reqs.Count == 1 );
            //Assert.That( (req.Source == reqs.Source) && (reqs.Source == ctx) );

            PluginRequirement req1 = reqs.AddOrSet( guid2, RunningRequirement.MustExistAndRun );
            Assert.That( reqs.Count == 2 );

            PluginRequirement req2 =  reqs.AddOrSet( guid3, RunningRequirement.MustExistAndRun );
            Assert.That( reqs.Count == 3 );

            Assert.That( reqs.Contains( req2 ) && reqs.Contains( req1 ) && reqs.Contains( req ) );

            PluginRequirement req3 =  reqs.AddOrSet( guid3, RunningRequirement.Optional );

            Assert.That( updated );
            Assert.That( req2 == req3 );
            Assert.That( reqs.Count == 3 );
        }
    }
}
