using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CK.Core;
using CK.Monitoring.Server.Index;

namespace CK.Monitoring.Server.UI
{
    static class Program
    {
        static ActivityMonitorServerHost _server;
        static Index.LogIndexer _indexer;
        static Timer _uiRefreshTicker;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // GrandOutput configuration for the Application.
            string applicationPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "CK.Monitoring.Server" );
            SystemActivityMonitor.RootLogPath = Path.Combine( applicationPath, "RootLogPath" );

            if( Directory.Exists( SystemActivityMonitor.RootLogPath ) ) Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );

            GrandOutput.EnsureActiveDefaultWithDefaultSettings();

            // Indexer initialization.
            var indexFactory = new Index.IndexStoreFactory( Path.Combine( applicationPath, "Index" ) );
            LogEntryDispatcher dispatcher = new LogEntryDispatcher();

            _indexer = new Index.LogIndexer( dispatcher, indexFactory );

            // Server configuration
            _server = new ActivityMonitorServerHost( new ActivityMonitorServerHostConfiguration
            {
                Port = 3712,
                CrititcalErrorPort = 3713
            } );
            _server.Open( dispatcher.DispatchLogEntry, dispatcher.DispatchCriticalError );

            // UI configuration

            LogEntryDispatcher uiDispatcher = new LogEntryDispatcher();
            LogSearcher searcher = new LogSearcher( indexFactory );

            _uiRefreshTicker = new Timer();
            _uiRefreshTicker.Interval = 2000;
            _uiRefreshTicker.Tick += ( sender, e ) =>
            {
                var entries = searcher.Search( "*", TimeSpan.FromSeconds( _uiRefreshTicker.Interval ) );
                foreach( var entry in entries )
                {
                    uiDispatcher.DispatchLogEntry( entry );
                }
            };

            MainForm mainView = new MainForm();
            Presenter presenter = new Presenter( mainView, new ClientMonitorViewModelRoot( uiDispatcher ) );
            presenter.Start();

            _uiRefreshTicker.Start();
            // Run the application
            Application.ApplicationExit += OnApplicationExit;
            Application.Run( mainView );
        }


        private static void OnApplicationExit( object sender, EventArgs e )
        {
            _uiRefreshTicker.Stop();
            _uiRefreshTicker.Dispose();
            _indexer.Dispose();
            _server.Dispose();
        }
    }
}
