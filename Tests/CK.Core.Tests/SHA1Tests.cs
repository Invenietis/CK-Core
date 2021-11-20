using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class SHA1Tests
    {
        static readonly string ThisFile = Path.Combine( TestHelper.SolutionFolder, "Tests", "CK.Core.Tests", "SHA1Tests.cs" );

        [Test]
        public void SHA1_ToString_and_Parse()
        {
            var sha = SHA1Value.ComputeFileHash( ThisFile );
            var s = sha.ToString();
            var shaBis = SHA1Value.Parse( s );
            shaBis.Should().Be( sha );
        }

        [Test]
        public void SHA1_ByteAmount()
        {
            var sha = SHA1Value.ComputeFileHash( ThisFile );
            sha.GetBytes().Length.Should().Be( 20 );
            sha.ToString().Length.Should().Be( 40 );
        }

        [Test]
        public void SHA1Empty_IsValid()
        {
            byte[] computedValue = SHA1.HashData( ReadOnlySpan<byte>.Empty );
            var storedValue = SHA1Value.Empty.GetBytes();
            storedValue.Span.SequenceEqual( computedValue );
        }

        [TestCase( "0000000000000000000000000000000000000000", "0000000000000000000000000000000000000000", '=' )]
        [TestCase( "0000000000000000000000000000000000000000", "0123456789012345678901234567890123456789", '<' )]
        [TestCase( "0123456789012345678901234567890123456789", "0000000000000000000000000000000000000000", '>' )]
        [TestCase( "0123456789012345678901234567890123456789", "0123456789012345678901234567890123456788", '>' )]
        [TestCase( "0123456789012345678901234567890123456789", "0123459789012345678901234567890123456788", '<' )]
        [TestCase( "0123459789012345678901234567890123456788", "0123459789012345678901234567890123456788", '=' )]
        public void SHA1_CompareTo( string v1, string v2, char cmp )
        {
            var s1 = SHA1Value.Parse( v1 );
            var s2 = SHA1Value.Parse( v2 );
            switch( cmp )
            {
                case '>': s1.CompareTo( s2 ).Should().BeGreaterThan( 0 ); break;
                case '<': s1.CompareTo( s2 ).Should().BeLessThan( 0 ); break;
                default:  s1.CompareTo( s2 ).Should().Be( 0 ); break;
            }
        }

        [TestCase( null, null )]
        [TestCase( "", null )]
        [TestCase( "012345678901234567890123456789012345678", null )]
        [TestCase( "0123456789012345678901234567890123456789", 0 )]
        [TestCase( "f730a999523afe0a2be07bf4c731d3d1f72fb3df-----", 5 )]
        public void SHA1_invalid_parse( string s, int? remainderOnSuccess )
        {
            SHA1Value v;
            var r = SHA1Value.TryParse( s.AsSpan(), out _ );
            r.Success.Should().Be( remainderOnSuccess != null );
            if( remainderOnSuccess != null )
            {
                r.Remainder.Length.Should().Be( remainderOnSuccess.Value );
            }
        }


        [Test]
        public async Task SHA1_from_file_async()
        {
            var sha = SHA1Value.ComputeFileHash( ThisFile );
            var sha2 = await SHA1Value.ComputeFileHashAsync( ThisFile );
            sha2.Should().Be( sha );
            using( var compressedPath = new TemporaryFile() )
            {
                using( var input = new FileStream( ThisFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                using( var compressed = new FileStream( compressedPath.Path, FileMode.Truncate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                {
                    var writer = GetCompressShellAsync( w => input.CopyToAsync( w ) );
                    await writer( compressed );
                }
                var shaCompressed = await SHA1Value.ComputeFileHashAsync( compressedPath.Path );
                shaCompressed.Should().NotBe( sha );
                var localSha = await SHA1Value.ComputeFileHashAsync( compressedPath.Path, r => new GZipStream( r, CompressionMode.Decompress, true ) );
                localSha.Should().Be( sha );
            }
        }

        static Func<Stream, Task> GetCompressShellAsync( Func<Stream, Task> writer )
        {
            return async w =>
            {
                using( var compressor = new GZipStream( w, CompressionLevel.Optimal, true ) )
                {
                    await writer( compressor );
                    compressor.Flush();
                }
            };
        }
    }
}
