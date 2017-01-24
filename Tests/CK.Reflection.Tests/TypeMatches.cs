#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Reflection.Tests\TypeMatches.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using FluentAssertions;
 
namespace CK.Reflection.Tests
{
    interface IOverBase2<T>
    {
        int Pouf4 { get; }
    }

    interface IBase2<T, T2> : IOverBase2<T>
    {
        int Pouf3 { get; }
    }

    interface IOverBase<T>
    {
        int Pouf0 { get; }
    }

    interface IBase<T> : IOverBase<T>
    {
        event EventHandler Event2;
        int Pouf { get; }
    }

    interface IDerived<T, T2> : IBase<T>, IBase2<T, T2>
    {
        event EventHandler Event1;
        int Pouf2 { get; }
    }

    [ExcludeFromCodeCoverage]
    public class A { }

    [ExcludeFromCodeCoverage]
    public class B : A { }

    [ExcludeFromCodeCoverage]
    public class C : B { }

    [ExcludeFromCodeCoverage]
    public class CloseBase<T> : IBase<T>
    {
        // Suppress: The event 'CK.Reflection.Tests.CloseBase<T>.Event2' is never used
        #pragma warning disable 0067
        public event EventHandler Event2;
        #pragma warning restore 0067
        public int Pouf { get; set; }
        public int Pouf0 { get; set; }
    }

    // This one has only one IBase<> : IBase<bool>
    [ExcludeFromCodeCoverage]
    public class Close : CloseBase<bool>, IDerived<bool, B>
    {

        #pragma warning disable 0067
        public event EventHandler Event1;
        public new event EventHandler Event2;
        #pragma warning restore 0067
        public int Pouf2 { get; set; }
        public new int Pouf { get; set; }
        public new int Pouf0 { get; set; }
        public int Pouf3 { get; set; }
        public int Pouf4 { get; set; }
    }

    // This one has two IBase<> : IBase<bool> and IBase<B>.
    [ExcludeFromCodeCoverage]
    public class Close2 : CloseBase<B>, IDerived<bool, B>
    {
        #pragma warning disable 0067
        public event EventHandler Event1;
        public new event EventHandler Event2;
        #pragma warning restore 0067
        public int Pouf2 { get; set; }
        public new int Pouf { get; set; }
        public new int Pouf0 { get; set; }
        public int Pouf3 { get; set; }
        public int Pouf4 { get; set; }
    }


    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TypeMatches
    {

        [Test]
        public void TestCovarianceMatch()
        {
            AssertCheck( "False - typeof(IBase<>).IsAssignableFrom( typeof(IDerived<,>) )", typeof( IBase<> ).IsAssignableFrom( typeof( IDerived<,> ) ) );
            AssertCheck( "False - typeof(IBase<>).IsAssignableFrom( typeof(IBase<int>) )", typeof( IBase<> ).IsAssignableFrom( typeof( IBase<int> ) ) );

            AssertCheck( "True  - typeof(IBase<>).IsGenericTypeDefinition", typeof( IBase<> ).GetTypeInfo().IsGenericTypeDefinition );
            AssertCheck( "False - typeof(IBase<bool>).IsGenericTypeDefinition", typeof( IBase<bool> ).GetTypeInfo().IsGenericTypeDefinition );
            AssertCheck( "True  - typeof(IDerived<,>).IsGenericTypeDefinition", typeof( IDerived<,> ).GetTypeInfo().IsGenericTypeDefinition );
            AssertCheck( "False - typeof(IDerived<int,bool>).IsGenericTypeDefinition", typeof( IDerived<int, bool> ).GetTypeInfo().IsGenericTypeDefinition );

            Type tC = typeof( Close );
            Type tC2 = typeof( Close2 );

            AssertCheck( "False - Close is NOT a generic: tC.IsGenericType = ", tC.GetTypeInfo().IsGenericType );
            AssertCheck( "True  - Close contains IDerived<bool,B>: IDerived<bool,B>.IsAssignableFrom( Close )", typeof( IDerived<bool, B> ).IsAssignableFrom( tC ) );
            AssertCheck( "False - ...but does not contain IDerived<bool,A>: IDerived<bool,A>.IsAssignableFrom( Close )", typeof( IDerived<bool, A> ).IsAssignableFrom( tC ) );

            //Console.WriteLine( "-- CovariantMatch on Close:" );
            CommonToCloseAndClose2( tC, "Close" );
            AssertCheck( "False - IBase<B>, Close", ReflectionHelper.CovariantMatch( typeof( IBase<B> ), tC ) );
            AssertCheck( "False - IBase<A>, Close", ReflectionHelper.CovariantMatch( typeof( IBase<A> ), tC ) );

            AssertCheck( "False  - CloseBase<B>, Close", ReflectionHelper.CovariantMatch( typeof( CloseBase<B> ), tC ) );
            AssertCheck( "False  - CloseBase<A>, Close", ReflectionHelper.CovariantMatch( typeof( CloseBase<A> ), tC ) );
            AssertCheck( "True  - CloseBase<bool>, Close", ReflectionHelper.CovariantMatch( typeof( CloseBase<bool> ), tC ) );
            AssertCheck( "True  - CloseBase<ValueType>, Close", ReflectionHelper.CovariantMatch( typeof( CloseBase<ValueType> ), tC ) );

            //Console.WriteLine( "-- CovariantMatch on Close2:" );
            CommonToCloseAndClose2( tC2, "Close2" );
            AssertCheck( "True - IBase<B>, Close2", ReflectionHelper.CovariantMatch( typeof( IBase<B> ), tC2 ) );
            AssertCheck( "True - IBase<A>, Close2", ReflectionHelper.CovariantMatch( typeof( IBase<A> ), tC2 ) );

            AssertCheck( "True  - CloseBase<B>, Close2", ReflectionHelper.CovariantMatch( typeof( CloseBase<B> ), tC2 ) );
            AssertCheck( "True  - CloseBase<A>, Close2", ReflectionHelper.CovariantMatch( typeof( CloseBase<A> ), tC2 ) );
            AssertCheck( "False - CloseBase<bool>, Close2", ReflectionHelper.CovariantMatch( typeof( CloseBase<bool> ), tC2 ) );
            AssertCheck( "False - CloseBase<ValueType>, Close2", ReflectionHelper.CovariantMatch( typeof( CloseBase<ValueType> ), tC2 ) );

        }

        private static void CommonToCloseAndClose2( Type tC, string name )
        {
            AssertCheck( "True - object" + name, ReflectionHelper.CovariantMatch( typeof( object ), tC ) );

            AssertCheck( "False - IEnumerable<bool>" + name, ReflectionHelper.CovariantMatch( typeof( IEnumerable<bool> ), tC ) );
            AssertCheck( "True  - IDerived<bool,A>" + name, ReflectionHelper.CovariantMatch( typeof( IDerived<bool, A> ), tC ) );
            AssertCheck( "True  - IDerived<bool,B>" + name, ReflectionHelper.CovariantMatch( typeof( IDerived<bool, B> ), tC ) );
            AssertCheck( "False - IDerived<bool,C>" + name, ReflectionHelper.CovariantMatch( typeof( IDerived<bool, C> ), tC ) );
            AssertCheck( "False - IDerived<int,A>" + name, ReflectionHelper.CovariantMatch( typeof( IDerived<int, A> ), tC ) );
            AssertCheck( "True  - IDerived<ValueType,A>" + name, ReflectionHelper.CovariantMatch( typeof( IDerived<ValueType, A> ), tC ) );
            AssertCheck( "True  - IOverBase<bool>" + name, ReflectionHelper.CovariantMatch( typeof( IOverBase<bool> ), tC ) );
            AssertCheck( "True  - IOverBase<ValueType>" + name, ReflectionHelper.CovariantMatch( typeof( IOverBase<ValueType> ), tC ) );
            AssertCheck( "True  - IOverBase<object>" + name, ReflectionHelper.CovariantMatch( typeof( IOverBase<object> ), tC ) );
            AssertCheck( "False - IOverBase<int>" + name, ReflectionHelper.CovariantMatch( typeof( IOverBase<int> ), tC ) );
            AssertCheck( "False - IBase<C>" + name, ReflectionHelper.CovariantMatch( typeof( IBase<C> ), tC ) );
            AssertCheck( "True  - IBase<object>" + name, ReflectionHelper.CovariantMatch( typeof( IBase<object> ), tC ) );

            AssertCheck( "True  - CloseBase<object>" + name, ReflectionHelper.CovariantMatch( typeof( CloseBase<object> ), tC ) );
            AssertCheck( "False - CloseBase<C>" + name, ReflectionHelper.CovariantMatch( typeof( CloseBase<C> ), tC ) );
        }

        static void AssertCheck( string msg, bool test )
        {
            (msg[0] == 'T').Should().Be( test, msg );
        }

    }

}
