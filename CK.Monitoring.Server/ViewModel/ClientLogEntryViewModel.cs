using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class ClientLogEntryViewModel
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

        private ClientLogEntryViewModel()
        {
        }

        public ClientLogEntryViewModel( IMulticastLogEntry logEntry )
        {
            _logEntry = logEntry;
        }


        public static ClientLogEntryViewModel Missing = new ClientLogEntryViewModel();
    }
}
