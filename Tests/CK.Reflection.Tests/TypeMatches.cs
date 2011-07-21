using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
 
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

    public class A { }
    public class B : A { }
    public class C : B { }

    public class CloseBase<T> : IBase<T>
    {
        public event EventHandler Event2;
        public int Pouf { get; set; }
        public int Pouf0 { get; set; }
    }

    // This one has only one IBase<> : IBase<bool>
    public class Close : CloseBase<bool>, IDerived<bool, B>
    {

        public event EventHandler Event1;
        public event EventHandler Event2;
        public int Pouf2 { get; set; }
        public int Pouf { get; set; }
        public int Pouf0 { get; set; }
        public int Pouf3 { get; set; }
        public int Pouf4 { get; set; }
    }

    // This one has two IBase<> : IBase<bool> and IBase<B>.
    public class Close2 : CloseBase<B>, IDerived<bool, B>
    {

        public event EventHandler Event1;
        public event EventHandler Event2;
        public int Pouf2 { get; set; }
        public int Pouf { get; set; }
        public int Pouf0 { get; set; }
        public int Pouf3 { get; set; }
        public int Pouf4 { get; set; }
    }


    [TestFixture]
    public class TypeMatches
    {

        [Test]
        public void TestCovarianceMatch()
        {
            AssertCheck( "False - typeof(IBase<>).IsAssignableFrom( typeof(IDerived<,>) )", typeof( IBase<> ).IsAssignableFrom( typeof( IDerived<,> ) ) );
            AssertCheck( "False - typeof(IBase<>).IsAssignableFrom( typeof(IBase<int>) )", typeof( IBase<> ).IsAssignableFrom( typeof( IBase<int> ) ) );

            AssertCheck( "True  - typeof(IBase<>).IsGenericTypeDefinition", typeof( IBase<> ).IsGenericTypeDefinition );
            AssertCheck( "False - typeof(IBase<bool>).IsGenericTypeDefinition", typeof( IBase<bool> ).IsGenericTypeDefinition );
            AssertCheck( "True  - typeof(IDerived<,>).IsGenericTypeDefinition", typeof( IDerived<,> ).IsGenericTypeDefinition );
            AssertCheck( "False - typeof(IDerived<int,bool>).IsGenericTypeDefinition", typeof( IDerived<int, bool> ).IsGenericTypeDefinition );

            Type tC = typeof( Close );
            Type tC2 = typeof( Close2 );

            AssertCheck( "False - Close is NOT a generic: tC.IsGenericType = ", tC.IsGenericType );
            AssertCheck( "True  - Close contains IDerived<bool,B>: IDerived<bool,B>.IsAssignableFrom( Close )", typeof( IDerived<bool, B> ).IsAssignableFrom( tC ) );
            AssertCheck( "False - ...but does not contain IDerived<bool,A>: IDerived<bool,A>.IsAssignableFrom( Close )", typeof( IDerived<bool, A> ).IsAssignableFrom( tC ) );

            //Console.WriteLine( "-- CovariantMatch on Close:" );
            CommonToCloseAndClose2( tC, "Close" );
            AssertCheck( "False - IBase<B>, Close", Helper.CovariantMatch( typeof( IBase<B> ), tC ) );
            AssertCheck( "False - IBase<A>, Close", Helper.CovariantMatch( typeof( IBase<A> ), tC ) );

            AssertCheck( "False  - CloseBase<B>, Close", Helper.CovariantMatch( typeof( CloseBase<B> ), tC ) );
            AssertCheck( "False  - CloseBase<A>, Close", Helper.CovariantMatch( typeof( CloseBase<A> ), tC ) );
            AssertCheck( "True  - CloseBase<bool>, Close", Helper.CovariantMatch( typeof( CloseBase<bool> ), tC ) );
            AssertCheck( "True  - CloseBase<ValueType>, Close", Helper.CovariantMatch( typeof( CloseBase<ValueType> ), tC ) );

            //Console.WriteLine( "-- CovariantMatch on Close2:" );
            CommonToCloseAndClose2( tC2, "Close2" );
            AssertCheck( "True - IBase<B>, Close2", Helper.CovariantMatch( typeof( IBase<B> ), tC2 ) );
            AssertCheck( "True - IBase<A>, Close2", Helper.CovariantMatch( typeof( IBase<A> ), tC2 ) );

            AssertCheck( "True  - CloseBase<B>, Close2", Helper.CovariantMatch( typeof( CloseBase<B> ), tC2 ) );
            AssertCheck( "True  - CloseBase<A>, Close2", Helper.CovariantMatch( typeof( CloseBase<A> ), tC2 ) );
            AssertCheck( "False - CloseBase<bool>, Close2", Helper.CovariantMatch( typeof( CloseBase<bool> ), tC2 ) );
            AssertCheck( "False - CloseBase<ValueType>, Close2", Helper.CovariantMatch( typeof( CloseBase<ValueType> ), tC2 ) );

        }

        private static void CommonToCloseAndClose2( Type tC, string name )
        {
            AssertCheck( "True - object" + name, Helper.CovariantMatch( typeof( object ), tC ) );

            AssertCheck( "False - IEnumerable<bool>" + name, Helper.CovariantMatch( typeof( IEnumerable<bool> ), tC ) );
            AssertCheck( "True  - IDerived<bool,A>" + name, Helper.CovariantMatch( typeof( IDerived<bool, A> ), tC ) );
            AssertCheck( "True  - IDerived<bool,B>" + name, Helper.CovariantMatch( typeof( IDerived<bool, B> ), tC ) );
            AssertCheck( "False - IDerived<bool,C>" + name, Helper.CovariantMatch( typeof( IDerived<bool, C> ), tC ) );
            AssertCheck( "False - IDerived<int,A>" + name, Helper.CovariantMatch( typeof( IDerived<int, A> ), tC ) );
            AssertCheck( "True  - IDerived<ValueType,A>" + name, Helper.CovariantMatch( typeof( IDerived<ValueType, A> ), tC ) );
            AssertCheck( "True  - IOverBase<bool>" + name, Helper.CovariantMatch( typeof( IOverBase<bool> ), tC ) );
            AssertCheck( "True  - IOverBase<ValueType>" + name, Helper.CovariantMatch( typeof( IOverBase<ValueType> ), tC ) );
            AssertCheck( "True  - IOverBase<object>" + name, Helper.CovariantMatch( typeof( IOverBase<object> ), tC ) );
            AssertCheck( "False - IOverBase<int>" + name, Helper.CovariantMatch( typeof( IOverBase<int> ), tC ) );
            AssertCheck( "False - IBase<C>" + name, Helper.CovariantMatch( typeof( IBase<C> ), tC ) );
            AssertCheck( "True  - IBase<object>" + name, Helper.CovariantMatch( typeof( IBase<object> ), tC ) );

            AssertCheck( "True  - CloseBase<object>" + name, Helper.CovariantMatch( typeof( CloseBase<object> ), tC ) );
            AssertCheck( "False - CloseBase<C>" + name, Helper.CovariantMatch( typeof( CloseBase<C> ), tC ) );
        }

        static void AssertCheck( string msg, bool test )
        {
            Assert.That( test == (msg[0] == 'T'), msg );
        }

    }

}
