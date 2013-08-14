using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;

namespace CK.Monitoring.Impl
{

    /// <summary>
    /// This kind of channel is bound to a <see cref="GrandOutputClient"/>. It is returned by <see cref="GrandOutput.ObtainChannel"/>
    /// when a configuration is being applied.
    /// </summary>
    class BufferingChannel : IChannel
    {
        readonly IGrandOutputSink _commonSink;
        readonly List<GrandOutputEventInfo> _buffer;
        readonly Guid _monitorId;

        internal BufferingChannel( Guid monitorId, IGrandOutputSink commonSink )
        {
            _commonSink = commonSink;
            _buffer = new List<GrandOutputEventInfo>();
            _monitorId = monitorId;
        }

        public GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
        {
            return new GrandOutputSource( monitor, channelName );
        }

        public void ReleaseInput( GrandOutputSource source )
        {
        }

        public LogLevelFilter MinimalFilter
        {
            get { return LogLevelFilter.None; }
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _commonSink.Handle( logEvent );
            _buffer.Add( logEvent );
        }
    }
}
