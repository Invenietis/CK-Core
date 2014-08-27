using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class ActivityMonitorServerHost : IDisposable
    {
        ILogReceiver _receiver;
        ActivityMonitorServerHostConfiguration _config;

        public ActivityMonitorServerHost( ActivityMonitorServerHostConfiguration config )
        {
            _config = config;
            _receiver = new UdpLogReceiver( _config.Port );
        }

        public void Open( Action<IMulticastLogEntry> onLogEntryReceived )
        {
            _receiver.ReceiveLog( onLogEntryReceived );
        }

        public void OpenAsync( Func<IMulticastLogEntry, Task> onLogEntryReceived )
        {
            _receiver.ReceiveLogAsync( onLogEntryReceived );
        }

        public void Dispose()
        {
            _receiver.Dispose();
            _receiver = null;
        }
    }
}
