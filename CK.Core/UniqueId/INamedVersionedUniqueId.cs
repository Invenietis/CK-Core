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
        /// It mmust never be null (defaults to <see cref="String.Empty"/>) and can be any string 
        /// in any culture (english US should be used as much a possible).
        /// </summary>
        string PublicName { get; }
    }

}
