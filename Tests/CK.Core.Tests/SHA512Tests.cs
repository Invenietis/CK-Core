using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CK.Core.Tests;

[TestFixture]
public class SHA512Tests
{
    static readonly string ThisFile = Path.Combine( TestHelper.SolutionFolder, "Tests", "CK.Core.Tests", "SHA512Tests.cs" );

    [Test]
    public void SHA512_ToString_and_Parse()
    {
        var sha = SHA512Value.ComputeFileHash( ThisFile );
        var s = sha.ToString();
        var shaBis = SHA512Value.Parse( s );
        shaBis.Should().Be( sha );
    }

    [Test]
    public void SHA512_ByteAmount()
    {
        var sha = SHA512Value.ComputeFileHash( ThisFile );
        sha.GetBytes().Length.Should().Be( 64 );
        sha.ToString().Length.Should().Be( 128 );
    }

    [Test]
    public void SHA512_CreateRandom()
    {
        var sha = SHA512Value.CreateRandom();
        // Ok... This MAY fail :).
        sha.Should().NotBe( SHA512Value.Zero );
        sha.Should().NotBe( SHA512Value.Empty );
        sha.GetBytes().Length.Should().Be( 64 );
        sha.ToString().Length.Should().Be( 128 );
    }

    [Test]
    public void SHA512Empty_IsValid()
    {
        byte[] computedValue = SHA512.HashData( ReadOnlySpan<byte>.Empty );
        var storedValue = SHA512Value.Empty.GetBytes();
        storedValue.Span.SequenceEqual( computedValue );
    }

    [TestCase( "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
               "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
               '=' )]
    [TestCase( "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
               "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               '<' )]
    [TestCase( "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
               '>' )]
    [TestCase( "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234566",
               '>' )]
    [TestCase( "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               "01234568890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               '<' )]
    [TestCase( "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567",
               '=' )]
    public void SHA512_CompareTo( string v1, string v2, char cmp )
    {
        var s1 = SHA512Value.Parse( v1 );
        var s2 = SHA512Value.Parse( v2 );
        switch( cmp )
        {
            case '>': s1.CompareTo( s2 ).Should().BeGreaterThan( 0 ); break;
            case '<': s1.CompareTo( s2 ).Should().BeLessThan( 0 ); break;
            default: s1.CompareTo( s2 ).Should().Be( 0 ); break;
        }
    }

    [TestCase( null, false )]
    [TestCase( "", false )]
    [TestCase( "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456", false )]
    [TestCase( "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567", true )]
    [TestCase( "f730a999523afe0a2be07bf4c731d3d1f72fb3dff730a999523afe0a2be07bf4c731d3d1f72fb3dff730a999523afe0a2be07bf4c731d3d1f72fb3df01234567-----", false )]
    public void SHA512_invalid_parse( string s, bool success )
    {
        SHA512Value.TryParse( s.AsSpan(), out _ ).Should().Be( success );
    }


    [Test]
    public async Task SHA512_from_file_Async()
    {
#pragma warning disable VSTHRD103 // Call async methods when in an async method
        var sha = SHA512Value.ComputeFileHash( ThisFile );
#pragma warning restore VSTHRD103 // Call async methods when in an async method
        var sha2 = await SHA512Value.ComputeFileHashAsync( ThisFile );
        sha2.Should().Be( sha );
        using( var compressedPath = new TemporaryFile() )
        {
            using( var input = new FileStream( ThisFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            using( var compressed = new FileStream( compressedPath.Path, FileMode.Truncate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous ) )
            {
                var writer = GetCompressShell( w => input.CopyToAsync( w ) );
                await writer( compressed );
            }
            var shaCompressed = await SHA512Value.ComputeFileHashAsync( compressedPath.Path );
            shaCompressed.Should().NotBe( sha );
            var localSha = await SHA512Value.ComputeFileHashAsync( compressedPath.Path, r => new GZipStream( r, CompressionMode.Decompress, true ) );
            localSha.Should().Be( sha );
        }
    }

    static Func<Stream, Task> GetCompressShell( Func<Stream, Task> writer )
    {
        return async w =>
        {
            using( var compressor = new GZipStream( w, CompressionLevel.Optimal, true ) )
            {
                await writer( compressor );
                await compressor.FlushAsync();
            }
        };
    }
}
