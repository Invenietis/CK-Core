using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core.Impl;

namespace CK.Monitoring
{
    /// <summary>
    /// Defines the source of log events for a <see cref="GrandOutput"/> as seen by <see cref="IGrandOutputSink"/> in a <see cref="IGrandOutputEventInfo"/>.
    /// </summary>
    public class GrandOutputSource
    {
        readonly Guid _monitorId;
        readonly string _channelName;
        readonly int _depth;

        internal GrandOutputSource( IActivityMonitorImpl monitor, string channelName )
        {
            var g = monitor.Current;
            _depth = g != null ? g.Depth : 0;
            _monitorId = monitor.UniqueId;
            _channelName = channelName;
        }

        /// <summary>
        /// Gets the monitor identity.
        /// </summary>
        public Guid MonitorId
        {
            get { return _monitorId; }
        }

        /// <summary>
        /// Gets the full channel name.
        /// </summary>
        public string ChannelName
        {
            get { return _channelName; }
        }

        /// <summary>
        /// Gets the initial number of opened groups in the origin monitor when this source has been created.
        /// </summary>
        public int InitialDepth
        {
            get { return _depth; }
        }
    }
}
