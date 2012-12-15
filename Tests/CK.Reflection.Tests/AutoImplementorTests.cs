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

namespace CK.Reflection.Tests
{
    [TestFixture]
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

        //abstract class ABase
        //{
        //    public ABase CallFirstMethod( int i )
        //    {
        //        return FirstMethod( i );
        //    }

        //    protected abstract ABase FirstMethod( int i );

        //    public abstract int M2( ref int i, ref ABase a );

        //    public abstract Guid M3( out int i, out ABase a );
        //}

        //class ABase_VImpl : ABase
        //{
        //    protected override ABase FirstMethod( int i )
        //    {
        //        return FirstMethod_VImpl( i );
        //    }

        //    public override int M2( ref int i, ref ABase a )
        //    {
        //        return M2_VImpl( ref i, ref a );
        //    }

        //    public override Guid M3( out int i, out ABase a )
        //    {
        //        return M3_VImpl( out i, out a );
        //    }
            
        //    protected virtual ABase FirstMethod_VImpl( int i )
        //    {
        //        return null;
        //    }

        //    protected virtual int M2_VImpl( ref int i, ref ABase a )
        //    {
        //        return 0;
        //    }

        //    protected virtual Guid M3_VImpl( out int i, out ABase a )
        //    {
        //        i = 0;
        //        a = null;
        //        return new Guid();
        //    }

        //    protected virtual T M3_VImpl<T>( out int i, out ABase a ) where T : new()
        //    {
        //        i = 0;
        //        a = null;
        //        return new T();
        //    }
        //}

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

        [Test]
        public void AutoImplementStubReturnsClassAndProtected()
        {
            Type t = typeof( A );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "FirstMethod", BindingFlags.Instance | BindingFlags.NonPublic ), false );
            Type builtType = b.CreateType();
            A o = (A)Activator.CreateInstance( builtType );
            Assert.That( o.CallFirstMethod( 10 ), Is.Null );
        }

        [Test]
        public void AutoImplementStubReturnsInt()
        {
            Type t = typeof( B );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            B o = (B)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( 0 ) );
        }

        [Test]
        public void AutoImplementStubReturnsShort()
        {
            Type t = typeof( C );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            C o = (C)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( 0 ) );
        }

        [Test]
        public void AutoImplementStubReturnsGuid()
        {
            Type t = typeof( D );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            D o = (D)Activator.CreateInstance( builtType );
            Assert.That( o.M( 10 ), Is.EqualTo( Guid.Empty ) );
        }

        [Test]
        public void AutoImplementStubRefInt()
        {
            Type t = typeof( E );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
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
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
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
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
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
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
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
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
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
            EmitHelper.ImplementStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateType();
            J o = (J)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;
            Assert.That( o.M( ref c ), Is.EqualTo( 0 ) );
            Assert.That( c, Is.SameAs( cOrigin ) );
        }

    }
}
