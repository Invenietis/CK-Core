using System;
using System.Collections.Generic;

namespace CK.Core
{
    public static class EnumerableForeach
    {
        /// <summary>
        /// Run foreach on the enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="this">The <see cref="IEnumerable{T}"/> to enumerate.</param>
        /// <param name="action">The action to run on the element enumerated.</param>
        public static void ForEach<T>( this IEnumerable<T> @this, Action<T> action )
        {
            foreach( T item in @this )
            {
                action( item );
            }
        }
    }
}
