namespace CK.Core;

/// <summary>
/// Contravariant interface for a collector: one can only add elements to a collector and know how much elements
/// there are (Note that if you do not need the <see cref="Count"/>, you should use a simple Fun&lt;T,bool&gt;).
/// </summary>
/// <typeparam name="T">Base type for the elements of the collector.</typeparam>
public interface ICKWritableCollector<in T>
{
    /// <summary>
    /// Gets the count of elements in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds an element to the collection. The exact behavior of this operation
    /// depends on the concrete implementation (duplicates, filters, etc.).
    /// </summary>
    /// <param name="e">Element to add.</param>
    /// <returns>True if the element has been added, false otherwise.</returns>
    bool Add( T e );

}
