using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Monitoring.Server
{
    public class ClientMonitor
    {
        static object _lock = new object();

        public ClientMonitor( Guid monitorId )
        {
            MonitorId = monitorId;
            Entries = new List<IMulticastLogEntry>();
        }

        public Guid MonitorId { get; set; }

        public List<IMulticastLogEntry> Entries { get; set; }

        public void AddEntry( IMulticastLogEntry entry )
        {
            // TODO Optim.
            lock( _lock )
            {
                Entries.Add( entry );
                Entries = Entries.OrderBy( x => x.LogTime ).ToList();
            }
        }
    }
}
