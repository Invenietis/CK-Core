using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    public interface IUdpPacketComposer<T>
    {
        /// <summary>
        /// Push a dataGram packet received from the UdpClient
        /// </summary>
        /// <param name="dataGram"></param>
        void PushBuffer( byte[] dataGram );

        /// <summary>
        /// Fired when a packet has been fully restored
        /// </summary>
        /// <param name="callback"></param>
        void OnObjectRestored( Action<T> callback );
    }
}
