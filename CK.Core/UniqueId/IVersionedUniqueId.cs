using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Extends <see cref="IUniqueId"/> to associate a <see cref="P:Version"/> number.
    /// </summary>
    public interface IVersionedUniqueId : IUniqueId
    {
        /// <summary>
        /// Gets the version number associated to this object.
        /// Never null: defaults to <see cref="Util.EmptyVersion"/>.
        /// </summary>
        Version Version { get; }
    }

}
