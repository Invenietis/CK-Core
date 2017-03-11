using System;

namespace CK.Core
{
    /// <summary>
    /// Minimal interface that enables any type (specially reference type) to
    /// expose a <see cref="Guid"/>.
    /// </summary>
    public interface IUniqueId
    {
        /// <summary>
        /// Gets the unique identifier associated to this object.
        /// </summary>
        Guid UniqueId { get; }
    }
    
}
