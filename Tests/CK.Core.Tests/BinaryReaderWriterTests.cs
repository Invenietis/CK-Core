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
        public static readonly DateTimeOffset DefDateTimeOffset = new DateTimeOffset( DefDateTime, TimeZoneInfo.Local.GetUtcOffset( DefDateTime ) );
        public static readonly double DefDouble = 35.9783e-78;
        public static readonly float DefSingle = (float)0.38974e-4;
        public static readonly char DefChar = 'c';
        public static readonly bool DefBoolean = true;
        public static readonly Index DefIndex = new Index( 3712, true );
        public static readonly Range DefRange = 5..^7;

        [Test]
        public void basic_types_writing_and_reading()
        {
            using( var mem = Util.RecyclableStreamManager.GetStream() )
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
                    w.Write( DefIndex );
                    w.Write( DefRange );

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
                    r.ReadIndex().Should().Be( DefIndex );
                    r.ReadRange().Should().Be( DefRange );

                    r.ReadSharedString().Should().Be( sShared );
                }
            }
        }

        enum EByte : byte { V0 = 54, V1 = 87 };
        enum ESByte : sbyte { V0 = -54, V1 = -78 };
        enum EUInt16 : ushort { V0 = 540, V1 = 8087 };
        enum EInt16 : short { V0 = -540, V1 = -7218 };
        enum EUInt32 : uint { V0 = 50040, V1 = 800087 };
        enum EInt32 : int { V0 = -5456460, V1 = -721678 };
        enum EUInt64 : ulong { V0 = 5032465040, V1 = 83545700087 };
        enum EInt64 : long { V0 = -545648760, V1 = -7216555778 };

        // ICKBinaryReader.ReadEnum<T>() and ICKBinaryWriter.WriteEnum<T>( T e ) have been removed
        // since it was NOT a good idea at all: it has to use reflection, was ugly and didn't
        // handle change of the integral type without any warnings: reading a changed type
        // would simply fail at runtime.
        // Casting to/from the integral type is simpler, faster and safer.
        [Test]
        public void writing_and_reading_enums_and_nullable_enums()
        {
            EByte? vNU8 = EByte.V1;
            ESByte? vNI8 = ESByte.V1;
            EInt16? vNI16 = EInt16.V1;
            EUInt16? vNU16 = EUInt16.V1;
            EInt32? vNI32 = EInt32.V1;
            EUInt32? vNUI32 = EUInt32.V1;
            EInt64? vNI64 = EInt64.V1;
            EUInt64? vNUI64 = EUInt64.V1;
            EByte vU8 = EByte.V1;
            ESByte vI8 = ESByte.V1;
            EInt16 vI16 = EInt16.V1;
            EUInt16 vU16 = EUInt16.V1;
            EInt32 vI32 = EInt32.V1;
            EUInt32 vUI32 = EUInt32.V1;
            EInt64 vI64 = EInt64.V1;
            EUInt64 vUI64 = EUInt64.V1;

            ReadWrite( w =>
            {
                w.WriteNullableUInt32( (uint?)vNI32 );
                w.WriteNullableByte( (byte?)vNU8 );
                w.WriteNullableSByte( (sbyte?)vNI8 );
                w.WriteNullableInt16( (short?)vNI16 );
                w.WriteNullableUInt16( (ushort?)vNU16 );
                w.WriteNullableUInt32( (uint?)vNUI32 );
                w.WriteNullableInt64( (long?)vNI64 );
                w.WriteNullableUInt64( (ulong?)vNUI64 );
                w.Write( (byte)vU8 );
                w.Write( (sbyte)vI8 );
                w.Write( (short)vI16 );
                w.Write( (ushort)vU16 );
                w.Write( (int)vI32 );
                w.Write( (uint)vUI32 );
                w.Write( (long)vI64 );
                w.Write( (ulong)vUI64 );
            },
            r =>
            {
                ((EInt32?)r.ReadNullableInt32()).Should().Be( vNI32 );
                ((EByte?)r.ReadNullableByte()).Should().Be( vNU8 );
                ((ESByte?)r.ReadNullableSByte()).Should().Be( vNI8 );
                ((EInt16?)r.ReadNullableInt16()).Should().Be( vNI16 );
                ((EUInt16?)r.ReadNullableUInt16()).Should().Be( vNU16 );
                ((EUInt32?)r.ReadNullableUInt32()).Should().Be( vNUI32 );
                ((EInt64?)r.ReadNullableInt64()).Should().Be( vNI64 );
                ((EUInt64?)r.ReadNullableUInt64()).Should().Be( vNUI64 );
                ((EByte)r.ReadByte()).Should().Be( vU8 );
                ((ESByte)r.ReadSByte()).Should().Be( vI8 );
                ((EInt16)r.ReadInt16()).Should().Be( vI16 );
                ((EUInt16)r.ReadUInt16()).Should().Be( vU16 );
                ((EInt32)r.ReadInt32()).Should().Be( vI32 );
                ((EUInt32)r.ReadUInt32()).Should().Be( vUI32 );
                ((EInt64)r.ReadInt64()).Should().Be( vI64 );
                ((EUInt64)r.ReadUInt64()).Should().Be( vUI64 );
            } );
        }

        [Test]
        public void writing_and_reading_nullable_types()
        {
            ReadWrite( w =>
            {
                w.WriteNullableBool( null );
                w.WriteNullableByte( null );
                w.WriteNullableSByte( null );
                w.WriteNullableDateTime( null );
                w.WriteNullableTimeSpan( null );
                w.WriteNullableDateTimeOffset( null );
                w.WriteNullableGuid( null );
                w.WriteNullableIndex( null );
                w.WriteNullableRange( null );

                w.WriteNullableDateTime( DefDateTime );
                w.WriteNullableTimeSpan( DefTimeSpan );
                w.WriteNullableDateTimeOffset( DefDateTimeOffset );
                w.WriteNullableGuid( DefGuid );
                w.WriteNullableBool( true );
                w.WriteNullableBool( false );
                w.WriteNullableIndex( DefIndex );
                w.WriteNullableRange( DefRange );

                for( int i = Byte.MinValue; i <= Byte.MaxValue; ++i ) w.WriteNullableByte( (byte)i );
                for( int i = SByte.MinValue; i <= SByte.MaxValue; ++i ) w.WriteNullableSByte( (sbyte)i );

                for( int i = UInt16.MinValue; i <= UInt16.MaxValue; ++i ) w.WriteNullableUInt16( (ushort)i );
                for( int i = Int16.MinValue; i <= Int16.MaxValue; ++i ) w.WriteNullableInt16( (short)i );
            },
            r =>
            {
                r.ReadNullableBool().Should().Be( null );
                r.ReadNullableByte().Should().Be( null );
                r.ReadNullableSByte().Should().Be( null );
                r.ReadNullableDateTime().Should().Be( null );
                r.ReadNullableTimeSpan().Should().Be( null );
                r.ReadNullableDateTimeOffset().Should().Be( null );
                r.ReadNullableGuid().Should().Be( null );
                r.ReadNullableIndex().Should().Be( null );
                r.ReadNullableRange().Should().Be( null );

                r.ReadNullableDateTime().Should().Be( DefDateTime );
                r.ReadNullableTimeSpan().Should().Be( DefTimeSpan );
                r.ReadNullableDateTimeOffset().Should().Be( DefDateTimeOffset );
                r.ReadNullableGuid().Should().Be( DefGuid );
                r.ReadNullableBool().Should().Be( true );
                r.ReadNullableBool().Should().Be( false );
                r.ReadNullableIndex().Should().Be( DefIndex );
                r.ReadNullableRange().Should().Be( DefRange );

                for( int i = Byte.MinValue; i <= Byte.MaxValue; ++i ) r.ReadNullableByte().Should().Be( (byte)i );
                for( int i = SByte.MinValue; i <= SByte.MaxValue; ++i ) r.ReadNullableSByte().Should().Be( (sbyte)i );

                for( int i = UInt16.MinValue; i <= UInt16.MaxValue; ++i ) r.ReadNullableUInt16().Should().Be( (ushort)i );
                for( int i = Int16.MinValue; i <= Int16.MaxValue; ++i ) r.ReadNullableInt16().Should().Be( (short)i );
            } );
        }

        [Test]
        public void nullable_types_size_check()
        {
            ReadWrite( w =>
            {
                w.WriteNullableBool( null );
                w.WriteNullableBool( true );
                w.WriteNullableBool( false );
            } ).Should()
            .Be( 3 );
        }

        [Test]
        public void nullable_types_size_check_Byte()
        {
            ReadWrite( w =>
            {
                w.WriteNullableByte( 0xFE );
                w.WriteNullableByte( 0xFF );
            }, r =>
            {
                r.ReadNullableByte().Should().Be( 0xFE );
                r.ReadNullableByte().Should().Be( 0xFF );

            } ).Should()
            .Be( 4, "2 bytes for 254 and 255." );

            ReadWrite( w =>
            {
                w.WriteNullableByte( null );
                for( int i = 0; i < 0xFE; ++i ) w.WriteNullableByte( (byte)i );
            } ).Should()
            .Be( 1 + 0xFE, "One byte for all 'small' values including null." );

        }

        [Test]
        public void nullable_types_size_check_SByte()
        {
            ReadWrite( w =>
            {
                w.WriteNullableSByte( -128 );
                w.WriteNullableSByte( 127 );
            }, r =>
            {
                r.ReadNullableSByte().Should().Be( -128 );
                r.ReadNullableSByte().Should().Be( 127 );
            }
            ).Should()
            .Be( 4, "2 bytes for -128 and 127." );

            ReadWrite( w =>
            {
                w.WriteNullableSByte( null );
                for( int i = -127; i < 127; ++i ) w.WriteNullableSByte( (sbyte)i );
            } ).Should()
            .Be( 1 + 127 + 127, "One byte for all 'small' values including null." );
        }

        [Test]
        public void nullable_types_size_check_UInt16()
        {
            ReadWrite( w =>
            {
                w.WriteNullableUInt16( UInt16.MaxValue - 1 );
                w.WriteNullableUInt16( UInt16.MaxValue );
            }, r =>
            {
                r.ReadNullableUInt16().Should().Be( UInt16.MaxValue - 1 );
                r.ReadNullableUInt16().Should().Be( UInt16.MaxValue );
            } ).Should()
            .Be( 2 * 3 );

            ReadWrite( w =>
            {
                w.WriteNullableUInt16( null );
                // We write UInt16.MaxValue values (zero based).
                for( int i = 0; i < UInt16.MaxValue - 1; ++i ) w.WriteNullableUInt16( (ushort)i );
            } ).Should()
            .Be( 2 + 2 * UInt16.MaxValue );
        }

        [Test]
        public void nullable_types_size_check_Int16()
        {
            ReadWrite( w =>
            {
                w.WriteNullableInt16( Int16.MinValue );
                w.WriteNullableInt16( Int16.MaxValue );
            }, r =>
            {
                r.ReadNullableInt16().Should().Be( Int16.MinValue );
                r.ReadNullableInt16().Should().Be( Int16.MaxValue );
            } ).Should()
            .Be( 2 * 3 );

            ReadWrite( w =>
            {
                w.WriteNullableInt16( null );
                // We write 2*Int16.MaxValue values.
                for( int i = Int16.MinValue + 1; i < Int16.MaxValue; ++i ) w.WriteNullableInt16( (short)i );
            } ).Should()
            .Be( 2 + 2 * (Int16.MaxValue + Int16.MaxValue) );

        }

        [Test]
        public void nullable_types_size_check_UInt32()
        {
            ReadWrite( w =>
            {
                w.WriteNullableUInt32( UInt32.MaxValue - 1 );
                w.WriteNullableUInt32( UInt32.MaxValue );
            }, r =>
            {
                r.ReadNullableUInt32().Should().Be( UInt32.MaxValue - 1 );
                r.ReadNullableUInt32().Should().Be( UInt32.MaxValue );
            } ).Should()
            .Be( 2 * 5 );

            ReadWrite( w =>
            {
                w.WriteNullableUInt32( null );
                // We write 200 values.
                for( int i = 0; i < 200; ++i ) w.WriteNullableUInt32( (uint)i );
            } ).Should()
            .Be( 4 + 4 * 200 );
        }

        [Test]
        public void nullable_types_size_check_Int32()
        {
            ReadWrite( w =>
            {
                w.WriteNullableInt32( Int32.MinValue );
                w.WriteNullableInt32( Int32.MaxValue );
            }, r =>
            {
                r.ReadNullableInt32().Should().Be( Int32.MinValue );
                r.ReadNullableInt32().Should().Be( Int32.MaxValue );
            } ).Should()
            .Be( 2 * 5 );

            ReadWrite( w =>
            {
                w.WriteNullableInt32( null );
                // We write 200 values.
                for( int i = -100; i < 100; ++i ) w.WriteNullableInt32( i );
            } ).Should()
            .Be( 4 + 4 * 200 );
        }


        [Test]
        public void nullable_types_size_check_UInt64()
        {
            ReadWrite( w =>
            {
                w.WriteNullableUInt64( UInt64.MaxValue - 1 );
                w.WriteNullableUInt64( UInt64.MaxValue );
            }, r =>
            {
                r.ReadNullableUInt64().Should().Be( UInt64.MaxValue - 1 );
                r.ReadNullableUInt64().Should().Be( UInt64.MaxValue );
            } ).Should()
            .Be( 2 * 9 );

            ReadWrite( w =>
            {
                w.WriteNullableUInt64( null );
                // We write 200 values.
                for( int i = 0; i < 200; ++i ) w.WriteNullableUInt64( (uint)i );
            } ).Should()
            .Be( 8 + 8 * 200 );
        }

        [Test]
        public void nullable_types_size_check_Int64()
        {
            ReadWrite( w =>
            {
                w.WriteNullableInt64( Int64.MinValue );
                w.WriteNullableInt64( Int64.MaxValue );
            }, r =>
            {
                r.ReadNullableInt64().Should().Be( Int64.MinValue );
                r.ReadNullableInt64().Should().Be( Int64.MaxValue );
            } ).Should()
            .Be( 2 * 9 );

            ReadWrite( w =>
            {
                w.WriteNullableInt64( null );
                // We write 200 values.
                for( int i = -100; i < 100; ++i ) w.WriteNullableInt64( i );
            } ).Should()
            .Be( 8 + 8 * 200 );
        }

        [Test]
        public void nullable_types_size_check_Char()
        {
            ReadWrite( w =>
                {
                    w.WriteNullableChar( (char)(Char.MinValue + 1) );
                    w.WriteNullableChar( Char.MinValue );
                }, r =>
                {
                    r.ReadNullableChar().Should().Be( (char)(Char.MinValue + 1) );
                    r.ReadNullableChar().Should().Be( Char.MinValue );
                } ).Should()
                .Be( (2 * 2), "Two single-byte chars, plus their respective 0x00 or 0x01 discriminator byte" );

            ReadWrite( w =>
                {
                    w.WriteNullableChar( 'の' ); // 'HIRAGANA LETTER NO' (U+306E - UTF-8: 0xE3 0x81 0xAE)
                }, r =>
                {
                    r.ReadNullableChar().Should().Be( 'の' );
                } ).Should()
                .Be( 3, "One 3-byte char" );

            int totalCharSize = 0;
            ReadWrite( w =>
                {
                    w.WriteNullableChar( null );
                    // We write Char.MaxValue values (zero based).
                    for( int i = 0x00; i < Char.MaxValue - 1; ++i )
                    {
                        char c = Convert.ToChar( i );
                        if( char.IsSurrogate( c ) ) continue;
                        totalCharSize += Encoding.UTF8.GetByteCount( new[] { c } );
                        w.WriteNullableChar( c );
                    }
                } ).Should()
                .Be( 2 + totalCharSize + 1, "Null: 0xFF00, plus the written size of all characters, plus the single 0x01 added to the very first char" );
        }

        [Test]
        public void object_pool_work()
        {
            using( var mem = Util.RecyclableStreamManager.GetStream() )
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
                    string? rA = pool.TryRead( out rA ).SetReadResult( r.ReadString() );
                    rA.Should().Be( sA );
                    string? rB = pool.Read( ( state, reader ) => reader.ReadString() );
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
            using( var mem = Util.RecyclableStreamManager.GetStream() )
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

        static int ReadWrite( Action<ICKBinaryWriter> writer, Action<ICKBinaryReader>? reader = null )
        {
            using( var mem = Util.RecyclableStreamManager.GetStream() )
            {
                using( var w = new CKBinaryWriter( mem, Encoding.UTF8, true ) )
                {
                    writer( w );
                }
                int pos = (int)mem.Position;
                if( reader != null )
                {
                    mem.Position = 0;
                    using( var r = new CKBinaryReader( mem, Encoding.UTF8, true ) )
                    {
                        reader( r );
                    }
                    mem.Position.Should().Be( pos, $"Written {pos} bytes should be the same as read bytes count but found {mem.Position} bytes." );
                }
                return pos;
            }
        }
    }
}
