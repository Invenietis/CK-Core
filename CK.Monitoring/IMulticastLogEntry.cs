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
    /// Unified interface for multi-cast log entries whatever their <see cref="ILogEntry.LogType"/> or their source <see cref="IMulticastLogInfo.MonitorId"/> is.
    /// All log entries can be exposed through this "rich" interface.
    /// </summary>
    public interface IMulticastLogEntry : ILogEntry, IMulticastLogInfo
    {
        /// <summary>
        /// Gets the depth of the entry in the source <see cref="IMulticastLogInfo.MonitorId"/>.
        /// This is always available (whatever the <see cref="ILogEntry.LogType">LogType</see> is <see cref="LogEntryType.OpenGroup"/>, <see cref="LogEntryType.CloseGroup"/>,
        /// or <see cref="LogEntryType.Line"/>).
        /// </summary>
        new int GroupDepth { get; }

        /// <summary>
        /// Creates a unicast entry from this multi-cast one.
        /// The <see cref="IMulticastLogInfo.MonitorId"/> and <see cref="GroupDepth"/> are lost (but less memory is used).
        /// </summary>
        ILogEntry CreateUnicastLogEntry();
    }
}
