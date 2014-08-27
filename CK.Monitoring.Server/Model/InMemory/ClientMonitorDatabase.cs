using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Server
{
    public class ClientMonitorDatabase
    {
        LogEntryDispatcher _dispatcher;
        List<ClientApplication> _applications;

        public IReadOnlyCollection<ClientApplication> Applications
        {
            get { return _applications; }
        }

        public ClientMonitorDatabase( LogEntryDispatcher dispatcher )
        {
            _applications = new List<ClientApplication>();

            _dispatcher = dispatcher;
            _dispatcher.LogEntryReceived += OnLogEntryReceived;
        }

        void OnLogEntryReceived( object sender, LogEntryEventArgs e )
        {
            var entry = e.LogEntry;
            if( entry.Tags.Overlaps( ActivityMonitor.Tags.ApplicationSignature ) )
            {
                AddApplication( entry.Text, entry.MonitorId );
            }
            AddLog( entry );
        }

        void AddApplication( string signature, Guid monitorId )
        {
            var app = _applications.Find( x => x.Signature == signature );
            if( app == null )
            {
                app =  new ClientApplication( signature );
                _applications.Add( app );
            }
            app.RegisterMonitor( monitorId );
        }

        void AddLog( IMulticastLogEntry entry )
        {
            ClientMonitor monitor = _applications
                .SelectMany( x => x.Monitors )
                .FirstOrDefault( x => x.MonitorId == entry.MonitorId );

            if( monitor != null )
            {
                monitor.AddEntry( entry );
            }
        }
    }

}
