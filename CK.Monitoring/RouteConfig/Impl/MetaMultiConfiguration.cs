using System.Collections.Generic;
using System.Linq;


namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Base class for meta configuration object that handles one or more items.
    /// </summary>
    public abstract class MetaMultiConfiguration<T> : MetaConfiguration
    {
        readonly List<T> _items;

        /// <summary>
        /// Initializes a configuration with at least one item.
        /// </summary>
        /// <param name="first">First and required item.</param>
        /// <param name="other">Optional multiple items.</param>
        public MetaMultiConfiguration( T first, params T[] other )
        {
            _items = new List<T>();
            if( first != null ) _items.Add( first );
            _items.AddRange( other.Where( i => i != null ) );
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        protected IReadOnlyList<T> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Adds a new item.
        /// </summary>
        /// <param name="item">Item to add.</param>
        protected void Add( T item )
        {
            if( item != null ) _items.Add( item );
        }

    }
}
