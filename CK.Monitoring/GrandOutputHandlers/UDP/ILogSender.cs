using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers.UDP
{
    public interface ILogSender : IDisposable
    {
        /// <summary>
        /// Sends a log
        /// </summary>
        /// <param name="entry"></param>
        void SendLog( IMulticastLogEntry entry );

        /// <summary>
        /// Sends a log asynchronous
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Task SendLogAsync( IMulticastLogEntry entry );

        /// <summary>
        /// Initializes this log sender
        /// </summary>
        /// <param name="monitor">The monitor used during initialization phasis</param>
        void Initialize( IActivityMonitor monitor );

        /// <summary>
        /// Closes this log sender
        /// </summary>
        /// <param name="monitor">The monitor used during the close phasis</param>
        void Close( IActivityMonitor monitor );
    }

}
