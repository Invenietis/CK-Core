using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class ClientLogEntry
    {
        readonly IMulticastLogEntry _logEntry;

        public IMulticastLogEntry LogEntry
        {
            get { return _logEntry; }
        }

        public bool IsMissingEntry
        {
            get { return LogEntry == null; }
        }

        private ClientLogEntry()
        {
        }

        public ClientLogEntry( IMulticastLogEntry logEntry )
        {
            _logEntry = logEntry;
        }


        public static ClientLogEntry Missing = new ClientLogEntry();
    }
}
