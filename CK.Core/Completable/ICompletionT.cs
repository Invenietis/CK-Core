using System;
using System.Threading.Tasks;

namespace CK.Core
{

    /// <summary>
    /// Read only aspect of the <see cref="CompletionSource{TResult}"/> that can be
    /// awaited with covariant result.
    /// </summary>
    public interface ICompletion<out TResult> : ICompletion
    {
        /// <summary>
        /// Gets an awaiter for this completion.
        /// </summary>
        new IAwaitable<TResult> GetAwaiter();

        /// <summary>
        /// Gets the <see cref="ICompletion.Task"/>'s result.
        /// This must be called only on successful completion (or if the <see cref="ICompletable{TResult}.OnError(Exception, ref CompletionSource{TResult}.OnError)"/>
        /// has set a result.
        /// </summary>
        TResult Result { get; }
    }
}
