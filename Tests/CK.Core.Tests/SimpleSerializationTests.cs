using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class SimpleSerializationTests
    {
        class Sample : ICKSimpleBinarySerializable
        {
            public readonly int Power;
            public readonly string Name;
            public readonly short? Age;

            public Sample( int power, string name, short? age )
            {
                Throw.CheckNotNullOrEmptyArgument( name );
                Power = power;
                Name = name;
                Age = age;
            }

            public Sample( ICKBinaryReader r )
            {
                r.ReadByte(); // Version
                Power = r.ReadInt32();
                Name = r.ReadString();
                Age = r.ReadNullableInt16();
            }

            public void Write( ICKBinaryWriter w )
            {
                w.Write( (byte)0 ); // Version
                w.Write( Power );
                w.Write( Name );
                w.WriteNullableInt16( Age );
            }

        }

        [Test]
        public void SimpleSeralizer_on_simple()
        {
            var o = new Sample( 87, "Hop", null );
            var bytes = o.SerializeSimple();
            var backO = SimpleSerializable.DeserializeSimple<Sample>( bytes );
            backO.Should().BeEquivalentTo( o );

            SimpleSerializable.DeepCloneSimple( o ).Should().BeEquivalentTo( o );
        }


        [SerializationVersion( 42 )]
        sealed class Thing : ICKVersionedBinarySerializable
        {
            // Name was nullable before v42.
            // Now it is necessarily not null, empty or white space.
            public readonly string Name;

            public Thing( string name )
            {
                Throw.CheckNotNullOrWhiteSpaceArgument( name );
                Name = name;
            }

            public Thing( ICKBinaryReader r, int version )
            {
                if( version < 42 )
                {
                    Name = r.ReadNullableString() ?? "(no name)";
                }
                else
                {
                    Name = r.ReadString();
                }
            }

            public void WriteData( ICKBinaryWriter w )
            {
                w.Write( Name );
            }
        }

        [Test]
        public void SimpleSeralizer_on_Versioned()
        {
            var o = new Thing( "Hop" );
            var bytes = o.SerializeVersioned();
            var backO = SimpleSerializable.DeserializeVersioned<Thing>( bytes );
            backO.Should().BeEquivalentTo( o );

            SimpleSerializable.DeepCloneVersioned( o ).Should().BeEquivalentTo( o );
        }


        /// <summary>
        /// Supporting both interfaces enables simple scenario to use the embedded version
        /// (to be used when not too many instances must be serialized) or use the shared version
        /// (when many instances must be serialized).
        /// </summary>
        [SerializationVersion( 3712 )]
        sealed class CanSupportBothSimpleSerialization : ICKSimpleBinarySerializable, ICKVersionedBinarySerializable
        {
            public string? Data { get; set; }

            public CanSupportBothSimpleSerialization()
            {
            }

            /// <summary>
            /// Simple deserialization constructor.
            /// </summary>
            /// <param name="r">The reader.</param>
            public CanSupportBothSimpleSerialization( ICKBinaryReader r )
                : this( r, r.ReadSmallInt32() )
            {
            }

            /// <summary>
            /// Versioned deserialization constructor.
            /// </summary>
            /// <param name="r">The reader.</param>
            /// <param name="version">The saved version number.</param>
            public CanSupportBothSimpleSerialization( ICKBinaryReader r, int version )
            {
                // Use the version as usual.
                Data = r.ReadNullableString();
            }

            public void Write( ICKBinaryWriter w )
            {
                // Using a Debug.Assert here avoids the cost of the reflexion.
                Debug.Assert( SerializationVersionAttribute.GetRequiredVersion( GetType() ) == 3712 );
                w.WriteSmallInt32( 3712 );
                WriteData( w );
            }

            public void WriteData( ICKBinaryWriter w )
            {
                // The version is externally managed.
                w.WriteNullableString( Data );
            }
        }

        [Test]
        public void SimpleSeralizer_on_Simple_and_Versioned()
        {
            var o = new CanSupportBothSimpleSerialization() { Data = "some data" };
            {
                var bytes = o.SerializeVersioned();
                var backO = SimpleSerializable.DeserializeVersioned<CanSupportBothSimpleSerialization>( bytes );
                backO.Should().BeEquivalentTo( o );

                SimpleSerializable.DeepCloneVersioned( o ).Should().BeEquivalentTo( o );
            }
            {
                var bytes = o.SerializeSimple();
                var backO = SimpleSerializable.DeserializeSimple<CanSupportBothSimpleSerialization>( bytes );
                backO.Should().BeEquivalentTo( o );

                SimpleSerializable.DeepCloneSimple( o ).Should().BeEquivalentTo( o );
                o.DeepClone().Should().BeEquivalentTo( o );
            }
        }




    }
}
