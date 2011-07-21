using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Basic implementation of <see cref="ISimpleErrorMessage"/>.
    /// </summary>
    public class SimpleErrorMessage : ISimpleErrorMessage
    {
        /// <summary>
        /// Initializes a new <see cref="SimpleErrorMessage"/> with 
        /// a null <see cref="ErrorMessage"/>.
        /// </summary>
        public SimpleErrorMessage()
        {
        }

        /// <summary>
        /// Gets or sets whether this error message 
        /// should be considered only as a warning.
        /// </summary>
        public bool IsWarning { get; set;  }

        /// <summary>
        /// Gets or sets an error message. Can be null.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
