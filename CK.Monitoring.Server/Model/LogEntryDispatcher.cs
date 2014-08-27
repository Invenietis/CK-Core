using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class LogEntryDispatcher
    {
        public event EventHandler<LogEntryEventArgs> LogEntryReceived;

        public void DispatchLogEntry( IMulticastLogEntry logEntry )
        {
            if( LogEntryReceived != null )
                LogEntryReceived( this, new LogEntryEventArgs( logEntry ) );
        }
    }

}
