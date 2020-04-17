#if NETSTANDARD2_1
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Provides useful extensions to ValueTask.
    /// </summary>
    public static class ValueTaskExtensions
    {
        /// <summary>
        /// Transforms a <see cref="ValueTask{TResult}"/> into a non generic <see cref="ValueTask"/>.
        /// See https://stackoverflow.com/questions/61256813/convert-a-valuetaskt-to-a-non-generic-valuetask 
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="valueTask">This value task.</param>
        /// <returns>The non generic value task.</returns>
        public static ValueTask AsNonGenericValueTask<T>( in this ValueTask<T> valueTask )
        {
            if( valueTask.IsCompletedSuccessfully )
            {
                valueTask.GetAwaiter().GetResult();
                return default;
            }
            return new ValueTask( valueTask.AsTask() );
        }
    }
}
#endif
