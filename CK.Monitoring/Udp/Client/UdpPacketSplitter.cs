using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    public class UdpPacketSplitter
    {
        int _maxUdpPacketSize;

        public UdpPacketSplitter( int maxUdpPacketSize )
        {
            _maxUdpPacketSize = maxUdpPacketSize;
        }

        public IEnumerable<UdpPacketEnvelope> Split( byte[] buffer )
        {
            short seq = 0;
            int currentByte = 0;
            int maxPacketSize = _maxUdpPacketSize + UdpPacketEnvelope.SizeWithoutPayload;

            Guid correlationId = Guid.NewGuid();
            short count = (short)(buffer.Length / maxPacketSize + 1);

            while( currentByte < buffer.Length )
            {
                UdpPacketEnvelope env = new UdpPacketEnvelope();
                env.CorrelationId = correlationId;
                env.SequenceNumber = seq++;
                env.Count = count;
                env.Version = LogReader.CurrentStreamVersion;

                int length = buffer.Length - maxPacketSize > currentByte ? maxPacketSize : buffer.Length - currentByte;
                env.Payload = new byte[length];

                Array.Copy( buffer, currentByte, env.Payload, 0, length );
                currentByte += length;
                yield return env;
            }
        }
    }
}
