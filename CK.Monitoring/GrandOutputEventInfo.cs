using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// The current topic of the monitor when the log occured. 
        /// </summary>
        public readonly string Topic;

        /// <summary>
        /// Initializes a new <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="e">Log entry.</param>
        public GrandOutputEventInfo( IMulticastLogEntry e, string topic )
        {
            Entry = e;
            Topic = topic;
        }
    }
}
