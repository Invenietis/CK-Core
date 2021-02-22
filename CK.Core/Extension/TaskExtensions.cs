using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace CK.Core
{
    /// <summary>
    /// Provides the <see cref="WaitAsync"/> extension method on <see cref="Task"/>.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Asynchronously waits for a task to be completed within a maximum amount of time (and/or as long
        /// as the <paramref name="cancellation"/> is not signaled), whatever the task is.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds to wait before returning false.</param>
        /// <param name="cancellation">Optional cancellation token.</param>
        /// <returns>True if <see cref="IsCompleted"/> is true, false if the timeout occurred before.</returns>
        public static async Task<bool> WaitAsync( this Task task, int millisecondsTimeout, CancellationToken cancellation = default )
        {
            // Fast path.
            if( task.IsCompleted ) return true;

            // Handling cancellation token: if required, a cancellation task will be signaled through a token registration.
            Task? tCancel;
            CancellationTokenRegistration? rCancel;

            if( cancellation.CanBeCanceled )
            {
                if( cancellation.IsCancellationRequested )
                {
                    return task.IsCompleted;
                }
                var tcsCancel = new TaskCompletionSource<object?>();
                rCancel = cancellation.Register( () => tcsCancel.SetCanceled(), useSynchronizationContext: false );
                tCancel = tcsCancel.Task;
            }
            else
            {
                tCancel = null;
                rCancel = null;
            }

            if( millisecondsTimeout == Timeout.Infinite && tCancel == null )
            {
                await task.ConfigureAwait( false );
                return true;
            }
            // Using a CancellationTokenSource to dispose the internal Delay timer as soon as the task has completed.
            var ctsDelay = new CancellationTokenSource();
            try
            {
                var any = tCancel == null
                            ? Task.WhenAny( task, Task.Delay( millisecondsTimeout, ctsDelay.Token ) ).ConfigureAwait( false )
                            : Task.WhenAny( task, Task.Delay( millisecondsTimeout, ctsDelay.Token ), tCancel ).ConfigureAwait( false );
                var completedTask = await any;
                if( completedTask == task )
                {
                    ctsDelay.Cancel();
                    return true;
                }
                return task.IsCompleted;
            }
            finally
            {
                // Disposing the CancellationTokenSource is not really necessary since
                // it is not a linked one (we do the "link" manually here) nor a timer dependent one,
                // but since we have a finally here, let's do it.
                ctsDelay.Dispose();
                rCancel?.Dispose();
            }
        }


    }
}
