using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Small adapter that exposes a <see cref="IReadOnlyCollection{T}"/> on a <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CKReandOnlyCollectionOnICollection<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Initializes a new <see cref="CKReandOnlyCollectionOnICollection{T}"/> with a null <see cref="Values"/>.
        /// </summary>
        public CKReandOnlyCollectionOnICollection()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKReandOnlyCollectionOnICollection{T}"/> on a <see cref="ICollection{T}"/>
        /// for the <see cref="Values"/>.
        /// </summary>
        /// <param name="values">Collection to wrap.</param>
        public CKReandOnlyCollectionOnICollection( ICollection<T> values )
        {
            Values = values;
        }

        /// <summary>
        /// Gets or sets the wrapped collection.
        /// </summary>
        public ICollection<T> Values { get; set; }

        /// <summary>
        /// Gets the count of items.
        /// </summary>
        public int Count => Values.Count;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator on the <see cref="Values"/>.</returns>
        public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
