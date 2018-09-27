using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class BinaryReaderWriterTests
    {
        public static readonly string DefString = "MultiPropertyType";
        public static readonly Guid DefGuid = new Guid( "4F5E996D-51E9-4B04-B572-5126B14A5ECA" );
        public static readonly int DefInt32 = -42;
        public static readonly uint DefUInt32 = 42;
        public static readonly long DefInt64 = -42 << 48;
        public static readonly ulong DefUInt64 = 42 << 48;
        public static readonly short DefInt16 = -3712;
        public static readonly ushort DefUInt16 = 3712;
        public static readonly byte DefByte = 255;
        public static readonly sbyte DefSByte = -128;
        public static readonly DateTime DefDateTime = new DateTime( 2018, 9, 5, 16, 6, 47, DateTimeKind.Local );
        public static readonly TimeSpan DefTimeSpan = new TimeSpan( 3, 2, 1, 59, 995 );
        public static readonly DateTimeOffset DefDateTimeOffset = new DateTimeOffset( DefDateTime, DateTimeOffset.Now.Offset );
        public static readonly double DefDouble = 35.9783e-78;
        public static readonly float DefSingle = (float)0.38974e-4;
        public static readonly char DefChar = 'c';
        public static readonly bool DefBoolean = true;

        [Test]
        public void basic_types_writing_and_reading()
        {
            using( var mem = new MemoryStream() )
            {
                var sShared = Guid.NewGuid().ToString();
                using( var w = new CKBinaryWriter( mem, Encoding.UTF8, true ) )
                {
                    w.WriteNullableString( DefString );
                    w.Write( DefInt32 );
                    w.Write( DefUInt32 );
                    w.Write( DefInt64 );
                    w.Write( DefUInt64 );
                    w.Write( DefInt16 );
                    w.Write( DefUInt16 );
                    w.Write( DefByte );
                    w.Write( DefSByte );
                    w.Write( DefDateTime );
                    w.Write( DefTimeSpan );

                    w.WriteSharedString( sShared );

                    w.Write( DefDateTimeOffset );
                    w.Write( DefGuid );
                    w.Write( DefDouble );
                    w.Write( DefSingle );
                    w.Write( DefChar );
                    w.Write( DefBoolean );

                    w.WriteSharedString( sShared );
                }
                mem.Position = 0;
                using( var r = new CKBinaryReader( mem, Encoding.UTF8, true ) )
                {
                    r.ReadNullableString().Should().Be( DefString );
                    r.ReadInt32().Should().Be( DefInt32 );
                    r.ReadUInt32().Should().Be( DefUInt32 );
                    r.ReadInt64().Should().Be( DefInt64 );
                    r.ReadUInt64().Should().Be( DefUInt64 );
                    r.ReadInt16().Should().Be( DefInt16 );
                    r.ReadUInt16().Should().Be( DefUInt16 );
                    r.ReadByte().Should().Be( DefByte );
                    r.ReadSByte().Should().Be( DefSByte );
                    r.ReadDateTime().Should().Be( DefDateTime );
                    r.ReadTimeSpan().Should().Be( DefTimeSpan );

                    r.ReadSharedString().Should().Be( sShared );

                    r.ReadDateTimeOffset().Should().Be( DefDateTimeOffset );
                    r.ReadGuid().Should().Be( DefGuid );
                    r.ReadDouble().Should().Be( DefDouble );
                    r.ReadSingle().Should().Be( DefSingle );
                    r.ReadChar().Should().Be( DefChar );
                    r.ReadBoolean().Should().Be( DefBoolean );
                    r.ReadSharedString().Should().Be( sShared );
                }
            }
        }

        [Test]
        public void object_pool_work()
        {
            using( var mem = new MemoryStream() )
            {
                var sA = new String( 'A', 100 );
                var sB = new String( 'B', 100 );
                using( var w = new CKBinaryWriter( mem, Encoding.UTF8, true ) )
                {
                    var pool = new CKBinaryWriter.ObjectPool<string>( w, StringComparer.InvariantCultureIgnoreCase );

                    var p = mem.Position;
                    p.Should().Be( 0 );

                    pool.MustWrite( sA ).Should().BeTrue();
                    w.Write( sA );
                    pool.MustWrite( sB ).Should().BeTrue();
                    w.Write( sB );

                    var delta = mem.Position - p;
                    p = mem.Position;
                    delta.Should().Be( 1 + 1 + sA.Length + 1 + 1 + sB.Length, "Marker byte + small length + UTF8 ascii string" );

                    for( int i = 0; i < 50; ++i )
                    {
                        pool.MustWrite( sA ).Should().BeFalse();
                        pool.MustWrite( sB ).Should().BeFalse();
                        pool.MustWrite( sA.ToLowerInvariant() ).Should().BeFalse();
                        pool.MustWrite( sB.ToLowerInvariant() ).Should().BeFalse();
                    }
                    delta = mem.Position - p;
                    delta.Should().Be( 50 * 4 * (1 + 1), "Marker byte + NonNegativeSmallInt32 that is actuall one byte..." );
                }
                mem.Position = 0;
                using( var r = new CKBinaryReader( mem, Encoding.UTF8, true ) )
                {
                    var pool = new CKBinaryReader.ObjectPool<string>( r );
                    string rA = pool.TryRead( out rA ).SetReadResult( r.ReadString() );
                    rA.Should().Be( sA );
                    string rB = pool.Read( ( state, reader ) => reader.ReadString() );
                    rB.Should().Be( sB );
                    for( int i = 0; i < 50; ++i )
                    {
                        pool.TryRead( out var rA2 ).Success.Should().BeTrue();
                        rA2.Should().Be( rA );
                        pool.Read( ( state, reader ) => reader.ReadString() ).Should().Be( rB );
                        pool.Read( ( state, reader ) => reader.ReadString() ).Should().Be( rA );
                        pool.Read( ( state, reader ) => reader.ReadString() ).Should().Be( rB );
                    }
                }
            }
        }

        [Test]
        public void object_pool_with_write_marker()
        {
            using( var mem = new MemoryStream() )
            {
                // Same string but in two different instances: the PureObjectRefEqualityComparer
                // does its job.
                var o1 = new String( 'B', 100 );
                var o2 = new String( 'B', 100 );
                using( var w = new CKBinaryWriter( mem, Encoding.UTF8, true ) )
                {
                    var pool = new CKBinaryWriter.ObjectPool<string>( w, PureObjectRefEqualityComparer<string>.Default );
                    pool.MustWrite( o1, 3 ).Should().BeTrue();
                    w.Write( o1 );
                    pool.MustWrite( o2, 255 ).Should().BeTrue();
                    w.Write( o2 );
                }
                mem.Position = 0;
                using( var r = new CKBinaryReader( mem, Encoding.UTF8, true ) )
                {
                    var pool = new CKBinaryReader.ObjectPool<string>( r );
                    var state1 = pool.TryRead( out var s1 );
                    s1.Should().BeNull();
                    state1.Success.Should().BeFalse();
                    state1.WriteMarker.Should().Be( 3 );
                    s1 = state1.SetReadResult( r.ReadString() );

                    var state2 = pool.TryRead( out var s2 );
                    s2.Should().BeNull();
                    state2.Success.Should().BeFalse();
                    state2.WriteMarker.Should().Be( 255 );
                    s2 = state2.SetReadResult( r.ReadString() );

                    s1.Should().Be( o1 ).And.Be( o2 );
                    s2.Should().Be( o1 ).And.Be( o2 );
                }
            }
        }
    }
}
