using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin
{
    /// <summary>
    /// Base interface that defines an error associated to an existing <see cref="Exception"/>.
    /// </summary>
    public interface ILogErrorCaught : ILogEntry, ILogErrorCulprit
    {
        /// <summary>
        /// The error itself. Can not be null.
        /// </summary>
        Exception Error { get; }

    }
}
