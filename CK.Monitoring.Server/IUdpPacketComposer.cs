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
        /// Push a dataGram packet received from the UdpClient
        /// </summary>
        /// <param name="dataGram"></param>
        void PushUdpDataGram( byte[] dataGram );

        /// <summary>
        /// Fired when an object has been fully restored from multiple UDP packets
        /// </summary>
        /// <param name="callback"></param>
        void OnObjectRestored( Action<T> callback );
    }
}
