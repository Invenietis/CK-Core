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
    public sealed class UdpHandler : HandlerBase
    {
        ILogSender<IMulticastLogEntry> _logSender;
        ILogSender<string> _crititcalErrorSender;

        readonly UdpHandlerConfiguration _config;

        /// <summary>
        /// Initializes a new <see cref="UdpHandler"/> bound to its <see cref="UdpHandlerConfiguration"/>.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public UdpHandler( UdpHandlerConfiguration config )
            : base( config )
        {
            if( config == null ) throw new ArgumentNullException( "config" );

            _config = config;
        }

        public override void Initialize( Core.IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );

            using( monitor.OpenTrace().Send( "Initializing Udp Handler '{0}'", Name ) )
            {
                UdpHandlerConfiguration c = _config;
                _logSender = new UdpLogEntrySender( c.ServerIPAddress, c.Port, c.MaxPacketSize, monitor );
                _crititcalErrorSender = new UdpCriticalErrorSender( c.ServerIPAddress, c.CriticalErrorsPort, c.MaxPacketSize, monitor );

                SystemActivityMonitor.OnError += SystemActivityMonitor_OnError;
            }
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            _logSender.SendLog( logEvent.Entry );
        }

        public override void Close( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );

            monitor.Info().Send( "Closing UdpClient for UDP handler '{0}'.", Name );
            _logSender.Dispose();

            SystemActivityMonitor.OnError -= SystemActivityMonitor_OnError;
            _crititcalErrorSender.Dispose();
        }

        void SystemActivityMonitor_OnError( object sender, SystemActivityMonitor.LowLevelErrorEventArgs e )
        {
            _crititcalErrorSender.SendLog( e.ErrorMessage );
        }
    }
}
