using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace CK.Reflection.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "EmitHelper" )]
    public class AutoImplementorTests
    {
        static ModuleBuilder _moduleBuilder;
        static int _typeID;

        public TypeBuilder CreateTypeBuilder( Type abstractType )
        {
            if( _moduleBuilder == null )
            {
                AssemblyName assemblyName = new AssemblyName( "TypeImplementorModule" );
                assemblyName.Version = new Version( 1, 0, 0, 0 );
                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.RunAndSave );
                _moduleBuilder = assemblyBuilder.DefineDynamicModule( "TypeImplementorModule" );
            }
            return _moduleBuilder.DefineType( abstractType.Name + Interlocked.Increment( ref _typeID ).ToString(), TypeAttributes.Class | TypeAttributes.Public, abstractType );
        }

        #region EmitHelper.ImplementEmptyStubMethod tests

        public abstract class A
        {
            public A CallFirstMethod( int i )
            {
                return FirstMethod( i );
            }

            protected abstract A FirstMethod( int i );
        }

        public abstract class B
        {
            public abstract int M( int i );
        }

        public abstract class C
        {
            public abstract short M( int i );
        }

        public abstract class D
        {
            public abstract Guid M( int i );
        }

        public abstract class E
        {
            public abstract byte M( ref int i );
        }

        public abstract class F
        {
            public abstract byte M( out int i );
        }

        public abstract class G
        {
            public abstract byte M( out Guid i );
        }

        public abstract class H
        {
            public abstract byte M( ref Guid i );
        }

        public abstract class I
        {
            public abstract byte M( out CultureAttribute i );
        }

        public abstract class J
        {
            public abstract byte M( ref CultureAttribute i );
        }


        delegate void DynamicWithOutParameters( out Action a, out byte b, ref Guid g, int x );

        [Test]
        public void ImplementOutParameters()
        {
            {
                var dyn = new DynamicMethod( "TestMethod", typeof( void ), new Type[] { typeof( Action ).MakeByRefType(), typeof( byte ).MakeByRefType(), typeof( Guid ).MakeByRefType(), typeof( int ) } );
                var g = dyn.GetILGenerator();

                var parameters = dyn.GetParameters();
                g.StoreDefaultValueForOutParameter( parameters[0] );
                g.StoreDefaultValueForOutParameter( parameters[1] );
                g.StoreDefaultValueForOutParameter( parameters[2] );
                g.Emit( OpCodes.Ret );

                var d = (DynamicWithOutParameters)dyn.CreateDelegate( typeof( DynamicWithOutParameters ) );
                Action a = () => { };
                Byte b = 87;
                Guid guid = Guid.NewGuid();
                d( out a, out b, ref guid, 6554 );

                Assert.That( a, Is.Null );
                Assert.That( b, Is.EqualTo( 0 ) );
                Assert.That( guid, Is.EqualTo( Guid.Empty ) );
            }
        }

        [Test]
        public void AutoImplementStubReturnsClassAndProtected()
        {
            Type t = typeof( A );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "FirstMethod", BindingFlags.Instance | BindingFlags.NonPublic ), false );
            Type builtType = b.CreateType();
            A o = (A)Activator.CreateInstance( builtType );
            Assert.That( o.CallFirstMethod( 10 ), Is.Null );
        }

        [Test]
        public void AutoImplementStubReturnsInt()
        {
            Type t = typeof( B );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            B o = (B)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( 0 ) );
        }

        [Test]
        public void AutoImplementStubReturnsShort()
        {
            Type t = typeof( C );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            C o = (C)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( 0 ) );
        }

        [Test]
        public void AutoImplementStubReturnsGuid()
        {
            Type t = typeof( D );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            D o = (D)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( Guid.Empty ) );
        }

        [Test]
        public void AutoImplementStubRefInt()
        {
            Type t = typeof( E );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            E o = (E)Activator.CreateInstance( builtType );
            int i = 3712;
            Assert.That( o.M( ref i ), Is.EqualTo( 0 ) );
            Assert.That( i, Is.EqualTo( 3712 ) );
        }

        [Test]
        public void AutoImplementStubOutInt()
        {
            Type t = typeof( F );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            F o = (F)Activator.CreateInstance( builtType );
            int i = 45;
            Assert.That( o.M( out i ), Is.EqualTo( 0 ) );
            Assert.That( i, Is.EqualTo( 0 ) );
        }

        [Test]
        public void AutoImplementStubOutGuid()
        {
            Type t = typeof( G );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            G o = (G)Activator.CreateInstance( builtType );
            Guid i = Guid.NewGuid();
            Assert.That( o.M( out i ), Is.EqualTo( 0 ) );
            Assert.That( i, Is.EqualTo( Guid.Empty ) );
        }

        [Test]
        public void AutoImplementStubRefGuid()
        {
            Type t = typeof( H );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            H o = (H)Activator.CreateInstance( builtType );
            Guid iOrigin = Guid.NewGuid();
            Guid i = iOrigin;
            Assert.That( o.M( ref i ), Is.EqualTo( 0 ) );
            Assert.That( i, Is.EqualTo( iOrigin ) );
        }

        [Test]
        public void AutoImplementStubOutClass()
        {
            Type t = typeof( I );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            I o = (I)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;
            Assert.That( o.M( out c ), Is.EqualTo( 0 ) );
            Assert.That( c, Is.Null );
        }

        [Test]
        public void AutoImplementStubRefClass()
        {
            Type t = typeof( J );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            J o = (J)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;
            Assert.That( o.M( ref c ), Is.EqualTo( 0 ) );
            Assert.That( c, Is.SameAs( cOrigin ) );
        }

        #endregion

        #region EmitHelper.ImplementEmptyStubProperty tests
        
        // Note: 
        // Abstract properties cannot have private accessors.
        public abstract class PA
        {
            public abstract int PublicWriteableValue { get; set; }
            public abstract int ProtectedWriteableValue { get; protected set; }

            public int PublicProperty { get; protected set; }

            public void SetProtectedValues( int v )
            {
                ProtectedWriteableValue = v;
                PublicProperty = v%255;
            }
        }

        public abstract class PB : PA
        {
            public new byte PublicProperty { get { return (byte)base.PublicProperty; } }
        }

        [Test]
        public void AutoImplementStubProperty()
        {
            Type tA = typeof( PA );
            TypeBuilder bA = CreateTypeBuilder( tA );
            EmitHelper.ImplementStubProperty( bA, tA.GetProperty( "PublicWriteableValue" ), true );
            EmitHelper.ImplementStubProperty( bA, tA.GetProperty( "ProtectedWriteableValue" ), true );
            Type builtTypeA = bA.CreateType();
            PA oA = (PA)Activator.CreateInstance( builtTypeA );
            oA.PublicWriteableValue = 4548;
            oA.SetProtectedValues( 2121 );
            Assert.That( oA.PublicWriteableValue, Is.EqualTo( 4548 ) );
            Assert.That( oA.ProtectedWriteableValue, Is.EqualTo( 2121 ) );

            Type tB = typeof( PB );
            TypeBuilder bB = CreateTypeBuilder( tB );
            EmitHelper.ImplementStubProperty( bB, tB.GetProperty( "PublicWriteableValue" ), true );
            EmitHelper.ImplementStubProperty( bB, tB.GetProperty( "ProtectedWriteableValue" ), true );
            Type builtTypeB = bB.CreateType();
            PB oB = (PB)Activator.CreateInstance( builtTypeB );
            oB.PublicWriteableValue = 4548;
            oB.SetProtectedValues( 2121 );
            Assert.That( oB.PublicWriteableValue, Is.EqualTo( 4548 ) );
            Assert.That( oB.ProtectedWriteableValue, Is.EqualTo( 2121 ) );
            Assert.That( oB.PublicProperty, Is.EqualTo( 2121%255 ) );
        }

        public abstract class CNonVirtualProperty
        {
            int _value;

            public CNonVirtualProperty()
            {
                _value = 654312;
            }

            public int PublicProperty { get { return _value * 2; } set { _value = value * 2; } }
        }

        [Test]
        public void AutoImplementStubForNonVirtualPropertyIsStupid()
        {
            Type tN = typeof( CNonVirtualProperty );
            TypeBuilder bN = CreateTypeBuilder( tN );
            EmitHelper.ImplementStubProperty( bN, tN.GetProperty( "PublicProperty" ), true );
            Type builtTypeN = bN.CreateType();
            CNonVirtualProperty oN = (CNonVirtualProperty)Activator.CreateInstance( builtTypeN );
            Assert.That( oN.PublicProperty, Is.EqualTo( 654312 * 2 ) );
            oN.PublicProperty = 2;
            Assert.That( oN.PublicProperty, Is.EqualTo( 2 * 4 ) );
        }

        public abstract class CVirtualProperty
        {
            int _value;

            public CVirtualProperty()
            {
                _value = 654312;
            }

            public virtual int PublicProperty { get { return _value * 2; } set { _value = value * 2; } }
        }

        [Test]
        public void AutoImplementStubForVirtualPropertyActuallyReplacesIt()
        {
            Type t = typeof( CVirtualProperty );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubProperty( b, t.GetProperty( "PublicProperty" ), false );
            Type builtType = b.CreateType();
            CVirtualProperty o = (CVirtualProperty)Activator.CreateInstance( builtType );
            Assert.That( o.PublicProperty, Is.EqualTo( 0 ), "Initial value is lost." );
            o.PublicProperty = 2;
            Assert.That( o.PublicProperty, Is.EqualTo( 2 ), "Mere stub implementation does its job." );
        }

        #endregion
    }
}
