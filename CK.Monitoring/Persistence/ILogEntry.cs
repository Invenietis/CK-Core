using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{

    /// <summary>
    /// Unified interface for log entries whatever their <see cref="LogType"/> is.
    /// All log entries can be exposed through this "rich" interface.
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// Gets the type of the log entry.
        /// </summary>
        LogEntryType LogType { get; }

        /// <summary>
        /// Get the log level (between <see cref="LogLevel.Trace"/> and <see cref="LogLevel.Fatal"/>).
        /// This is available whatever <see cref="LogType"/> is.
        /// </summary>
        LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the log text.
        /// Null when when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets the tags for this entry.
        /// Always equals to <see cref="ActivityMonitor.EmptyTag"/> when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        CKTrait Tags { get; }

        /// <summary>
        /// Gets the log time.
        /// </summary>
        DateTime LogTimeUtc { get; }
        
        /// <summary>
        /// Gets the exception data if any (can be not null only when <see cref="LogType"/> is <see cref="LogEntryType.OpenGroup"/>: exceptions are exclusively carried by groups).
        /// </summary>
        CKExceptionData Exception { get; }

        /// <summary>
        /// Gets any group conclusion. 
        /// Always null except of course when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get; }
    }
}
