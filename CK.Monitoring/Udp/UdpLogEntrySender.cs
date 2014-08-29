using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Udp
{
    class UdpLogEntrySender : UdpLogSenderBase<IMulticastLogEntry>
    {
        public UdpLogEntrySender( string serverAddress, int port, int maxUdpPacketSize = 1280, IActivityMonitor monitor = null )
            : base( serverAddress, port, maxUdpPacketSize, monitor )
        {
        }

        protected override byte[] PrepareSend( IMulticastLogEntry entry )
        {
            using( System.IO.MemoryStream ms = new System.IO.MemoryStream() )
            using( System.IO.BinaryWriter w = new System.IO.BinaryWriter( ms ) )
            {
                entry.WriteLogEntry( w );
                w.Flush();
                return ms.ToArray();
            }
        }
    }
}
