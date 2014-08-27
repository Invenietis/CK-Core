using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Monitoring.Server
{
    public class ClientApplication
    {
        public ClientApplication( string signature )
        {
            Signature = signature;
            Monitors = new List<ClientMonitor>();
        }

        public string Signature { get; set; }

        public List<ClientMonitor> Monitors { get; set; }

        public void RegisterMonitor( Guid monitorId )
        {
            if( !Monitors.Exists( x => x.MonitorId == monitorId ) )
            {
                Monitors.Add( new ClientMonitor( monitorId ) );
            }
        }
    }
}
