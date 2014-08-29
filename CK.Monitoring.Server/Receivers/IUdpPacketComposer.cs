using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public interface IUdpPacketComposer<T>
    {
        /// <summary>
        /// Push a dataGram packet received from the UdpClient. This method is thread-safe.
        /// </summary>
        /// <param name="dataGram"></param>
        void PushUdpDataGram( byte[] dataGram );

        /// <summary>
        /// Fired when an object has been fully restored from multiple UDP packets.
        /// Executes on the caller thread.
        /// </summary>
        /// <param name="callback"></param>
        void OnObjectRestored( Action<T> callback );
    }
}
