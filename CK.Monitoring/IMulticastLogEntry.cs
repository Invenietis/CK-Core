﻿using CK.Core;
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
        /// Writes the multi-cast entry in a <see cref="BinaryWriter"/>.
        /// Use <see cref="LogEntry.Read"/> to read it back.
        /// </summary>
        /// <param name="w">The binary writer.</param>
        void WriteMultiCastLogEntry( BinaryWriter w );

        /// <summary>
        /// Creates a unicast entry from this multi-cast one.
        /// The <see cref="MonitorId"/> and <see cref="GroupDepth"/> are lost (but less memory is used).
        /// </summary>
        ILogEntry CreateUnicastLogEntry();
    }
}