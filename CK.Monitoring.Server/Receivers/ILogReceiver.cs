using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public interface ILogReceiver<T> : IDisposable
    {
        void ReceiveLog( Action<T> onLogEntryReceived );

        void ReceiveLogAsync( Func<T, Task> onLogEntryReceived );
    }
}
