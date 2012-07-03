#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Reflection.Tests\ObjectAndTypeTest.cs) is part of CiviKey. 
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
using System.Reflection;

namespace CK.Reflection.Tests
{
    [TestFixture]
    public class ObjectAndType
    {
        static string _lastCalledName;
        static int  _lastCalledParam;

        public class A
        {
            public virtual string SimpleMethod( int i )
            {
                _lastCalledName = "A.SimpleMethod";
                _lastCalledParam = i;
                return i.ToString();
            }

            public static string StaticMethod( int i )
            {
                _lastCalledName = "A.StaticMethod";
                _lastCalledParam = i;
                return i.ToString();
            }
        }

        public class B : A
        {
            public override string SimpleMethod( int i )
            {
                _lastCalledName = "B.SimpleMethod";
                _lastCalledParam = i;
                return i.ToString();
            }
        }

        [Test]
        public void StaticInvoker()
        {
            Type tA = typeof( A );
            Type tB = typeof( B );
            {
                {
                    // Null or MissingMethodException
                    Func<int, int> fsUnk1 = tA.GetStaticInvoker<Func<int, int>>( "StaticMethod" );
                    Assert.That( fsUnk1, Is.Null );

                    Func<int, string> fsUnk2 = tA.GetStaticInvoker<Func<int, string>>( "StaticMethodUnk" );
                    Assert.That( fsUnk2, Is.Null );

                    Assert.Throws<MissingMethodException>( () => tA.GetStaticInvoker<Func<int, int>>( "StaticMethod", true ) );
                    Assert.Throws<MissingMethodException>( () => tA.GetStaticInvoker<Func<int, string>>( "StaticMethodUnk", true ) );

                    Assert.That( tA.GetStaticInvoker<Func<int, int>>( "StaticMethod", false ), Is.Null );
                    Assert.That( tA.GetStaticInvoker<Func<int, string>>( "StaticMethodUnk", false ), Is.Null );
                }
                // Delegate to the static method.
                Func<int, string> fsA = tA.GetStaticInvoker<Func<int, string>>( "StaticMethod" );
                Assert.That( fsA, Is.Not.Null );
                Assert.That( fsA( 1 ), Is.EqualTo( "1" ) );
                Assert.That( _lastCalledName, Is.EqualTo( "A.StaticMethod" ) );
                Assert.That( _lastCalledParam, Is.EqualTo( 1 ) );

            }
        }
        
        [Test]
        public void InstanceInvoker()
        {
            Type tA = typeof( A );
            Type tB = typeof( B );

            {
                // Null or MissingMethodException.
                Func<A, int, int> fUnk1 = tA.GetInstanceInvoker<Func<A, int, int>>( "SimpleMethod" );
                Assert.That( fUnk1, Is.Null );

                Func<A, int, string> fUnk2 = tA.GetInstanceInvoker<Func<A, int, string>>( "SimpleMethoddUnk" );
                Assert.That( fUnk2, Is.Null );

                Assert.Throws<MissingMethodException>( () => tA.GetInstanceInvoker<Func<A, int, int>>( "SimpleMethod", true ) );
                Assert.Throws<MissingMethodException>( () => tA.GetInstanceInvoker<Func<A, int, string>>( "SimpleMethodUnk", true ) );

                Assert.That( tA.GetInstanceInvoker<Func<A, int, int>>( "SimpleMethod", false ), Is.Null );
                Assert.That( tA.GetInstanceInvoker<Func<A, int, string>>( "SimpleMethodUnk", false ), Is.Null );
            }

            A a = new A();
            B b = new B();
            {
                Func<A,int, string> fA = tA.GetInstanceInvoker<Func<A, int, string>>( "SimpleMethod" );
                Assert.That( fA( a, 2 ), Is.EqualTo( "2" ) );
                Assert.That( _lastCalledName, Is.EqualTo( "A.SimpleMethod" ) );
                Assert.That( _lastCalledParam, Is.EqualTo( 2 ) );

                Assert.That( fA( b, 3 ), Is.EqualTo( "3" ) );
                Assert.That( _lastCalledName, Is.EqualTo( "B.SimpleMethod" ), "Calling the virtual method: B method." );
                Assert.That( _lastCalledParam, Is.EqualTo( 3 ) );
            }
        }

        [Test]
        public void NonVirtualInstanceInvoker()
        {
            Type tA = typeof( A );
            Type tB = typeof( B );
            {
                // Null or MissingMethodException.
                Func<A, int, int> fUnk1 = tA.GetNonVirtualInvoker<Func<A, int, int>>( "SimpleMethod" );
                Assert.That( fUnk1, Is.Null );

                Func<A, int, string> fUnk2 = tA.GetNonVirtualInvoker<Func<A, int, string>>( "SimpleMethoddUnk" );
                Assert.That( fUnk2, Is.Null );

                Assert.Throws<MissingMethodException>( () => tA.GetNonVirtualInvoker<Func<A, int, int>>( "SimpleMethod", true ) );
                Assert.Throws<MissingMethodException>( () => tA.GetNonVirtualInvoker<Func<A, int, string>>( "SimpleMethodUnk", true ) );

                Assert.That( tA.GetNonVirtualInvoker<Func<A, int, int>>( "SimpleMethod", false ), Is.Null );
                Assert.That( tA.GetNonVirtualInvoker<Func<A, int, string>>( "SimpleMethodUnk", false ), Is.Null );
            }

            A a = new A();
            B b = new B();
            {
                Func<A,int, string> fA = tA.GetNonVirtualInvoker<Func<A, int, string>>( "SimpleMethod" );
                Assert.That( fA( a, 20 ), Is.EqualTo( "20" ) );
                Assert.That( _lastCalledName, Is.EqualTo( "A.SimpleMethod" ) );
                Assert.That( _lastCalledParam, Is.EqualTo( 20 ) );

                Assert.That( fA( b, 30 ), Is.EqualTo( "30" ) );
                Assert.That( _lastCalledName, Is.EqualTo( "A.SimpleMethod" ), "It is the base A method that is called, even if b overrides it." );
                Assert.That( _lastCalledParam, Is.EqualTo( 30 ) );
            }
        }
    }
}
