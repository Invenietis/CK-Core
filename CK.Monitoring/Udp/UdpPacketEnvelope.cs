using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    public class UdpPacketEnvelope
    {
        public static int SizeWithoutPayload;

        static UdpPacketEnvelope()
        {
            SizeWithoutPayload = 16 + sizeof( short ) + sizeof( short ) + sizeof( int );
        }

        public Guid CorrelationId;

        public short SequenceNumber;

        public short Count;

        public int Version;

        public byte[] Payload;

        public byte[] ToByteArray()
        {
            using( MemoryStream ms = new MemoryStream() )
            {
                using( BinaryWriter w = new BinaryWriter( ms, Encoding.UTF8, leaveOpen: true ) )
                {
                    w.Write( CorrelationId.ToByteArray() );
                    w.Write( SequenceNumber );
                    w.Write( Count );
                    w.Write( Version );
                    w.Write( Payload );
                }
                return ms.ToArray();
            }
        }

        public static UdpPacketEnvelope FromByteArray( byte[] buffer )
        {
            using( MemoryStream ms = new MemoryStream( buffer ) )
            {
                using( BinaryReader r = new BinaryReader( ms, Encoding.UTF8 ) )
                {
                    UdpPacketEnvelope e = new UdpPacketEnvelope();
                    e.CorrelationId = new Guid( r.ReadBytes( 16 ) );
                    e.SequenceNumber = r.ReadInt16();
                    e.Count = r.ReadInt16();
                    e.Version = r.ReadInt32();
                    e.Payload = r.ReadBytes( buffer.Length - UdpPacketEnvelope.SizeWithoutPayload );
                    return e;
                }
            }
        }

    }
}
