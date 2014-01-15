using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CK.Monitoring
{

    /// <summary>
    /// Unified interface for multi-cast log entries whatever their <see cref="ILogEntry.LogType"/> or their source <see cref="MonitorId"/> is.
    /// All log entries can be exposed through this "rich" interface.
    /// </summary>
    public interface IMulticastLogEntry : ILogEntry
    {
        /// <summary>
        /// Gets the monitor identifier.
        /// </summary>
        Guid MonitorId { get; }

        /// <summary>
        /// Gets the depth of the entry in the source <see cref="MonitorId"/>.
        /// This is always available (whatever the <see cref="ILogEntry.LogType">LogType</see> is <see cref="LogEntryType.OpenGroup"/>, <see cref="LogEntryType.CloseGroup"/>,
        /// or <see cref="LogEntryType.Line"/>).
        /// </summary>
        int GroupDepth { get; }

        /// <summary>
        /// Gets the previous entry type. <see cref="LogEntryType.None"/> when unknown.
        /// </summary>
        LogEntryType PreviousEntryType { get; }

        /// <summary>
        /// Gets the previous log time. <see cref="DateTimeStamp.Unknown"/> when unknown.
        /// </summary>
        DateTimeStamp PreviousLogTime { get; }

        /// <summary>
        /// Creates a unicast entry from this multi-cast one.
        /// The <see cref="MonitorId"/> and <see cref="GroupDepth"/> are lost (but less memory is used).
        /// </summary>
        ILogEntry CreateUnicastLogEntry();
    }
}
