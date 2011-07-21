using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Minimal implementation of the minimal <see cref="INamedVersionedUniqueId"/> interface.
    /// </summary>
    public class SimpleVersionedUniqueId : IVersionedUniqueId
    {
        /// <summary>
        /// Empty <see cref="INamedVersionedUniqueId"/> bount to the <see cref="Guid.Empty"/> and <see cref="Util.EmptyVersion"/>.
        /// </summary>
        public static readonly IVersionedUniqueId Empty = SimpleNamedVersionedUniqueId.Empty;

        /// <summary>
        /// Gets a <see cref="INamedVersionedUniqueId"/> that must be used to denote an invalid key.
        /// This value MUST NOT be used for anything else than a temporary marker.
        /// </summary>
        public static readonly INamedVersionedUniqueId InvalidId = SimpleNamedVersionedUniqueId.InvalidId;

        /// <summary>
        /// Empty array of <see cref="IUniqueId"/>.
        /// </summary>
        public static readonly INamedVersionedUniqueId[] EmptyArray = SimpleNamedVersionedUniqueId.EmptyArray;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        /// <param name="version">Version object. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        public SimpleVersionedUniqueId( string p, Version version )
            : this( new Guid( p ), version )
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> expressed as a string.</param>
        /// <param name="version">Version as a string. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        public SimpleVersionedUniqueId( string p, string version )
            : this( new Guid( p ), version != null ? new Version( version ) : Util.EmptyVersion )
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleVersionedUniqueId"/> where its <see cref="UniqueId"/> is built from the string
        /// representation of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="p"><see cref="Guid"/> for the <see cref="UniqueId"/>.</param>
        /// <param name="version">Version as a string. If null, <see cref="Util.EmptyVersion"/> is used.</param>
        public SimpleVersionedUniqueId( Guid p, Version version )
        {
            UniqueId = p;
            Version = version ?? Util.EmptyVersion;
        }

        /// <summary>
        /// Gets the unique identifier that this object represents.
        /// </summary>
        public Guid UniqueId { get; private set; }

        /// <summary>
        /// Gets the version of this object.
        /// </summary>
        public Version Version { get; private set; }
    }

}
