#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Reflection.Tests\HelperTest.cs) is part of CiviKey. 
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
    public class HelperTest
    {
        public int AnIntProperty { get { return 3; } }

        [Test]
        public void PropertyInfoThroughLambda()
        {
            {
                // Both types are inferred (holder and property type).
                // Requires an instance of the holder.
                string oneInstance = null;
                PropertyInfo i = ReflectionHelper.GetPropertyInfo( oneInstance, s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                Assert.Throws<ArgumentException>( () => ReflectionHelper.GetPropertyInfo( oneInstance, s => s.IndexOf( 'e' ) ) );
            }
            {
                // Same as before, but default() is used to "obtain" an instance of the holder type.
                // To avoid this, next versions can be used.
                PropertyInfo i = ReflectionHelper.GetPropertyInfo( default( KeyValuePair<int, long> ), p => p.Value );
                Assert.That( i.Name, Is.EqualTo( "Value" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( long ) ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified).
                PropertyInfo i = ReflectionHelper.GetPropertyInfo<string>( s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );
                
                Assert.Throws<ArgumentException>( () => ReflectionHelper.GetPropertyInfo<string>( s => s.IndexOf( 'e' ) ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified),
                // and enables property type checking.
                //
                // PropertyInfo iFail = Helper.GetPropertyInfo<string, byte>( s => s.Length );
                PropertyInfo i = ReflectionHelper.GetPropertyInfo<string, int>( s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );
            }

            {
                // This version uses the closure to capture the reference to the property.
                PropertyInfo i = ReflectionHelper.GetPropertyInfo( () => AnIntProperty );
                Assert.That( i.Name, Is.EqualTo( "AnIntProperty" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                PropertyInfo i2 = ReflectionHelper.GetPropertyInfo( () => i.Name );
                Assert.That( i2.Name, Is.EqualTo( "Name" ) );
                Assert.That( i2.PropertyType, Is.SameAs( typeof( string ) ) );

                Assert.Throws<ArgumentException>( () => ReflectionHelper.GetPropertyInfo( () => AppDomain.CurrentDomain.ActivationContext.ApplicationManifestBytes[4] ) );
            }
            {
                // This version uses the closure to capture the reference to the property
                // and enables property type checking.
                
                // PropertyInfo iFail = Helper.GetPropertyInfo<string>( () => AnIntProperty );

                PropertyInfo i = ReflectionHelper.GetPropertyInfo<int>( () => AnIntProperty );
                Assert.That( i.Name, Is.EqualTo( "AnIntProperty" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                PropertyInfo i2 = ReflectionHelper.GetPropertyInfo<string>( () => i.Name );
                Assert.That( i2.Name, Is.EqualTo( "Name" ) );
                Assert.That( i2.PropertyType, Is.SameAs( typeof( string ) ) );
                
                Assert.Throws<ArgumentException>( () => ReflectionHelper.GetPropertyInfo( () => i2.Name.Clone() ) );

            }
        }

        [Test]
        public void PropertySetter()
        {
            {
                string s = "a string";
                Assert.Throws<InvalidOperationException>( () => ReflectionHelper.CreateSetter( s, x => x.Length ) );
                Assert.That( ReflectionHelper.CreateSetter( s, x => x.Length, ReflectionHelper.CreateInvalidSetterOption.NullAction ), Is.Null );
                var p = ReflectionHelper.CreateSetter( s, x => x.Length, ReflectionHelper.CreateInvalidSetterOption.VoidAction );
                p( s, 4554 );
            }
            {
                // NUnit.Framework.TestAttribute is an object with a public read/write property...
                TestAttribute a = new TestAttribute();
                var setter = ReflectionHelper.CreateSetter( a, x => x.Description );
                Assert.That( a.Description, Is.Null );
                setter( a, "Hello World!" );
                Assert.That( a.Description, Is.EqualTo( "Hello World!" ) );
            }
        }

        [Test]
        public void Parameters()
        {
            var bindingJustForTest = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
            MethodInfo m = typeof( HelperTest ).GetMethod( "JustForTest", bindingJustForTest );
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters() );
                Assert.That( p[0] == typeof( int ) );
                Assert.That( p[1] == typeof( HelperTest ) );
                Assert.That( p.Length == 2 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), 0 );
                Assert.That( p[0] == typeof( int ) );
                Assert.That( p[1] == typeof( HelperTest ) );
                Assert.That( p.Length == 2 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), 1 );
                Assert.That( p[0] == typeof( HelperTest ) );
                Assert.That( p.Length == 1 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), typeof( CategoryAttribute ) );
                Assert.That( p[0] == typeof( CategoryAttribute ) );
                Assert.That( p[1] == typeof( int ) );
                Assert.That( p[2] == typeof( HelperTest ) );
                Assert.That( p.Length == 3 );
            }
        }

        delegate void JustForTestDelegate( int i, HelperTest obj );

        static void JustForTest( int i, HelperTest obj )
        {
        }

        #region Properties: Flatten or not ?

        public interface IA
        {
            IA PropA { get; set; }
        }

        public interface IB : IA
        {
            IB PropB { get; set; }
        }

        public interface IC : IA
        {
            new IC PropA { get; set; }
        }

        class A : IA
        {
            int PrivatePropOnA { get; set; }
            protected int ProtectedPropOnA { get; set; }
            public IA PropA { get; set; }
        }

        class B : A
        {
            int PrivatePropOnB { get; set; }
            protected int ProtectedPropOnB { get; set; }
            public IB PropB { get; set; }
        }

        class C : A, IC
        {
            IC IC.PropA { get { return (IC)base.PropA; } set { base.PropA = (IC)value; } }
        }

        class D : C
        {
            // Masked definition of PropA in order to specializes its type.
            // This IS NOT a good idea in general... BUT when (and only when) the property setting
            // is totally under control (by a DI framework for instance), this enables "Property Covariance"... and its great !! :-)
            public new IB PropA { get { return (IB)base.PropA; } set { base.PropA = value; } }
            public int PropD { get; set; }
            int ZePrivateOnD { get; set; }
        }

        [Test]
        public void FlattenProperties()
        {
            Assert.That( typeof( IB ).GetProperties( BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ).Single().Name, Is.EqualTo( "PropB" ), 
                "PropA missed. FlattenHierarchy is totally useless on interfaces..." );
            Assert.That( ReflectionHelper.GetFlattenProperties( typeof( IB ) ).Count(), Is.EqualTo( 2 ), "ReflectionHelper.GetFlattenProperties() does the job." );

            var BProperties = typeof( B ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            Assert.That( BProperties.Select( p => p.Name ).OrderBy( n => n ).ToArray(), Is.EquivalentTo( new[] { "PrivatePropOnB", "PropA", "PropB", "ProtectedPropOnA", "ProtectedPropOnB" } ),
                @"GetProperties() is okay for class properties!... 
                  But be careful: private for the targeted type is returned, not base classes' private properties." );

            var BPublicProperties = typeof( B ).GetProperties( BindingFlags.Public | BindingFlags.Instance );
            Assert.That( BPublicProperties.Select( p => p.Name ).OrderBy( n => n ).ToArray(), Is.EquivalentTo( new[] { "PropA", "PropB" } ),
                @"GetProperties(Public|Instance) do not return protected properties." );

            var CProperties = typeof( C ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            Assert.That( CProperties.Select( p => p.Name ).OrderBy( n => n ).ToArray(), Is.EquivalentTo( new[] { "CK.Reflection.Tests.HelperTest.IC.PropA", "PropA", "ProtectedPropOnA" } ),
                "GetProperties() on class can catch explicit implementation (see ExplicitImplementation() test below)." );

            var explicitProp = CProperties.First();
            Assert.That( explicitProp.Name.Contains( '.' ), "This is the only way to detect an explicitely implemented property (or method)." );

            var DProperties = typeof( D ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            Assert.That( DProperties.Select( p => p.Name ).OrderBy( n => n ).ToArray(), Is.EquivalentTo( new[] { "PropA", "PropA", "PropD", "ProtectedPropOnA", "ZePrivateOnD" } ),
                @"Explicit implementations are like private: they are caught only for the target type...
                  and unfortunately, masked property appear multiple times :-(" );

            // To get properties that are public or protected but not private nor explicitely implemented on the target type, use:
            // (Those properties are guaranteed to remain acccessible even when the class is specialized.)
            // We require that BOTH getter and setter are NOT private since if one of them is private, the "Property Covariance" trick can not be implemented.
            // 
            var cleanCProperties = typeof( C ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                                                .Where( p => !p.Name.Contains( '.' ) && !p.GetSetMethod(true).IsPrivate && !p.GetGetMethod(true).IsPrivate );

            Assert.That( cleanCProperties.Select( p => p.Name ).OrderBy( n => n ).ToArray(), Is.EquivalentTo( new[] { "PropA", "ProtectedPropOnA" } ) );

            // And now... Fighting against the masked property :-(
            var cleanDProperties = typeof( D ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ).Where( p => !p.Name.Contains( '.' ) );
            Dictionary<string,PropertyInfo> byName = new Dictionary<string, PropertyInfo>();
            foreach( var p in cleanDProperties )
            {
                if( p.GetSetMethod( true ).IsPrivate || p.GetGetMethod( true ).IsPrivate )
                {
                    // Warning: not a "Property Covariance" compliant property since
                    // specialized classes will not be able to "override" its signature.
                }
                else
                {
                    PropertyInfo existing;
                    if( byName.TryGetValue( p.Name, out existing ) )
                    {
                        // Which one is the "best one" ?
                        // If a duplicate occurs, there is necessarily at least one that is masked and it is the most specialized one.
                        // But may be the 2 are masks... The simplest thing to do is to keep the most specialized one.
                        if( existing.DeclaringType.IsAssignableFrom( p.DeclaringType ) )
                        {
                            // No luck, p is better than existing. Swap.
                            byName[p.Name] = p;
                        }
                    }
                    else byName.Add( p.Name, p );
                }
            }
            var orderedD = byName.Values.OrderBy( p => p.Name ).ToArray();
            Assert.That( orderedD.Select( p => p.Name ).ToArray(), Is.EquivalentTo( new[] { "PropA", "PropD", "ProtectedPropOnA" } ) );
            Assert.That( orderedD[0].DeclaringType, Is.SameAs( typeof( D ) ) ); 
        }

        #endregion

        #region Properties inheritance & Attributes

        [AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
        public class PropInheritedAttribute : Attribute
        {
        }

        [AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
        public class PropNOTInheritedAttribute : Attribute
        {
        }

        public class PA
        {
            [PropInheritedAttribute]
            [PropNOTInheritedAttribute]
            public int PropA { get; set; }
            
            [PropInheritedAttribute]
            [PropNOTInheritedAttribute]
            public virtual int PropAVirtual { get; set; }
            
        }

        public class PB : PA
        {
            public new int PropA { get { return base.PropA; } set { base.PropA = value; } }
            public override int PropAVirtual { get { return base.PropAVirtual; } set { base.PropAVirtual = value; } }
        }

        [Test]
        public void PropertyInheritanceAndAttributes()
        {
            {
                // Case 1:
                /// the virtual/override case works nearly as it should (must use static methods on Attribute class).

                Assert.That( typeof( PB ).GetProperty( "PropAVirtual" ).GetCustomAttributes( typeof( PropNOTInheritedAttribute ), inherit: true ).Length == 0,
                    "OK: PropNOTInherited is not available on B.PropAVirtual" );
                Assert.That( typeof( PB ).GetProperty( "PropA" ).GetCustomAttributes( typeof( PropInheritedAttribute ), inherit: true ).Length == 0,
                    "KO! PropInheritedAttribute is ALSO NOT available on B.PropAVirtual. PropertyInfo.GetCustomAttributes does NOT honor bool inherit parameter :-(." );

                // To get it, one must use static methods on Attribute class.

                Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropAVirtual" ), typeof( PropNOTInheritedAttribute ), inherit: true ).Length == 0,
                                 "OK: PropNOTInherited is not available on B.PropAVirtual" );
                Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropAVirtual" ), typeof( PropInheritedAttribute ), inherit: true ).Length == 1,
                    "OK! It works as it should!" );
            }
            {
                // Case 2:
                // Inheritance does not not work for Masked properties.
                Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropA" ), typeof( PropInheritedAttribute ), inherit: true ).Length == 0,
                    "No attribute inheritance here, a Masked property is a 'new' property :-)" );
            }
        }


        #endregion

        #region Explicit & Implicit implementation

        interface IExplicit
        {
            bool Start();
        }

        class ImplicitImpl : IExplicit
        {
            public bool Start() { return true; }
        }

        class ExplicitImpl : IExplicit
        {
            bool IExplicit.Start() { return true; }
        }

        class ExplicitAndImplicitImpl : IExplicit
        {
            protected ExplicitAndImplicitImpl( int i )
            {
            }

            bool IExplicit.Start() { return true; }
            public bool Start() { return false; }
        }
        
        class ExplicitAndImplicitImplSpecialized : ExplicitAndImplicitImpl
        {
            public ExplicitAndImplicitImplSpecialized()
                : base( 0 )
            {
            }
        }

        [Test]
        public void ExplicitImplementation()
        {
            {
                var c = new ExplicitImpl();
                MethodInfo m0 = c.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Null, "Start is not found..." );

                Assert.That( typeof(IExplicit).FullName, Is.EqualTo( "CK.Reflection.Tests.HelperTest+IExplicit" ) );
                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace( '+', '.' ), BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Not.Null );
                Assert.That( m1.Invoke( c, null ), Is.True );
                Assert.That( m1.DeclaringType, Is.EqualTo( c.GetType() ), "To obtain an explicit implementation, one can use the FullName of the properties, ignoring nested class marker (+)." );

                Assert.That( c.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).Length > 0, "Default ctor is accessible." );
            }
            {
                var c = new ImplicitImpl();
                MethodInfo m0 = c.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Not.Null, "Start exists." );

                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace( '+', '.' ), BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Null, "But the explicit does not exist... Implicit hides explicit implementation." );
            }
            {
                ExplicitAndImplicitImpl c2 = new ExplicitAndImplicitImplSpecialized();
                MethodInfo m0 = c2.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Not.Null, "Found the exposed one." );
                Assert.That( m0.Invoke( c2, null ), Is.False );

                MethodInfo m1 = c2.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace('+', '.'), BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Not.Null );
                Assert.That( m1.Invoke( c2, null ), Is.True );
                Assert.That( m1.DeclaringType, Is.EqualTo( typeof( ExplicitAndImplicitImpl ) ), "Both exist, both are found." );

                Assert.That( c2.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters().Length == 0, "The .ctor() is accessible." );
                Assert.That( typeof( ExplicitAndImplicitImpl ).GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters()[0].ParameterType,
                    Is.EqualTo( typeof( int ) ), "The protected .ctor(int) is accessible." );
            }
        }
        #endregion

    }
}
