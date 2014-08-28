using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    class UdpCriticalErrorSender : UdpLogSenderBase<string>
    {
        readonly int _port;
        readonly UdpClient _client;
        readonly UdpPacketSplitter _splitter;

        public UdpCriticalErrorSender( int port, int maxUdpPacketSize = 1280 )
            : base( port, maxUdpPacketSize )
        {
            _port = port;
            _client = new UdpClient();
        }

        protected override byte[] PrepareSend( string entry )
        {
            return Encoding.UTF8.GetBytes( entry );
        }
    }
}
