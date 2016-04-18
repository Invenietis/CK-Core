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
using NUnit.Framework;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace CK.Reflection.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category("Reflection")]
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
                    Func<int, int> fsUnk1 = DelegateHelper.GetStaticInvoker<Func<int, int>>( tA, "StaticMethod" );
                    Assert.That( fsUnk1, Is.Null );

                    Func<int, string> fsUnk2 = DelegateHelper.GetStaticInvoker<Func<int, string>>( tA, "StaticMethodUnk" );
                    Assert.That( fsUnk2, Is.Null );

                    Assert.Throws<MissingMethodException>( () => DelegateHelper.GetStaticInvoker<Func<int, int>>( tA, "StaticMethod", true ) );
                    Assert.Throws<MissingMethodException>( () => DelegateHelper.GetStaticInvoker<Func<int, string>>( tA, "StaticMethodUnk", true ) );

                    Assert.That( DelegateHelper.GetStaticInvoker<Func<int, int>>( tA, "StaticMethod", false ), Is.Null );
                    Assert.That( DelegateHelper.GetStaticInvoker<Func<int, string>>( tA, "StaticMethodUnk", false ), Is.Null );
                }
                // Delegate to the static method.
                Func<int, string> fsA = DelegateHelper.GetStaticInvoker<Func<int, string>>( tA, "StaticMethod" );
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
                Func<A, int, int> fUnk1 = DelegateHelper.GetInstanceInvoker<Func<A, int, int>>( tA, "SimpleMethod" );
                Assert.That( fUnk1, Is.Null );

                Func<A, int, string> fUnk2 = DelegateHelper.GetInstanceInvoker<Func<A, int, string>>( tA, "SimpleMethoddUnk" );
                Assert.That( fUnk2, Is.Null );

                Assert.Throws<MissingMethodException>( () => DelegateHelper.GetInstanceInvoker<Func<A, int, int>>( tA, "SimpleMethod", true ) );
                Assert.Throws<MissingMethodException>( () => DelegateHelper.GetInstanceInvoker<Func<A, int, string>>( tA, "SimpleMethodUnk", true ) );

                Assert.That( DelegateHelper.GetInstanceInvoker<Func<A, int, int>>( tA, "SimpleMethod", false ), Is.Null );
                Assert.That( DelegateHelper.GetInstanceInvoker<Func<A, int, string>>( tA, "SimpleMethodUnk", false ), Is.Null );
            }

            A a = new A();
            B b = new B();
            {
                Func<A,int, string> fA = DelegateHelper.GetInstanceInvoker<Func<A, int, string>>( tA, "SimpleMethod" );
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
                Func<A, int, int> fUnk1 = DelegateHelper.GetNonVirtualInvoker<Func<A, int, int>>( tA, "SimpleMethod" );
                Assert.That( fUnk1, Is.Null );

                Func<A, int, string> fUnk2 = DelegateHelper.GetNonVirtualInvoker<Func<A, int, string>>( tA, "SimpleMethoddUnk" );
                Assert.That( fUnk2, Is.Null );

                Assert.Throws<MissingMethodException>( () => DelegateHelper.GetNonVirtualInvoker<Func<A, int, int>>( tA, "SimpleMethod", true ) );
                Assert.Throws<MissingMethodException>( () => DelegateHelper.GetNonVirtualInvoker<Func<A, int, string>>( tA, "SimpleMethodUnk", true ) );

                Assert.That( DelegateHelper.GetNonVirtualInvoker<Func<A, int, int>>( tA, "SimpleMethod", false ), Is.Null );
                Assert.That( DelegateHelper.GetNonVirtualInvoker<Func<A, int, string>>( tA, "SimpleMethodUnk", false ), Is.Null );
            }

            A a = new A();
            B b = new B();
            {
                Func<A,int, string> fA = DelegateHelper.GetNonVirtualInvoker<Func<A, int, string>>( tA, "SimpleMethod" );
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
