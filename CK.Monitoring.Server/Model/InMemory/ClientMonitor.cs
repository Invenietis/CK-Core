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
        public ClientMonitor( Guid monitorId )
        {
            MonitorId = monitorId;
            Entries = new ObservableCollection<ClientLogEntry>();
        }

        public Guid MonitorId { get; set; }

        public ObservableCollection<ClientLogEntry> Entries { get; set; }

        public void AddEntry( IMulticastLogEntry entry )
        {
            // TODO Optim.
            lock( this )
            {
                var clientLogEntry = new ClientLogEntry( entry );
                var lastEntry = Entries.Where( x => x.IsMissingEntry == false ).FirstOrDefault( x => x.LogEntry.LogTime > entry.LogTime );
                if( lastEntry == null )
                {
                    Entries.Add( clientLogEntry );
                }
                else
                {
                    Entries.Insert( Entries.IndexOf( lastEntry ), clientLogEntry );
                }

                var previousEntry = Entries.Where( x => x.IsMissingEntry == false ).FirstOrDefault( x => x.LogEntry.LogTime == entry.PreviousLogTime );
                if( previousEntry == null )
                {
                    Entries.Insert( Entries.IndexOf( clientLogEntry ), ClientLogEntry.Missing );
                }
            }
        }
    }
}
