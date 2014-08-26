using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    public interface ILogReceiver : IDisposable
    {
        void ReceiveLog( Action<IMulticastLogEntry> onLogEntryReceived );

        void ReceiveLogAsync( Func<IMulticastLogEntry, Task> onLogEntryReceived );
    }
}
