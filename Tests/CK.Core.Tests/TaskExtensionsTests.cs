using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core.Tests;

[TestFixture]
public class TaskExtensionsTests
{
    [Test]
    public async Task WaitAsync_resolved_before_timeout_returns_true_Async()
    {
        var t = Task.Delay( 100 );
        t.IsCompleted.Should().BeFalse();
        bool gotIt = await t.WaitForTaskCompletionAsync( 200 );
        gotIt.Should().BeTrue();
        t.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Test]
    public async Task WaitAsync_timeout_returns_false_Async()
    {
        var t = Task.Delay( 200 );
        DateTime now = DateTime.UtcNow;
        t.IsCompleted.Should().BeFalse();
        bool gotIt = await t.WaitForTaskCompletionAsync( 100 );
        gotIt.Should().BeFalse();
        (DateTime.UtcNow - now).Should().NotBeCloseTo( TimeSpan.Zero, precision: TimeSpan.FromMilliseconds( 1 ) );
    }

    [Test]
    public async Task WaitAsync_on_completed_is_an_immediate_true_Async()
    {
        var t = Task.CompletedTask;
        DateTime now = DateTime.UtcNow;
        bool gotIt = await t.WaitForTaskCompletionAsync( 100 );
        gotIt.Should().BeTrue();
        (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero, precision: TimeSpan.FromMilliseconds( 1 ) );
    }

    [Test]
    public async Task WaitAsync_on_cancelled_is_an_immediate_true_Async()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var t = Task.FromCanceled( cts.Token );
        DateTime now = DateTime.UtcNow;
        bool gotIt = await t.WaitForTaskCompletionAsync( 100 );
        gotIt.Should().BeTrue();
        (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero, precision: TimeSpan.FromMilliseconds( 1 ) );
    }

    [Test]
    public async Task WaitAsync_on_error_is_an_immediate_true_Async()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetException( new Exception( "Just for fun." ) );

        DateTime now = DateTime.UtcNow;
        bool gotIt = await tcs.Task.WaitForTaskCompletionAsync( 100 );
        gotIt.Should().BeTrue();
        (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero, precision: TimeSpan.FromMilliseconds( 1 ) );
    }

    [Test]
    public async Task WaitAsync_allows_negative_Timeout_Infinite_Async()
    {
        var tcs = new TaskCompletionSource();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 100 );
            tcs.SetResult();
        } );
        bool gotIt = await tcs.Task.WaitForTaskCompletionAsync( Timeout.Infinite );
        gotIt.Should().BeTrue();
    }

    [TestCase( "Error" )]
    [TestCase( "Canceled" )]
    [TestCase( "Success" )]
    public async Task WaitAsync_on_error_canceled_or_success_returns_true_Async( string action )
    {
        bool actionDone = false;
        var tcs = new TaskCompletionSource<string>();
        _ = Task.Run( async () =>
        {
            await Task.Delay( 150 );
            actionDone = true;
            switch( action )
            {
                case "Error": tcs.SetException( new Exception( "Just for fun." ) ); break;
                case "Cancel": tcs.SetCanceled(); break;
                default: tcs.SetResult( "Yop!" ); break;
            }
        } );

        DateTime now = DateTime.UtcNow;
        bool gotIt = await tcs.Task.WaitForTaskCompletionAsync( 1000 );
        actionDone.Should().BeTrue();
        gotIt.Should().BeTrue();
        (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.FromMilliseconds( 150 ), precision: TimeSpan.FromMilliseconds( 50 ) );

        switch( action )
        {
            case "Error": tcs.Task.IsFaulted.Should().BeTrue(); break;
            case "Cancel": tcs.Task.IsCanceled.Should().BeTrue(); break;
            default: tcs.Task.IsCompletedSuccessfully.Should().BeTrue(); break;
        }

    }

    [Test]
    public async Task WaitAsync_itself_can_be_canceled_via_the_CancellationToken_and_returns_false_Async()
    {
        // The cancellationToken fires DURING the WaitAsync.
        {
            using var cts = new CancellationTokenSource( 150 );
            var tcs = new TaskCompletionSource<string>();

            DateTime now = DateTime.UtcNow;
            bool gotIt = await tcs.Task.WaitForTaskCompletionAsync( 500, cts.Token );
            gotIt.Should().BeFalse();
            cts.Token.IsCancellationRequested.Should().BeTrue();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.FromMilliseconds( 150 ), precision: TimeSpan.FromMilliseconds( 50 ) );
        }
        // The cancellationToken has fired BEFORE the WaitAsync.
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var tcs = new TaskCompletionSource<string>();
            DateTime now = DateTime.UtcNow;
            bool gotIt = await tcs.Task.WaitForTaskCompletionAsync( 500, cts.Token );
            gotIt.Should().BeFalse();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero, precision: TimeSpan.FromMilliseconds( 1 ) );
        }

    }

}
