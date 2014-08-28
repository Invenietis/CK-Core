using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Monitoring.Server
{
    public class ClientMonitor
    {
        static object _lock = new object();

        public ClientMonitor( Guid monitorId )
        {
            MonitorId = monitorId;
            Entries = new ObservableCollection<IMulticastLogEntry>();
        }

        public Guid MonitorId { get; set; }

        public ObservableCollection<IMulticastLogEntry> Entries { get; set; }

        public void AddEntry( IMulticastLogEntry entry )
        {
            // TODO Optim.
            lock( _lock )
            {
                var lastEntry = Entries.FirstOrDefault( x => x.LogTime > entry.LogTime );
                if( lastEntry == null )
                {
                    Entries.Add( entry );
                }
                else
                {
                    Entries.Insert( Entries.IndexOf( lastEntry ), entry );
                }
            }
        }
    }
}
