using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{

    /// <summary>
    /// Type of a <see cref="ILogEntry"/>.
    /// </summary>
    public enum LogEntryType
    {
        /// <summary>
        /// Non applicable.
        /// </summary>
        None,

        /// <summary>
        /// A standard log entry.
        /// Except <see cref="ILogEntry.Conclusions"/> (reserved to <see cref="CloseGroup"/>) and <see cref="ILogEntry.Exception"/> (only <see cref="OpenGroup"/> can carry
        /// an exception), all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        Line,

        /// <summary>
        /// Group is opened.
        /// Except <see cref="ILogEntry.Conclusions"/>, all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        OpenGroup,

        /// <summary>
        /// Group is closed. 
        /// Note that the only available information are <see cref="ILogEntry.Conclusions"/>, <see cref="ILogEntry.LogLevel"/> and <see cref="ILogEntry.LogTimeUtc"/>.
        /// All other properties are set to their default: <see cref="ILogEntry.Text"/> for instance is empty.
        /// </summary>
        CloseGroup
    }
}
