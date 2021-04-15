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
    }
}
