using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace CK.Core.Tests
{
    [TestFixture]
    public class SHA1Tests
    {
        static readonly string ThisFile = Path.Combine( TestHelper.SolutionFolder, "Tests", "CK.Core.Tests", "SHA1Tests.cs" );

        [Test]
        public void SHA1_ToString_and_Parse()
        {
            var sha = SHA1Value.ComputeFileSHA1( ThisFile );
            var s = sha.ToString();
            var shaBis = SHA1Value.Parse( s );
            shaBis.Should().Be( sha );
        }

        [Test]
        public void SHA1_ByteAmount()
        {
            var sha = SHA1Value.ComputeFileSHA1( ThisFile );
            sha.GetBytes().Count.Should().Be( 20 );
            sha.ToString().Length.Should().Be( 40 );
        }

        [Test]
        public void SHA1Empty_IsValid()
        {
            SHA1Managed sha1 = new SHA1Managed();
            byte[] computedValue = sha1.ComputeHash( new byte[0] );
            IReadOnlyList<byte> storedValue = SHA1Value.EmptySHA1.GetBytes();
            storedValue.SequenceEqual( computedValue );
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

        [TestCase( 0, null, false )]
        [TestCase( 1, "", false )]
        [TestCase( 1, "X012345678901234567890123456789012345678", false )]
        [TestCase( 2, "XY0123456789012345678901234567890123456789", true )]
        [TestCase( 2, "--f730a999523afe0a2be07bf4c731d3d1f72fb3df-----", true )]
       public void SHA1_invalid_parse( int offset, string s, bool success )
        {
            SHA1Value v;
            SHA1Value.TryParse( s, offset, out v ).Should().Be( success );
        }


        [Test]
        public async Task SHA1_from_file_async()
        {
            var sha = SHA1Value.ComputeFileSHA1( ThisFile );
            var sha2 = await SHA1Value.ComputeFileSHA1Async( ThisFile );
            sha2.Should().Be( sha );
            using( var compressedPath = new TemporaryFile() )
            {
                using( var input = new FileStream( ThisFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                using( var compressed = new FileStream( compressedPath.Path, FileMode.Truncate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
                {
                    var writer = GetCompressShellAsync( w => input.CopyToAsync( w ) );
                    await writer( compressed );
                }
                var shaCompressed = await SHA1Value.ComputeFileSHA1Async( compressedPath.Path );
                shaCompressed.Should().NotBe( sha );
                var localSha = await SHA1Value.ComputeFileSHA1Async( compressedPath.Path, r => new GZipStream( r, CompressionMode.Decompress, true ) );
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
