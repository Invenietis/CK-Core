using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Read only aspect of the <see cref="ICompletionSource"/> that unifies <see cref="CompletionSource"/>
    /// and <see cref="CompletionSource{TResult}"/>
    /// </summary>
    public interface ICompletion
    {
        /// <summary>
        /// Gets the underlying task (that may be a <see cref="Task{TResult}"/>) for this completion.
        /// </summary>
        Task Task { get; }

        /// <summary>
        /// Gets an awaiter for this completion.
        /// </summary>
        TaskAwaiter GetAwaiter();

        /// <summary>
        /// Gets the exception data if an exception has been set.
        /// Just like <see cref="HasFailed"/> and <see cref="HasBeenCanceled"/>, this is independent
        /// of any error transformation applied by the <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/>
        /// OnError implemented method: it is always captured and available if an exception has been set.
        /// </summary>
        CKExceptionData? Exception { get; }

        /// <summary>
        /// Gets whether the command completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets whether the command succeeded (SetResult or TrySetResult methods have been called).
        /// </summary>
        bool HasSucceed { get; }

        /// <summary>
        /// Gets whether the command failed (SetException or TrySetException have been called).
        /// </summary>
        bool HasFailed { get; }

        /// <summary>
        /// Gets whether the command has been canceled (SetCanceled or TrySetCanceled have been called).
        /// </summary>
        bool HasBeenCanceled { get; }
    }
}
