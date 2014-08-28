using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK.Monitoring.Server.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            MainForm mainView = new MainForm();

            ActivityMonitorServerHostConfiguration config = new ActivityMonitorServerHostConfiguration
            {
                Port = 3712,
                CrititcalErrorPort = 3713
            };

            LogEntryDispatcher dispatcher = new LogEntryDispatcher();

            ClientMonitorDatabase database = new ClientMonitorDatabase( dispatcher );
            Presenter presenter = new Presenter( mainView, database );
            presenter.Start();

            ActivityMonitorServerHost server = new ActivityMonitorServerHost( config );
            server.Open( dispatcher.DispatchLogEntry, dispatcher.DispatchCriticalError );

            Application.Run( mainView );
        }
    }
}
