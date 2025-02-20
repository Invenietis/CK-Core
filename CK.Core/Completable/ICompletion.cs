using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CK.Core;

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
    /// Gets the exception if an exception has been set.
    /// Just like <see cref="HasFailed"/> and <see cref="HasBeenCanceled"/>, this is independent
    /// of any error transformation applied by the <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/>
    /// OnError implemented method: it is always captured and available if an exception has been set.
    /// </summary>
    Exception? OriginalException { get; }

    /// <summary>
    /// Gets whether the command completed (succeed, canceled or on error).
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets whether the command succeeded (SetResult or TrySetResult methods have been called successfully).
    /// When this is true, the <see cref="Task.Status"/> is also on success (<see cref="TaskStatus.RanToCompletion"/>).
    /// </summary>
    bool HasSucceed { get; }

    /// <summary>
    /// Gets whether the command failed (SetException or TrySetException have been called successfully).
    /// When this is true, the <see cref="Task.Status"/> can be also on error (<see cref="TaskStatus.Faulted"/>),
    /// but if a transformation occurred, the task may be on success (<see cref="TaskStatus.RanToCompletion"/>) or
    /// canceled (<see cref="TaskStatus.Canceled"/>.
    /// </summary>
    bool HasFailed { get; }

    /// <summary>
    /// Gets whether the command has been canceled (SetCanceled or TrySetCanceled have been called successfully).
    /// When this is true, the <see cref="Task.Status"/> can be also canceled (<see cref="TaskStatus.Canceled"/>),
    /// but if a transformation occurred, the task can be on success (<see cref="TaskStatus.RanToCompletion"/>).
    /// </summary>
    bool HasBeenCanceled { get; }
}
