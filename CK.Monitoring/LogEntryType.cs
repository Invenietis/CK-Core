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
        None = 0,

        /// <summary>
        /// A standard log entry.
        /// Except <see cref="ILogEntry.Conclusions"/> (reserved to <see cref="CloseGroup"/>), all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        Line = 1,

        /// <summary>
        /// Group is opened.
        /// Except <see cref="ILogEntry.Conclusions"/> (reserved to <see cref="CloseGroup"/>), all other properties of the <see cref="ILogEntry"/> may be set.
        /// </summary>
        OpenGroup = 2,

        /// <summary>
        /// Group is closed. 
        /// Note that the only available information are <see cref="ILogEntry.Conclusions"/>, <see cref="ILogEntry.LogLevel"/> and <see cref="ILogEntry.LogTime"/>.
        /// All other properties are set to their default: <see cref="ILogEntry.Text"/> for instance is null.
        /// </summary>
        CloseGroup = 3
    }
}
