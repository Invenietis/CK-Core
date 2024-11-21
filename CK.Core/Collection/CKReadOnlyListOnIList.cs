using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core;

/// <summary>
/// Small adapter that exposes a <see cref="IReadOnlyList{T}"/> on a <see cref="IList{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the list's items.</typeparam>
[ExcludeFromCodeCoverage]
public class CKReadOnlyListOnIList<T> : IReadOnlyList<T>
{
    IList<T> _values;

    /// <summary>
    /// Initializes a new <see cref="CKReadOnlyListOnIList{T}"/> with an empty <see cref="Values"/>.
    /// </summary>
    public CKReadOnlyListOnIList()
    {
        _values = Array.Empty<T>();
    }

    /// <summary>
    /// Initializes a new <see cref="CKReadOnlyListOnIList{T}"/> on a <see cref="IList{T}"/>
    /// for the <see cref="Values"/>.
    /// </summary>
    /// <param name="values">List to wrap.</param>
    public CKReadOnlyListOnIList( IList<T> values )
    {
        _values = values ?? Array.Empty<T>();
    }

    /// <summary>
    /// Gets or sets the wrapped collection.
    /// </summary>
    public IList<T> Values { get => _values; set => _values = value ?? Array.Empty<T>(); }

    /// <summary>
    /// Gets the count of items.
    /// </summary>
    public int Count => Values.Count;

    /// <summary>
    /// Gets the item at a given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index] => Values[index];

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>An enumerator on the <see cref="Values"/>.</returns>
    public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
