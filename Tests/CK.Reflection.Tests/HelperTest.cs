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
using FluentAssertions;

namespace CK.Reflection.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category("Reflection")]
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
                i.Name.Should().Be( "Length" );
                i.PropertyType.Should().BeSameAs( typeof( int ) );

                Should.Throw<ArgumentException>(    () => ReflectionHelper.GetPropertyInfo( oneInstance, s => s.IndexOf( 'e' ) ) );
            }
            {
                // Same as before, but default() is used to "obtain" an instance of the holder type.
                // To avoid this, next versions can be used.
                PropertyInfo i = ReflectionHelper.GetPropertyInfo( default( KeyValuePair<int, long> ), p => p.Value );
                i.Name.Should().Be( "Value" );
                i.PropertyType.Should().BeSameAs( typeof( long ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified).
                PropertyInfo i = ReflectionHelper.GetPropertyInfo<string>( s => s.Length );
                i.Name.Should().Be( "Length" );
                i.PropertyType.Should().BeSameAs( typeof( int ) );
                
                Should.Throw<ArgumentException>( () => ReflectionHelper.GetPropertyInfo<string>( s => s.IndexOf( 'e' ) ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified),
                // and enables property type checking.
                //
                // PropertyInfo iFail = Helper.GetPropertyInfo<string, byte>( s => s.Length );
                PropertyInfo i = ReflectionHelper.GetPropertyInfo<string, int>( s => s.Length );
                i.Name.Should().Be( "Length" );
                i.PropertyType.Should().BeSameAs( typeof( int ) );
            }

             {
                // This version uses the closure to capture the reference to the property.
                PropertyInfo i = ReflectionHelper.GetPropertyInfo( () => AnIntProperty );
                i.Name.Should().Be( "AnIntProperty" );
                i.PropertyType.Should().BeSameAs( typeof( int ) );

                PropertyInfo i2 = ReflectionHelper.GetPropertyInfo( () => i.Name );
                i2.Name.Should().Be( "Name" );
                i2.PropertyType.Should().BeSameAs( typeof( string ) );

                byte[] anArray = new byte[1]; 
                Should.Throw<ArgumentException>( () => ReflectionHelper.GetPropertyInfo( () => anArray[0] ) );
            }
            {
                // This version uses the closure to capture the reference to the property
                // and enables property type checking.
                
                // PropertyInfo iFail = Helper.GetPropertyInfo<string>( () => AnIntProperty );

                PropertyInfo i = ReflectionHelper.GetPropertyInfo<int>( () => AnIntProperty );
                i.Name.Should().Be( "AnIntProperty" );
                i.PropertyType.Should().BeSameAs( typeof( int ) );

                PropertyInfo i2 = ReflectionHelper.GetPropertyInfo<string>( () => i.Name );
                i2.Name.Should().Be( "Name" );
                i2.PropertyType.Should().BeSameAs( typeof( string ) );
                
                Should.Throw<ArgumentException>( () => ReflectionHelper.GetPropertyInfo( () => i2.Name.ToString() ) );
            }
        }

        [Test]
        public void PropertySetter()
        {
            {
                string s = "a string";
                Should.Throw<InvalidOperationException>( () => DelegateHelper.CreateSetter( s, x => x.Length ) );
                DelegateHelper.CreateSetter( s, x => x.Length, DelegateHelper.CreateInvalidSetterOption.NullAction )
                    .Should().BeNull();
                var p = DelegateHelper.CreateSetter( s, x => x.Length, DelegateHelper.CreateInvalidSetterOption.VoidAction );
                p( s, 4554 );
            }
            {
                System.IO.StringWriter a = new System.IO.StringWriter();
                var setter = DelegateHelper.CreateSetter( a, x => x.NewLine );
                a.NewLine.Should().Be( Environment.NewLine );
                setter( a, "Hello World!" );
                a.NewLine.Should().Be( "Hello World!" );
            }
        }

        [Test]
        public void Parameters()
        {
            var bindingJustForTest = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
            MethodInfo m = typeof( HelperTest ).GetMethod( "JustForTest", bindingJustForTest );
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters() );
                p[0].Should().BeSameAs( typeof( int ) );
                p[1].Should().BeSameAs( typeof( HelperTest ) );
                p.Length.Should().Be( 2 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), 0 );
                p[0].Should().BeSameAs(typeof( int ) );
                p[1].Should().BeSameAs(typeof( HelperTest ) );
                p.Length.Should().Be( 2 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), 1 );
                p[0].Should().BeSameAs( typeof( HelperTest ) );
                p.Length.Should().Be( 1 );
            }
            {
                Type[] p = ReflectionHelper.CreateParametersType( m.GetParameters(), typeof( CategoryAttribute ) );
                p[0].Should().BeSameAs(typeof( CategoryAttribute ) );
                p[1].Should().BeSameAs(typeof( int ) );
                p[2].Should().BeSameAs(typeof( HelperTest ) );
                p.Length.Should().Be( 3 );
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
            typeof( IB ).GetProperties( BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                .Single().Name.Should().Be( "PropB", 
                "PropA missed. FlattenHierarchy is totally useless on interfaces..." );
            ReflectionHelper.GetFlattenProperties( typeof( IB ) )
                .Should().HaveCount( 2, "ReflectionHelper.GetFlattenProperties() does the job." );

            var BProperties = typeof( B ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            BProperties.Select( p => p.Name )
                .Should().BeEquivalentTo( new[] { "PrivatePropOnB", "PropA", "PropB", "ProtectedPropOnA", "ProtectedPropOnB" },
                @"GetProperties() is okay for class properties!... 
                  But be careful: private for the targeted type is returned, not base classes' private properties." );

            var BPublicProperties = typeof( B ).GetProperties( BindingFlags.Public | BindingFlags.Instance );
            BPublicProperties.Select( p => p.Name ).Should().BeEquivalentTo( new[] { "PropA", "PropB" },
                @"GetProperties(Public|Instance) do not return protected properties." );

            var CProperties = typeof( C ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            CProperties.Select( p => p.Name ).Should().BeEquivalentTo( new[] { "CK.Reflection.Tests.HelperTest.IC.PropA", "PropA", "ProtectedPropOnA" },
                "GetProperties() on class can catch explicit implementation (see ExplicitImplementation() test below)." );

            var explicitProp = CProperties.First();
            explicitProp.Name.Should().Contain( ".", "This is the only way to detect an explicitely implemented property (or method)." );

            var DProperties = typeof( D ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            DProperties.Select( p => p.Name ).Should().BeEquivalentTo( new[] { "PropA", "PropA", "PropD", "ProtectedPropOnA", "ZePrivateOnD" },
                @"Explicit implementations are like private: they are caught only for the target type...
                  and unfortunately, masked property appear multiple times :-(" );

            // To get properties that are public or protected but not private nor explicitely implemented on the target type, use:
            // (Those properties are guaranteed to remain acccessible even when the class is specialized.)
            // We require that BOTH getter and setter are NOT private since if one of them is private, the "Property Covariance" trick can not be implemented.
            // 
            var cleanCProperties = typeof( C ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                                                .Where( p => !p.Name.Contains( '.' ) && !p.GetSetMethod(true).IsPrivate && !p.GetGetMethod(true).IsPrivate );

            cleanCProperties.Select( p => p.Name ).Should().BeEquivalentTo( new[] { "PropA", "ProtectedPropOnA" } );

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
            orderedD.Select( p => p.Name ).Should().BeEquivalentTo( new[] { "PropA", "PropD", "ProtectedPropOnA" } );
            orderedD[0].DeclaringType.Should().BeSameAs( typeof( D ) ); 
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

                typeof( PB ).GetProperty( "PropAVirtual" ).GetCustomAttributes( typeof( PropNOTInheritedAttribute ), inherit: true )
                    .Should().BeEmpty( "OK: PropNOTInherited is not available on B.PropAVirtual" );

                typeof( PB ).GetProperty( "PropA" ).GetCustomAttributes( typeof( PropInheritedAttribute ), inherit: true )
                    .Should().BeEmpty( "KO! PropInheritedAttribute is ALSO NOT available on B.PropAVirtual. PropertyInfo.GetCustomAttributes does NOT honor bool inherit parameter :-(.");

                // To get it, one must use static methods on Attribute class.

                // REVIEW: statics methods an Attribute does no longer exists.
                // Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropAVirtual" ), typeof( PropNOTInheritedAttribute ), inherit: true ).Length == 0,
                //                  "OK: PropNOTInherited is not available on B.PropAVirtual" );
                // Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropAVirtual" ), typeof( PropInheritedAttribute ), inherit: true ).Length == 1,
                //     "OK! It works as it should!" );
            }
            {
                // Case 2:
                // Inheritance does not not work for Masked properties.
                
                // REVIEW: statics methods an Attribute does no longer exists.
                // Assert.That( Attribute.GetCustomAttributes( typeof( PB ).GetProperty( "PropA" ), typeof( PropInheritedAttribute ), inherit: true ).Length == 0,
                //     "No attribute inheritance here, a Masked property is a 'new' property :-)" );
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
                m0.Should().BeNull( "Start is not found..." );

                typeof(IExplicit).FullName.Should().Be( "CK.Reflection.Tests.HelperTest+IExplicit" );
                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace( '+', '.' ), BindingFlags.Instance | BindingFlags.NonPublic );
                m1.Should().NotBeNull();
                m1.Invoke( c, null ).Should().Be( true );
                m1.DeclaringType.Should().BeSameAs( c.GetType(), "To obtain an explicit implementation, one can use the FullName of the properties, ignoring nested class marker (+)." );

                c.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).Should().NotBeEmpty( "Default ctor is accessible." );
            }
            {
                var c = new ImplicitImpl();
                MethodInfo m0 = c.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                m0.Should().NotBeNull( "Start exists." );

                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace( '+', '.' ), BindingFlags.Instance | BindingFlags.NonPublic );
                m1.Should().BeNull( "But the explicit does not exist... Implicit hides explicit implementation." );
            }
            {
                ExplicitAndImplicitImpl c2 = new ExplicitAndImplicitImplSpecialized();
                MethodInfo m0 = c2.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                m0.Should().NotBeNull( "Found the exposed one." );
                m0.Invoke( c2, null ).Should().Be( false );

                MethodInfo m1 = c2.GetType().GetMethod( "CK.Reflection.Tests.HelperTest+IExplicit.Start".Replace('+', '.'), BindingFlags.Instance | BindingFlags.NonPublic );
                m1.Should().NotBeNull();
                m1.Invoke( c2, null ).Should().Be( true );
                m1.DeclaringType.Should().BeSameAs( typeof( ExplicitAndImplicitImpl ), "Both exist, both are found." );

                c2.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters().Should().BeEmpty( "The .ctor() is accessible." );
                typeof( ExplicitAndImplicitImpl ).GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters()[0].ParameterType
                    .Should().BeSameAs( typeof( int ), "The protected .ctor(int) is accessible." );
            }
        }
        #endregion

    }
}
