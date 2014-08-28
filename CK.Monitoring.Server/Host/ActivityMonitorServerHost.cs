using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class ActivityMonitorServerHost : IDisposable
    {
        ILogReceiver<IMulticastLogEntry> _receiver;
        ILogReceiver<string> _criticalReceiver;
        ActivityMonitorServerHostConfiguration _config;

        public ActivityMonitorServerHost( ActivityMonitorServerHostConfiguration config )
        {
            _config = config;
            _receiver = new UdpLogReceiver<IMulticastLogEntry>( new MultiCastLogEntryComposer(), _config.Port );
            _criticalReceiver = new UdpLogReceiver<string>( new CriticalErrorComposer(), _config.CrititcalErrorPort );
        }

        public void Open( Action<IMulticastLogEntry> onLogEntryReceived, Action<string> onCriticalLogReceived = null )
        {
            _receiver.ReceiveLog( onLogEntryReceived );
            if( onCriticalLogReceived != null )
            {
                _criticalReceiver.ReceiveLog( onCriticalLogReceived );
            }
        }

        public void OpenAsync( Func<IMulticastLogEntry, Task> onLogEntryReceived, Func<string, Task> onCriticalLogReceived = null )
        {
            _receiver.ReceiveLogAsync( onLogEntryReceived );
            if( onCriticalLogReceived != null )
            {
                _criticalReceiver.ReceiveLogAsync( onCriticalLogReceived );
            }
        }

        public void Dispose()
        {
            _receiver.Dispose();
            _receiver = null;
        }
    }
}
