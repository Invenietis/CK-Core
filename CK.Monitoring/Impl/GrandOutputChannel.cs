using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    internal class GrandOutputChannel : IGrandOutputSink
    {
        readonly IGrandOutputSink _common;
        int _inputCount;

        internal GrandOutputChannel( IGrandOutputSink common )
        {
            _common = common;
        }

        internal GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
        {
            Interlocked.Increment( ref _inputCount );
            return new GrandOutputSource( monitor, channelName );
        }

        internal void ReleaseInput( GrandOutputSource source )
        {
            Interlocked.Decrement( ref _inputCount );
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _common.Handle( logEvent );
        }
    }
}
