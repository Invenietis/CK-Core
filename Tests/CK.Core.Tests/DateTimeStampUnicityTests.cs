using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class DateTimeStampUnicityTests
    {

        [Test]
        public void generating_time_collisions()
        {
            DateTimeStamp[] all = new DateTimeStamp[8192];
            DateTimeStamp current = DateTimeStamp.UtcNow;
            for( int i = 0; i < all.Length; ++i )
            {
                DateTimeStamp next = new DateTimeStamp( current, DateTime.UtcNow );
                all[i] = current = next;
            }
            TimeSpan delta = all[all.Length - 1].TimeUtc - all[0].TimeUtc;
            int collisionCount = all.Count( d => d.Uniquifier != 0 );
            Console.WriteLine( $"Delta = {delta}, Collisions = {collisionCount} out of {all.Length}." );
        }

        [Test]
        public void DateTimeStampProvider_tests()
        {
            // The only way to show that this test proves anything is
            // to remove the lock( _lock ) in DateTimeStampProvider.GetNetxNow():
            // duplicate stamps appear.
            var provider = new DateTimeStampProvider();

            const int count = 5000;
            const int nbThread = 8;
            var all = new DateTimeStamp[count * nbThread];
            var threads = Enumerable.Range(0,nbThread).Select( iT => new Thread( () =>
            {
                for( int i = 0; i < count; ++i )
                {
                    all[iT * count + i] = provider.GetNextNow();
                }

            } ) ).ToArray();
            foreach( var t in threads ) t.Start();
            foreach( var t in threads ) t.Join();

            // Each set is ever increasing.
            for( int iT = 0; iT < nbThread; ++iT )
            {
                all.Skip( iT * count ).Take( count ).IsSortedStrict().Should().BeTrue();
            }
            // All stamps are different.
            for( int iT = 0; iT < nbThread; ++iT )
            {
                new HashSet<DateTimeStamp>( all ).Count.Should().Be( all.Length );
            }
            // But stamps across all the threads are NOT ever increasing: this
            // ensures that threads have yield.
            all.IsSortedStrict().Should().BeFalse();
        }


        [Test]
        public void generating_forced_time_collisions()
        {
            DateTimeStamp fake = DateTimeStamp.UtcNow;

            Stopwatch w = new Stopwatch();
            w.Start();

            DateTimeStamp[] all = new DateTimeStamp[8192];
            DateTimeStamp current = fake;
            for( int i = 0; i < all.Length; ++i )
            {
                DateTimeStamp next = new DateTimeStamp( current, fake );
                all[i] = current = next;
            }
            TimeSpan delta = all[all.Length - 1].TimeUtc - all[0].TimeUtc;
            int collisionCount = all.Count( d => d.Uniquifier != 0 );
            Console.WriteLine( $"Delta = {delta}, Collisions = {collisionCount}." );

            w.Stop();
            Console.WriteLine( $"Ticks = {w.ElapsedTicks}." );
        }

        [Test]
        public void DateTimeStamp_ToString_and_TryFormat()
        {
            DateTimeStamp d1 = DateTimeStamp.UtcNow;
            d1.Uniquifier.Should().Be( 0 );
            var b = new char[32];

            d1.TryFormat( b.AsSpan(), out var cb, ReadOnlySpan<char>.Empty, null );
            cb.Should().Be( 27 );
            d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
            d1.TryFormat( b.AsSpan(0,26), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
            cb.Should().Be( 0 );

            d1 = new DateTimeStamp( d1.TimeUtc, 5 );
            d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
            cb.Should().Be( 30 );
            d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
            d1.TryFormat( b.AsSpan( 0, 29 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
            cb.Should().Be( 0 );

            d1 = new DateTimeStamp( d1.TimeUtc, 99 );
            d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
            cb.Should().Be( 31 );
            d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
            d1.TryFormat( b.AsSpan( 0, 30 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
            cb.Should().Be( 0 );

            d1 = new DateTimeStamp( d1.TimeUtc, 255 );
            d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
            cb.Should().Be( 32 );
            d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
            d1.TryFormat( b.AsSpan( 0, 31 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
            cb.Should().Be( 0 );
        }
    }
}
