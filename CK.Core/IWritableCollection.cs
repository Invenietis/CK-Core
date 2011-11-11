using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Contravariant interface for a collection that allows to <see cref="Clear"/> and <see cref="Remove"/>
    /// element.
    /// </summary>
    /// <typeparam name="T">Base type for the elements of the collection.</typeparam>
    public interface IWritableCollection<in T> : IWritableCollector<T>
    {
        /// <summary>
        /// Clears the collection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes the element if it exists.
        /// </summary>
        /// <param name="e">Element to remove.</param>
        /// <returns>True if the element has been removed, false otherwise.</returns>
        bool Remove( T e );
    
    }
}
