using FluentAssertions;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Linq;

namespace CK.Core.Tests
{
    [TestFixture]
    public class CheckedWriteStreamTests
    {
        [Test]
        public void empty_streams_are_equals()
        {
            var checker = CheckedWriteStream.Create( ReadOnlySequence<byte>.Empty );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.None );
        }

        [TestCase( 0 )]
        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 45 )]
        [TestCase( 255 )]
        public void longer_than_reference_bytes( int initialLength )
        {
            var content = Enumerable.Range( 0, initialLength ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            checker.Write( content );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.None );
            if( initialLength > 0 )
            {
                checker.Write( content );
                checker.GetResult().Should().Be( CheckedWriteStream.Result.LongerThanRefBytes );
                checker.Position.Should().Be( initialLength );
            }
        }

        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 45 )]
        [TestCase( 255 )]
        public void shorter_than_reference_bytes( int initialLength )
        {
            var content = Enumerable.Range( 0, initialLength ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            checker.Write( content, 0, content.Length - 1 );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.ShorterThanRefBytes );
            checker.Write( content, content.Length - 1, 1 );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.None );
        }

        [TestCase( 5 )]
        [TestCase( 255 )]
        [TestCase( 3712 )]
        public void byte_differs( int initialLength )
        {
            var content = Enumerable.Range( 0, initialLength ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            int idx = Random.Shared.Next( initialLength );
            var modified = content.ToArray();
            modified[idx] = (byte)(idx + 1);
            checker.Write( modified );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.HasByteDifference );
            checker.Position.Should().Be( idx );
        }

        [Test]
        public void ThrowArgumentException_on_longer_than_reference_bytes()
        {
            var content = Enumerable.Range( 0, 100 ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            checker.ThrowArgumentException = true;
            checker.Write( content );
            checker.GetResult().Should().Be( CheckedWriteStream.Result.None );
            FluentActions.Invoking( () => checker.WriteByte( 0 ) ).Should().Throw<ArgumentException>();
        }

        [Test]
        public void ThrowArgumentException_on_shorter_than_reference_bytes()
        {
            var content = Enumerable.Range( 0, 100 ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            checker.ThrowArgumentException = true;
            checker.Write( content, 0, content.Length - 1 );
            FluentActions.Invoking( () => checker.GetResult() ).Should().Throw<ArgumentException>();
        }

        [TestCase( 5 )]
        [TestCase( 255 )]
        [TestCase( 3712 )]
        public void ThrowArgumentException_on_byte_differs( int initialLength )
        {
            var content = Enumerable.Range( 0, initialLength ).Select( i => (byte)i ).ToArray();
            var checker = CheckedWriteStream.Create( new ReadOnlySequence<byte>( content ) );
            checker.ThrowArgumentException = true;
            int idx = Random.Shared.Next( initialLength );
            var modified = content.ToArray();
            modified[idx] = (byte)(idx + 1);
            FluentActions.Invoking( () => checker.Write( modified ) ).Should().Throw<ArgumentException>();
        }


    }
}
