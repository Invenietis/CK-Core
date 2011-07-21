using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Minimal implementation of the minimal <see cref="IUniqueId"/> interface.
    /// </summary>
    public class SimpleUniqueId : IUniqueId
    {
        /// <summary>
        /// Empty <see cref="IUniqueId"/> bount to the <see cref="Guid.Empty"/>.
        /// </summary>
        public static readonly IUniqueId Empty = SimpleNamedVersionedUniqueId.Empty;

        /// <summary>
        /// Gets a <see cref="IUniqueId"/> that must be used to denote an invalid key.
        /// This value MUST NOT be used for anything else than a marker.
        /// </summary>
        public static readonly IUniqueId InvalidId = SimpleNamedVersionedUniqueId.InvalidId;

        /// <summary>
        /// Empty array of <see cref="IUniqueId"/>.
        /// </summary>
        public static readonly IUniqueId[] EmptyArray = SimpleNamedVersionedUniqueId.EmptyArray;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        public SimpleUniqueId( string p ) 
        { 
            UniqueId = new Guid( p ); 
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleUniqueId"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> for the <see cref="UniqueId"/>.</param>
        public SimpleUniqueId( Guid p ) 
        { 
            UniqueId = p; 
        }

        /// <summary>
        /// Gets the unique identifier that this object represents.
        /// </summary>
        public Guid UniqueId { get; private set; }
    }

    
}
