using System;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Unifies <see cref="CompletionSource"/> and <see cref="CompletionSource{TResult}"/>.
    /// This is independent of any result type, this only allows exception or cancellation to be set.
    /// </summary>
    public interface ICompletionSource : ICompletion
    {
        /// <summary>
        /// Transitions the <see cref="ICompletion.Task"/> into the <see cref="TaskStatus.Faulted"/> state (this
        /// can be changed by the command's overridden <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation).
        /// <para>
        /// An <see cref="InvalidOperationException"/> is thrown if Task is already in one of the three final
        /// states (<see cref="TaskStatus.RanToCompletion"/>, <see cref="TaskStatus.Faulted"/> or <see cref="TaskStatus.Canceled"/>)00.
        /// </para>
        /// <para>
        /// Note that, on success, <see cref="ICompletion.HasFailed"/> is set to true, regardless of any alteration of
        /// the Task's result by Command's <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation
        /// and this <see cref="ICompletion.OriginalException"/> data is also available.
        /// </para>
        /// </summary>
        void SetException( Exception exception );

        /// <summary>
        /// Attempts to transition the <see cref="ICompletion.Task"/> into the <see cref="TaskStatus.Faulted"/> state (this
        /// can be changed by the command's overridden <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation).
        /// <para>
        /// Note that, on success, <see cref="ICompletion.HasFailed"/> is set to true, regardless of any alteration of
        /// the Task's result by Command's <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation
        /// and this <see cref="ICompletion.OriginalException"/> data is also available.
        /// </para>
        /// </summary> 
        /// <returns>
        /// True if the operation was successful; false if the operation was unsuccessful.
        /// </returns>
        bool TrySetException( Exception exception );

        /// <summary>
        /// Transitions the <see cref="ICompletion.Task"/> into the <see cref="TaskStatus.Canceled"/> state (this can
        /// be changed by the command's overridden <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation).
        /// <para>
        /// An <see cref="InvalidOperationException"/> is thrown if Task is already in one of the three final
        /// states (<see cref="TaskStatus.RanToCompletion"/>, <see cref="TaskStatus.Faulted"/> or <see cref="TaskStatus.Canceled"/>)00.
        /// </para>
        /// <para>
        /// Note that, on success, <see cref="ICompletion.HasBeenCanceled"/> is set to true, regardless of any alteration of
        /// the Task's result by Command's <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation.
        /// </para>
        /// </summary>
        void SetCanceled();

        /// <summary>
        /// Attempts to transition the <see cref="ICompletion.Task"/> into the <see cref="TaskStatus.Canceled"/> state (this can
        /// be changed by the command's overridden <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation).
        /// <para>
        /// Note that, on success, <see cref="ICompletion.HasBeenCanceled"/> is set to true, regardless of any alteration of
        /// the Task's result by Command's <see cref="ICompletable"/> or <see cref="ICompletable{TResult}"/> implementation.
        /// </para>
        /// </summary>
        /// <returns>
        /// True if the operation was successful; false if the operation was unsuccessful.
        /// </returns>
        bool TrySetCanceled();
    }
}
