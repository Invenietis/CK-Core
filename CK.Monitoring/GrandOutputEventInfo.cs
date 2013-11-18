using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    /// <summary>
    /// Captures a log data with the current <see cref="IActivityMonitor.Topic"/>.
    /// </summary>
    public struct GrandOutputEventInfo
    {
        /// <summary>
        /// A unified, immutable, log data.
        /// </summary>
        public readonly IMulticastLogEntry Entry;
        
        /// <summary>
        /// The current topic of the monitor when the log occurred. 
        /// </summary>
        public readonly string Topic;

        /// <summary>
        /// Initializes a new <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="e">Log entry.</param>
        /// <param name="topic">Current topic.</param>
        public GrandOutputEventInfo( IMulticastLogEntry e, string topic )
        {
            Entry = e;
            Topic = topic;
        }
    }
}
