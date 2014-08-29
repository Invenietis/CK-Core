using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class LogEntryDispatcher
    {
        /// <summary>
        /// This event is fired when a log entry has been received by <see cref="DispatchLogEntry"/> 
        /// </summary>
        public event EventHandler<LogEntryEventArgs> LogEntryReceived;

        /// <summary>
        /// This event is fired when a critical error has been received by <see cref="DispatchCriticalError"/>
        /// </summary>
        public event EventHandler<CriticalErrorEventArgs> CriticalErrorReceived;

        /// <summary>
        /// Dispatch a log entry firing <see cref="LogEntryReceived"/>.
        /// </summary>
        /// <param name="logEntry">A <see cref="IMulticastLogEntry"/></param>
        public void DispatchLogEntry( IMulticastLogEntry logEntry )
        {
            if( LogEntryReceived != null )
                LogEntryReceived( this, new LogEntryEventArgs( logEntry ) );
        }

        /// <summary>
        /// Dispatch a critical error firing a <see cref="CriticalErrorReceived"/>
        /// </summary>
        /// <param name="error">The critical error</param>
        public void DispatchCriticalError( string error )
        {
            if( CriticalErrorReceived != null )
                CriticalErrorReceived( this, new CriticalErrorEventArgs( error ) );
        }
    }

}
