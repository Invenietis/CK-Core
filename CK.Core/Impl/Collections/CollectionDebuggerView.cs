using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Core.Impl
{
    /// <summary>
    /// Debugger object for <see cref="IReadOnlyCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>
    public sealed class ReadOnlyCollectionDebuggerView<T>
    {
        private IReadOnlyCollection<T> _collection;
        
        /// <summary>
        /// Called by the debugger when needed.
        /// </summary>
        /// <param name="collection">The collection to debug.</param>
        public ReadOnlyCollectionDebuggerView( IReadOnlyCollection<T> collection )
        {
            if( collection == null ) throw new ArgumentNullException( "collection" );
            _collection = collection;
        }

        /// <summary>
        /// Gets the items as a flattened array view.
        /// </summary>
        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
        public T[] Items
        {
            get
            {
                T[] a = new T[_collection.Count];
                int i = 0; 
                foreach( var e in _collection ) a[i++] = e;
                return a;
            }
        }
    }
}
