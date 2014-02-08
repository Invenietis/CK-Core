using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Information required by a <see cref="IMulticastLogEntry"/>.
    /// </summary>
    public interface IMulticastLogInfo
    {
        /// <summary>
        /// Gets the monitor identifier.
        /// </summary>
        Guid MonitorId { get; }

        /// <summary>
        /// Gets the depth of the entry in the source <see cref="MonitorId"/>.
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
    }
}
