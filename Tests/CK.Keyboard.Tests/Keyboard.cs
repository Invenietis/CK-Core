#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Contexts\Keyboard.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using NUnit.Framework;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Plugin;
using CK.Keyboard;

namespace Keyboard
{
    [TestFixture]
	public class Keyboard
	{
        KeyboardContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new KeyboardContext();
        }

        [Test]
        public void KeysCount()
        {
            IKeyboard kb = Context.Keyboards.Create( "test" );
            IZone z1 = kb.Zones.Create( "zone1" );
            IZone z2 = kb.Zones.Create( "zone2" );
            IZone z3 = kb.Zones.Create( "zone3" );
            IKey k11 = z1.Keys.Create();
            IKey k12 = z1.Keys.Create();
            IKey k13 = z1.Keys.Create();
            IKey k21 = z2.Keys.Create();
            IKey k22 = z2.Keys.Create();
            IKey k31 = z3.Keys.Create();
            IKey k32 = z3.Keys.Create();

            Assert.That( kb.Keys.Count == 7 );
            k11.Destroy();
            k12.Destroy();
            k21.Destroy();
            Assert.That( kb.Keys.Count == 4 );
            z3.Keys.Create();
            Assert.That( kb.Keys.Count == 5 );
        }

        [Test]
        public void KeysContains()
        {
            IKeyboard kb = Context.Keyboards.Create( "test" );
            IZone z1 = kb.Zones.Create( "zone1" );
            IKey k11 = z1.Keys.Create();

            IKeyboard kb1 = Context.Keyboards.Create( "test1" );
            IZone z2 = kb1.Zones.Create( "zone1" );
            IKey k21 = z2.Keys.Create();

            Assert.That( kb.Keys.Contains( k11 ) );
            Assert.That( kb1.Keys.Contains( k21 ) );
            Assert.That( !kb1.Keys.Contains( k11 ) );
            Assert.That( !kb.Keys.Contains( k21 ) );
        }

        //[Test]
        //public void ServiceRequirements()
        //{
        //    ServiceRequirementCollection reqs = new ServiceRequirementCollection( );
        //    IServiceRequirement req = reqs.AddOrSet( "service1", CK.Plugin.RunningRequirement.MustExistAndRun );
        //    Assert.That( reqs.Count == 1 );
        //    IServiceRequirement req1 = reqs.AddOrSet( "service2", CK.Plugin.RunningRequirement.MustExistAndRun );
        //    Assert.That( reqs.Count == 2 );
        //    IServiceRequirement req2 =  reqs.AddOrSet( "service3", CK.Plugin.RunningRequirement.MustExistAndRun );
        //    Assert.That( reqs.Count == 3 );

        //    Assert.That( reqs.Contains( req2 ) );
        //    Assert.That( reqs.Contains( req1 ) );
        //    Assert.That( reqs.Contains( req ) );

        //    req2.Destroy();

        //    Assert.That( reqs.Count == 2 );

        //    Assert.That( !reqs.Contains( req2 ) );

        //    foreach( IServiceRequirement o in reqs )
        //    {
        //        Assert.That( o.AssemblyQualifiedName.StartsWith( "service" ) );
        //    }
        //}

        [Test]
        public void KeysIndexOf()
        {
            IKeyboard kb = Context.Keyboards.Create( "test" );
            IZone z1 = kb.Zones.Create( "zone1" );
            IKey k11 = z1.Keys.Create();
            Assert.That( kb.Keys.IndexOf( k11 ) == 0 );

            IKey k12 = z1.Keys.Create();
            IKey k13 = z1.Keys.Create();
            Assert.That( kb.Keys.IndexOf( k12 ) == 1 );
            Assert.That( kb.Keys.IndexOf( k13 ) == 2 );

            k12.Destroy();
            Assert.That( kb.Keys.IndexOf( k13 ) == 1 );

            IZone z2 = kb.Zones.Create( "zone2" );
            IKey k21 = z2.Keys.Create();
            IKey k22 = z2.Keys.Create();
            Assert.That( kb.Keys.IndexOf( k21 ) == 2 );
            Assert.That( kb.Keys.IndexOf( k22 ) == 3 );

            IKey poufKey = z1.Keys.Create();
            Assert.That( kb.Keys.IndexOf( k11 ) == 0 );
            Assert.That( kb.Keys.IndexOf( k13 ) == 1 );
            Assert.That( kb.Keys.IndexOf( poufKey ) == 2 );
            Assert.That( kb.Keys.IndexOf( k21 ) == 3 );
            Assert.That( kb.Keys.IndexOf( k22 ) == 4 );
        }

        [Test]
        public void FindKeyByIndex()
        {
            IKeyboard kb = Context.Keyboards.Create( "test" );
            IZone z1 = kb.Zones.Create( "zone1" );
            IKey k11 = z1.Keys.Create();
            IKey k12 = z1.Keys.Create();
            IKey k13 = z1.Keys.Create();
            Assert.That( kb.Keys[1] == k12 );
            Assert.That( kb.Keys[2] == k13 );

            k12.Destroy();
            Assert.That( kb.Keys[1] == k13 );

            IZone z2 = kb.Zones.Create( "zone2" );
            IKey k21 = z2.Keys.Create();
            IKey k22 = z2.Keys.Create();
            Assert.That( kb.Keys[2] == k21 );
            Assert.That( kb.Keys[3] == k22 );

            IKey poufKey = z1.Keys.Create();
            Assert.That( kb.Keys[0] == k11 );
            Assert.That( kb.Keys[1] == k13 );
            Assert.That( kb.Keys[2] == poufKey );
            Assert.That( kb.Keys[3] == k21 );
            Assert.That( kb.Keys[4] == k22 );
        }
	}
}
