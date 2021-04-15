using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Small adapter that exposes a <see cref="IReadOnlyCollection{T}"/> on a <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    [ExcludeFromCodeCoverage]
    [Obsolete( "Please use the AsIReadOnlyCollection() extension method (that handles covariance instead).", true)]
    public class CKReadOnlyCollectionOnICollection<T> : IReadOnlyCollection<T>
    {
        ICollection<T> _values;

        /// <summary>
        /// Initializes a new <see cref="CKReadOnlyCollectionOnICollection{T}"/> with an empty <see cref="Values"/>.
        /// </summary>
        public CKReadOnlyCollectionOnICollection()
        {
            _values = Array.Empty<T>();
        }

        /// <summary>
        /// Initializes a new <see cref="CKReadOnlyCollectionOnICollection{T}"/> on a <see cref="ICollection{T}"/>
        /// for the <see cref="Values"/>.
        /// </summary>
        /// <param name="values">Collection to wrap. Can be null.</param>
        public CKReadOnlyCollectionOnICollection( ICollection<T> values )
        {
            _values = values ?? Array.Empty<T>();
        }

        /// <summary>
        /// Gets or sets the wrapped collection.
        /// Can never be null (default to an empty collection).
        /// </summary>
        public ICollection<T> Values { get => _values; set => _values = value ?? Array.Empty<T>(); }

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
