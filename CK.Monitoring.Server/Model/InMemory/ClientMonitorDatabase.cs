using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Server
{
    public class ClientMonitorDatabase
    {
        LogEntryDispatcher _dispatcher;
        ObservableCollection<ClientApplication> _applications;
        ObservableCollection<string> _errors;

        public ObservableCollection<ClientApplication> Applications
        {
            get { return _applications; }
        }

        public ObservableCollection<string> CriticalErrors
        {
            get { return _errors; }
        }

        public ClientMonitorDatabase( LogEntryDispatcher dispatcher )
        {
            _applications = new ObservableCollection<ClientApplication>();
            _errors = new ObservableCollection<string>();

            _dispatcher = dispatcher;
            _dispatcher.LogEntryReceived += OnLogEntryReceived;
            _dispatcher.CriticalErrorReceived += OnCriticalErrorReceived;
        }

        void OnCriticalErrorReceived( object sender, CriticalErrorEventArgs e )
        {
            _errors.Add( e.Error );
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
            var app = _applications.FirstOrDefault( x => x.Signature == signature );
            if( app == null )
            {
                app = new ClientApplication( signature );
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
