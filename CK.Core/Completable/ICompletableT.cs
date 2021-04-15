using System;

namespace CK.Core
{
    /// <summary>
    /// Abstraction of a completable (typically a command) with a result that
    /// holds its <see cref="Completion"/>.
    /// <para>
    /// The OnError and OnCanceled methods enable the error and cancel
    /// strategies of the <see cref="CompletionSource{TResult}"/> to be provided by the completable itself.
    /// </para>
    /// </summary>
    public interface ICompletable<TResult>
    {
        /// <summary>
        /// Gets the completion of this command.
        /// </summary>
        ICompletion<TResult> Completion { get; }

        /// <summary>
        /// Called by the <see cref="CompletionSource{TResult}"/> when a error is set.
        /// The default implementation should call <see cref="CompletionSource{TResult}.OnError.SetException(Exception)"/>.
        /// </summary>
        /// <param name="ex">The error.</param>
        /// <param name="result">Captures the result: one of the 3 available methods must be called.</param>
        void OnError( Exception ex, ref CompletionSource<TResult>.OnError result );

        /// <summary>
        /// Called by the <see cref="CompletionSource{TResult}"/> when a cancellation occurred.
        /// The default implementation should call <see cref="CompletionSource{TResult}.OnCanceled.SetCanceled()"/>.
        /// </summary>
        /// <param name="result">Captures the result: one of the 2 available methods must be called.</param>
        void OnCanceled( ref CompletionSource<TResult>.OnCanceled result );
    }
}
