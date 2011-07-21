using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Abstraction available to any object that exposes an <see cref="ErrorMessage"/>.
    /// </summary>
    public interface ISimpleErrorMessage
    {
        /// <summary>
        /// Gets whether this error message should be considered only as a warning.
        /// </summary>
        bool IsWarning { get; }

        /// <summary>
        /// Gets an error message. May be null if no error is carried by the object.
        /// </summary>
        string ErrorMessage { get; }
    }
}
