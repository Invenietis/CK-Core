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
        ILogSender<IMulticastLogEntry> _logSender;
        ILogSender<string> _crititcalErrorSender;

        /// <summary>
        /// Initializes a new <see cref="UdpHandler"/> bound to its <see cref="UdpHandlerConfiguration"/>.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public UdpHandler( UdpHandlerConfiguration config )
            : base( config )
        {
            _logSender = new UdpLogEntrySender( config.Port, config.MaxPacketSize );
            _crititcalErrorSender = new UdpCriticalErrorSender( config.CriticalErrorsPort, config.MaxPacketSize );
        }

        public override void Initialize( Core.IActivityMonitor monitor )
        {
            using( monitor.OpenTrace().Send( "Initializing Udp Handler '{0}'", Name ) )
            {
                _logSender.Initialize( monitor );
                _crititcalErrorSender.Initialize( monitor );
                SystemActivityMonitor.OnError += SystemActivityMonitor_OnError;
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

            SystemActivityMonitor.OnError -= SystemActivityMonitor_OnError;
            _crititcalErrorSender.Close( monitor );
        }


        void SystemActivityMonitor_OnError( object sender, SystemActivityMonitor.LowLevelErrorEventArgs e )
        {
            _crititcalErrorSender.SendLog( e.ErrorMessage );
        }

    }
}
