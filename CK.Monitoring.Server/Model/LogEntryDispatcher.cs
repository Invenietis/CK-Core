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

        public event EventHandler<CriticalErrorEventArgs> CriticalErrorReceived;

        public void DispatchLogEntry( IMulticastLogEntry logEntry )
        {
            if( LogEntryReceived != null )
                LogEntryReceived( this, new LogEntryEventArgs( logEntry ) );
        }

        public void DispatchCriticalError( string error )
        {
            if( CriticalErrorReceived != null )
                CriticalErrorReceived( this, new CriticalErrorEventArgs( error ) );
        }
    }

}
