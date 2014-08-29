using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Udp
{
    class UdpCriticalErrorSender : UdpLogSenderBase<string>
    {
        public UdpCriticalErrorSender( string serverAddress, int port, int maxUdpPacketSize, IActivityMonitor monitor = null )
            : base( serverAddress, port, maxUdpPacketSize, monitor )
        {
        }

        protected override byte[] PrepareSend( string entry )
        {
            return Encoding.UTF8.GetBytes( entry );
        }
    }
}
