using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;

namespace CK.Core.Tests;

[TestFixture]
public class DateTimeStampTests
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
    public void DateTimeStamp_ToString_and_TryFormat_and_Parse()
    {
        DateTimeStamp d1 = DateTimeStamp.UtcNow;
        d1.Uniquifier.Should().Be( 0 );
        var b = new char[32];

        d1.TryFormat( b.AsSpan(), out var cb, ReadOnlySpan<char>.Empty, null );
        cb.Should().Be( 27 );
        d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
        d1.TryFormat( b.AsSpan( 0, 26 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
        cb.Should().Be( 0 );
        CheckMatchAndParse( d1 );

        d1 = new DateTimeStamp( d1.TimeUtc, 5 );
        d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
        cb.Should().Be( 30 );
        d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
        d1.TryFormat( b.AsSpan( 0, 29 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
        cb.Should().Be( 0 );
        CheckMatchAndParse( d1 );

        d1 = new DateTimeStamp( d1.TimeUtc, 99 );
        d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
        cb.Should().Be( 31 );
        d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
        d1.TryFormat( b.AsSpan( 0, 30 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
        cb.Should().Be( 0 );
        CheckMatchAndParse( d1 );

        d1 = new DateTimeStamp( d1.TimeUtc, 255 );
        d1.TryFormat( b.AsSpan(), out cb, ReadOnlySpan<char>.Empty, null );
        cb.Should().Be( 32 );
        d1.ToString().AsSpan().SequenceEqual( b.AsSpan() );
        d1.TryFormat( b.AsSpan( 0, 31 ), out cb, ReadOnlySpan<char>.Empty, null ).Should().BeFalse();
        cb.Should().Be( 0 );
        CheckMatchAndParse( d1 );

        DateTimeStamp.TryParse( "nop", out var d2 ).Should().BeFalse();
        FluentActions.Invoking( () => DateTimeStamp.Parse( "" ) ).Should().Throw<FormatException>();

        static void CheckMatchAndParse( DateTimeStamp d1 )
        {
            var s = d1.ToString();
            DateTimeStamp.TryParse( s.AsSpan(), out var d2 ).Should().BeTrue();
            d2.Should().Be( d1 );
            DateTimeStamp.Parse( d1.ToString().AsSpan() );

            s += "remainder";

            var head = s.AsSpan();
            DateTimeStamp.TryMatch( ref head, out d2 ).Should().BeTrue();
            d2.Should().Be( d1 );
            head.SequenceEqual( "remainder" ).Should().BeTrue();

            DateTimeStamp.TryParse( s, out var failed ).Should().BeFalse();
            failed.Should().Be( DateTimeStamp.Unknown );
            FluentActions.Invoking( () => DateTimeStamp.Parse( s ) ).Should().Throw<FormatException>();
        }
    }

}
