using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Reflection.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    class AttributesBehavior
    {

        interface IMarker
        {
        }

        [AttributeUsage( AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property )]
        class MarkerAttribute : Attribute, IMarker
        {
        }

        [AttributeUsage( AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property )]
        class Marker2Attribute : Attribute, IMarker
        {
        }

        [Marker]
        class Test
        {
            [Marker]
            public void Method() { }

            [Marker]
            [Marker2]
            public void Method2() { }
        }

        [Test]
        public void WorksWithAbstractions()
        {
            Assert.That( typeof( Test ).IsDefined( typeof( IMarker ), false ), Is.True, "IsDefined works with any base type of attributes." );
            Assert.That( typeof( Test ).GetMethod( "Method" ).IsDefined( typeof( IMarker ), false ), Is.True, "IsDefined works with any base type of attributes." );

            Assert.That( typeof( Test ).GetMethod( "Method2" ).IsDefined( typeof( IMarker ), false ), Is.True, "IsDefined works with multiple attributes." );
            Assert.That( typeof( Test ).GetMethod( "Method2" ).GetCustomAttributes( typeof( IMarker ), false ).Length, Is.EqualTo( 2 ), "GetCustomAttributes works with multiple base type attributes." );

        }
        
        [Test]
        public void CreatedEachTimeGetCustomAttributesIsCalled()
        {
            object a1 = typeof( Test ).GetMethod( "Method" ).GetCustomAttributes( typeof( IMarker ), false )[0];
            object a2 = typeof( Test ).GetMethod( "Method" ).GetCustomAttributes( typeof( IMarker ), false )[0];
            Assert.That( a1, Is.Not.SameAs( a2 ) );
        }
    }
}
