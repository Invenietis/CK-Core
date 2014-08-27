using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Monitoring.Server
{
    public class LogEntryEventArgs : EventArgs
    {
        public readonly IMulticastLogEntry LogEntry;

        public LogEntryEventArgs( IMulticastLogEntry entry )
        {
            LogEntry = entry;
        }
    }

}
