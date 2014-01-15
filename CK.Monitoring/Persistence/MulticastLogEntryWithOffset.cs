using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    public struct MulticastLogEntryWithOffset
    {
        public readonly IMulticastLogEntry Entry;
        public readonly long Offset;

        public MulticastLogEntryWithOffset( IMulticastLogEntry e, long o )
        {
            Entry = e;
            Offset = o;
        }
    }

}
