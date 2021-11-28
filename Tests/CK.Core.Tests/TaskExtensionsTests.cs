using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class TaskExtensionsTests
    {
        [Test]
        public async Task WaitAsync_resolved_before_timeout_returns_true()
        {
            var t = Task.Delay( 100 );
            t.IsCompleted.Should().BeFalse();
            bool gotIt = await t.WaitAsync( 200 );
            gotIt.Should().BeTrue();
            t.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Test]
        public async Task WaitAsync_timeout_returns_false()
        {
            var t = Task.Delay( 200 );
            DateTime now = DateTime.UtcNow;
            t.IsCompleted.Should().BeFalse();
            bool gotIt = await t.WaitAsync( 100 );
            gotIt.Should().BeFalse();
            (DateTime.UtcNow - now).Should().NotBeCloseTo( TimeSpan.Zero );
        }

        [Test]
        public async Task WaitAsync_on_completed_is_an_immediate_true()
        {
            var t = Task.CompletedTask;
            DateTime now = DateTime.UtcNow;
            bool gotIt = await t.WaitAsync( 100 );
            gotIt.Should().BeTrue();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero );
        }

        [Test]
        public async Task WaitAsync_on_cancelled_is_an_immediate_true()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            var t = Task.FromCanceled( cts.Token );
            DateTime now = DateTime.UtcNow;
            bool gotIt = await t.WaitAsync( 100 );
            gotIt.Should().BeTrue();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero );
        }

        [Test]
        public async Task WaitAsync_on_error_is_an_immediate_true()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException( new Exception( "Just for fun." ) );

            DateTime now = DateTime.UtcNow;
            bool gotIt = await tcs.Task.WaitAsync( 100 );
            gotIt.Should().BeTrue();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero );
        }

        [TestCase( "Error" )]
        [TestCase( "Canceled" )]
        [TestCase( "Success" )]
        public async Task WaitAsync_on_error_canceled_or_success_returns_true( string action )
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
            bool gotIt = await tcs.Task.WaitAsync( 1000 );
            actionDone.Should().BeTrue();
            gotIt.Should().BeTrue();
            (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.FromMilliseconds( 150 ), precision: 50 );

            switch( action )
            {
                case "Error": tcs.Task.IsFaulted.Should().BeTrue(); break;
                case "Cancel": tcs.Task.IsCanceled.Should().BeTrue(); break;
                default: tcs.Task.IsCompletedSuccessfully.Should().BeTrue(); break;
            }

        }

        [Test]
        public async Task WaitAsync_itself_can_be_canceled_via_the_CancellationToken_and_returns_false()
        {
            // The cancellationToken fires DURING the WaitAsync.
            {
                using var cts = new CancellationTokenSource( 150 );
                var tcs = new TaskCompletionSource<string>();

                DateTime now = DateTime.UtcNow;
                bool gotIt = await tcs.Task.WaitAsync( 500, cts.Token );
                gotIt.Should().BeFalse();
                cts.Token.IsCancellationRequested.Should().BeTrue();
                (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.FromMilliseconds( 150 ), precision: 50 );
            }
            // The cancellationToken has fired BEFORE the WaitAsync.
            {
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                var tcs = new TaskCompletionSource<string>();
                DateTime now = DateTime.UtcNow;
                bool gotIt = await tcs.Task.WaitAsync( 500, cts.Token );
                gotIt.Should().BeFalse();
                (DateTime.UtcNow - now).Should().BeCloseTo( TimeSpan.Zero );
            }

        }

    }
}
