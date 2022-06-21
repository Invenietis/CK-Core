using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CK.Core.Tests
{
    [TestFixture]
    public class FastUniqueIdGeneratorTests
    {
        [Test]
        public void unique_string_id_are_11_characters_long()
        {
            FastUniqueIdGenerator generator = new FastUniqueIdGenerator();
            generator.GetNextString().Length.Should().Be( 11 );
        }

        [Test]
        public void unique_string_id_are_not_increasing_because_base64_does_not_guaranty_ordering_but_share_the_same_prefix()
        {
            FastUniqueIdGenerator generator = new FastUniqueIdGenerator();
            var ids = Enumerable.Range( 0, 1000 ).Select( _ => generator.GetNextString() ).ToList();
            ids.Should().NotBeInAscendingOrder();
            // We cannot test the prefix for all consecutive pairs since the initial one is random but we can assert that on any sample
            // of 999 pairs there must be at most 63 with 2 (or more) different last characters.
            var startsWithPrevious = ids.Skip( 1 )
                                        .Select( ( u, idx ) => (idx, ids[idx], u, StartsWithPrevious: ids[idx].AsSpan().StartsWith( u.AsSpan( 0, u.Length - 1 ) )) ).ToList();
            var lastTwoDiffer = startsWithPrevious.Count( t => !t.StartsWithPrevious );
            lastTwoDiffer.Should().BeLessThan( 64 );
        }

        [Test]
        public void GetNextString_and_FillNextUtf8String_gives_the_same_result()
        {
            long initialValue = 0;
            var bytes = MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref initialValue, 1 ) );
            System.Security.Cryptography.RandomNumberGenerator.Fill( bytes );

            FastUniqueIdGenerator gS = new FastUniqueIdGenerator( initialValue );
            FastUniqueIdGenerator gU = new FastUniqueIdGenerator( initialValue );

            var idS = Enumerable.Range( 0, 1000 ).Select( _ => gS.GetNextString() ).ToList();
            var idU = Enumerable.Range( 0, 1000 ).Select( _ => GetByUtf8Buffer( gU ) ).ToList();

            static string GetByUtf8Buffer( FastUniqueIdGenerator g )
            {
                Span<byte> buffer = stackalloc byte[11];
                g.FillNextUtf8String( buffer );
                return System.Text.Encoding.UTF8.GetString( buffer );
            }

            idS.Should().BeEquivalentTo( idU );
        }
    }
}
