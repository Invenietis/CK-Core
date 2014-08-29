using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring.Udp;

namespace CK.Monitoring.Server
{
    class MultiCastLogEntryComposer : UdpPacketComposer<IMulticastLogEntry>
    {
        protected override IMulticastLogEntry Recompose( UdpPacketEnvelope[] envelopes )
        {
            if( envelopes.Length == 0 ) throw new ArgumentException( "UdpPacketEnvelope array muust not be empty", "envelopes" );

            int version = envelopes[0].Version;
            using( MemoryStream ms = new MemoryStream() )
            {
                using( BinaryWriter w = new BinaryWriter( ms, Encoding.UTF8, leaveOpen: true ) )
                {
                    foreach( var e in envelopes ) w.Write( e.Payload );
                }
                ms.Seek( 0, SeekOrigin.Begin );
                using( BinaryReader r = new BinaryReader( ms, Encoding.UTF8, leaveOpen: true ) )
                {
                    bool badEof = false;
                    return (IMulticastLogEntry)LogEntry.Read( r, version, out badEof );
                }
            }
        }
    }


}
