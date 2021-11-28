using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// An awaitable covariant result holder.
    /// This is required to support covariance of <see cref="ICompletion{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IAwaitable<out TResult> : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Returns if the value of the awaitable is already available and
        /// the execution may proceed synchronously.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Waits for the awaitable to complete and returns the result.
        /// </summary>
        /// <returns>The final result.</returns>
        TResult GetResult();
    }
}
