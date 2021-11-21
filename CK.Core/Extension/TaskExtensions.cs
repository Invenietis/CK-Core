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
        /// Asynchronously waits for an existing task to be completed within a maximum amount of time (and/or as long
        /// as the <paramref name="cancellation"/> is not signaled), whatever the task is.
        /// <para>
        /// Caution! Just like the standard <see cref="Task.WaitAsync(TimeSpan, CancellationToken)"/> from .Net, this doesn't wait
        /// for this task's completion: it must be used to "query" an existing Task, that belongs to another "activity".
        /// </para>
        /// </summary>
        /// <param name="task">This task.</param>
        /// <param name="millisecondsTimeout">The timeout in milliseconds to wait before returning false.</param>
        /// <param name="cancellation">Optional cancellation token.</param>
        /// <returns>True if <see cref="Task.IsCompleted"/> is true, false if the timeout occurred before.</returns>
        public static async Task<bool> WaitAsync( this Task task, int millisecondsTimeout, CancellationToken cancellation = default )
        {
            // Fast path.
            if( task.IsCompleted ) return true;
            if( millisecondsTimeout == Timeout.Infinite && !cancellation.CanBeCanceled )
            {
                await task.ConfigureAwait( false );
                return true;
            }
            var tcsCancel = new TaskCompletionSource();

            static void DoCancel( object? c ) => ((TaskCompletionSource)c!).TrySetCanceled( default );

            CancellationTokenRegistration rCancel1 = cancellation.CanBeCanceled
                                                     ? cancellation.UnsafeRegister( DoCancel, tcsCancel )
                                                     : default;
            CancellationTokenRegistration rCancel2 = default;
            var ctsDelay = millisecondsTimeout > 0 ? new CancellationTokenSource( millisecondsTimeout ) : null;
            if( ctsDelay != null )
            {
                rCancel2 = ctsDelay.Token.UnsafeRegister( DoCancel, tcsCancel );
            }
            try
            {
                var completedTask = await Task.WhenAny( task, tcsCancel.Task ).ConfigureAwait( false );
                if( completedTask == task )
                {
                    ctsDelay?.Cancel();
                    return true;
                }
            }
            finally
            {
                await rCancel1.DisposeAsync();
                await rCancel2.DisposeAsync();
            }
            return task.IsCompleted;
        }

    }
}
