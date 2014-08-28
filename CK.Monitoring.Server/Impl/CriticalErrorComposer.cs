using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    class CriticalErrorComposer : UdpPacketComposer<string>
    {
        protected override string Recompose( Udp.UdpPacketEnvelope[] envelopes )
        {
            string log = String.Empty;
            foreach( var env in envelopes )
            {
                log += Encoding.UTF8.GetString( env.Payload );
            }
            return log;
        }
    }
}
