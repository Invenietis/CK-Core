using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CK.Monitoring.Server
{
    public class ClientApplicationViewModel
    {
        public ClientApplicationViewModel( string signature )
        {
            Signature = signature;
            Monitors = new ObservableCollection<ClientMonitorViewModel>();
        }

        public string Signature { get; set; }

        public ObservableCollection<ClientMonitorViewModel> Monitors { get; set; }

        public void RegisterMonitor( Guid monitorId )
        {
            if( !Monitors.Any( x => x.MonitorId == monitorId ) )
            {
                Monitors.Add( new ClientMonitorViewModel( monitorId ) );
            }
        }
    }
}
