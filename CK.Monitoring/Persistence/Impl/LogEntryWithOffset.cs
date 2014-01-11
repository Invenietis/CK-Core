using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    public struct LogEntryWithOffset
    {
        public readonly ILogEntry Entry;
        public readonly long Offset;

        public LogEntryWithOffset( ILogEntry e, long o )
        {
            Entry = e;
            Offset = o;
        }
    }

}
