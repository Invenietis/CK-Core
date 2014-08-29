using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Udp
{
    public interface ILogSender<T> : IDisposable
    {
        /// <summary>
        /// Sends a log
        /// </summary>
        /// <param name="entry"></param>
        void SendLog( T entry );

        /// <summary>
        /// Sends a log asynchronous
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Task SendLogAsync( T entry );
    }
}
