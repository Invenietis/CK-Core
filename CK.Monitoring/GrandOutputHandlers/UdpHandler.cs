using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.Udp;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// UDP Handler
    /// </summary>
    public class UdpHandler : HandlerBase
    {
        ILogSender _logSender;

        /// <summary>
        /// Initializes a new <see cref="UdpHandler"/> bound to its <see cref="UdpHandlerConfiguration"/>.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public UdpHandler( UdpHandlerConfiguration config )
            : base( config )
        {
            _logSender = new UdpLogSender( config.Port );
        }

        public override void Initialize( Core.IActivityMonitor monitor )
        {
            using( monitor.OpenTrace().Send( "Initializing Udp Handler '{0}'", Name ) )
            {
                _logSender.Initialize( monitor );
            }
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            _logSender.SendLog( logEvent.Entry );
        }

        public override void Close( IActivityMonitor monitor )
        {
            monitor.Info().Send( "Closing UdpClient for UDP handler '{0}'.", Name );
            _logSender.Close( monitor );
        }
    }
}
