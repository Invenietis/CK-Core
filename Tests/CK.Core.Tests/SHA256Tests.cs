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
    public class SHA256Tests
    {
        static readonly string ThisFile = Path.Combine( TestHelper.SolutionFolder, "Tests", "CK.Core.Tests", "SHA256Tests.cs" );

        [Test]
        public void SHA256_ToString_and_Parse()
        {
            var sha = SHA256Value.ComputeFileSHA256( ThisFile );
            var s = sha.ToString();
            var shaBis = SHA256Value.Parse( s );
            shaBis.Should().Be( sha );
        }

        [Test]
        public void SHA256_ByteAmount()
        {
            var sha = SHA256Value.ComputeFileSHA256( ThisFile );
            sha.GetBytes().Count.Should().Be( 32 );
            sha.ToString().Length.Should().Be( 64 );
        }

        [Test]
        public void SHA256Empty_IsValid()
        {
            SHA256Managed sha256 = new SHA256Managed();
            byte[] computedValue = sha256.ComputeHash( new byte[0] );
            IReadOnlyList<byte> storedValue = SHA256Value.EmptySHA256.GetBytes();
            storedValue.SequenceEqual( computedValue );
        }

        [TestCase( "0000000000000000000000000000000000000000000000000000000000000000",
                   "0000000000000000000000000000000000000000000000000000000000000000",
                   '=' )]
        [TestCase( "0000000000000000000000000000000000000000000000000000000000000000",
                   "0123456789012345678901234567890123456789012345678901234567890123",
                   '<' )]
        [TestCase( "0123456789012345678901234567890123456789012345678901234567890123",
                   "0000000000000000000000000000000000000000000000000000000000000000",
                   '>' )]
        [TestCase( "0123456789012345678901234567890123456789012345678901234567890123",
                   "0123456789012345678901234567890123456789012345678901234567890122",
                   '>' )]
        [TestCase( "0123456789012345678901234567890123456789012345678901234567890123",
                   "0123456889012345678901234567890123456789012345678901234567890123",
                   '<' )]
        [TestCase( "0123456789012345678901234567890123456789012345678901234567890123",
                   "0123456789012345678901234567890123456789012345678901234567890123",
                   '=' )]
        public void SHA256_CompareTo( string v1, string v2, char cmp )
        {
            var s1 = SHA256Value.Parse( v1 );
            var s2 = SHA256Value.Parse( v2 );
            switch( cmp )
            {
                case '>': s1.CompareTo( s2 ).Should().BeGreaterThan( 0 ); break;
                case '<': s1.CompareTo( s2 ).Should().BeLessThan( 0 ); break;
                default: s1.CompareTo( s2 ).Should().Be( 0 ); break;
            }
        }

        [TestCase( 0, null, false )]
        [TestCase( 1, "", false )]
        [TestCase( 1, "X012345678901234567890123456789012345678901234567890123456789012", false )]
        [TestCase( 2, "XY0123456789012345678901234567890123456789012345678901234567890123", true )]
        [TestCase( 2, "--f730a999523afe0a2be07bf4c731d3d1f72fb3dff730a999523afe0a2be07bf4-----", true )]
        public void SHA256_invalid_parse( int offset, string s, bool success )
        {
            SHA256Value v;
            SHA256Value.TryParse( s, offset, out v ).Should().Be( success );
        }


        [Test]
        public async Task SHA256_from_file_async()
        {
            var sha = SHA256Value.ComputeFileSHA256( ThisFile );
            var sha2 = await SHA256Value.ComputeFileSHA256Async( ThisFile );
            sha2.Should().Be( sha );
            using( var compressedPath = new TemporaryFile() )
            {
                using( var input = new FileStream( ThisFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                using( var compressed = new FileStream( compressedPath.Path, FileMode.Truncate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                {
                    var writer = GetCompressShellAsync( w => input.CopyToAsync( w ) );
                    await writer( compressed );
                }
                var shaCompressed = await SHA256Value.ComputeFileSHA256Async( compressedPath.Path );
                shaCompressed.Should().NotBe( sha );
                var localSha = await SHA256Value.ComputeFileSHA256Async( compressedPath.Path, r => new GZipStream( r, CompressionMode.Decompress, true ) );
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
