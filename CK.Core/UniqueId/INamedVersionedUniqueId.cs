using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{

    /// <summary>
    /// Extends <see cref="IVersionedUniqueId"/> to associate a <see cref="P:PublicName"/> descriptor.
    /// </summary>
    public interface INamedVersionedUniqueId : IVersionedUniqueId
    {
        /// <summary>
        /// Gets the public name of this object. 
        /// Never null: defaults to <see cref="String.Empty"/>.
        /// </summary>
        string PublicName { get; }
    }

}
