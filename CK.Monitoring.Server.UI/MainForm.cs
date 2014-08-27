using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK.Monitoring.Server.UI
{
    public partial class MainForm : Form
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            ActivityMonitorServerHostConfiguration config = new ActivityMonitorServerHostConfiguration
            {
                Port = 3712
            };

            LogEntryDispatcher dispatcher = new LogEntryDispatcher();
            ClientMonitorDatabase database = new ClientMonitorDatabase( dispatcher );

            dispatcher.LogEntryReceived += ( sender, entry ) =>
            {
                var appli = database.Applications.FirstOrDefault( x => x.Signature == Environment.MachineName );
                if( appli != null )
                {
                    TabPage page = this.Clients.TabPages[appli.Signature];
                    if( page == null )
                    {
                        page = new TabPage( appli.Signature );
                        page.Name = appli.Signature;
                        TabContentControl content = new TabContentControl();
                        content.Name = "ClientMonitorPanel" + appli.Signature;
                        page.Controls.Add( content );
                        this.Clients.TabPages.Add( page );
                    }
                    else
                    {
                        TabContentControl control = (TabContentControl)page.Controls["ClientMonitorPanel" + appli.Signature];
                        control.Bind( appli.Monitors );
                    }
                }
            };
            ActivityMonitorServerHost server = new ActivityMonitorServerHost( config );
            server.Open( dispatcher.DispatchLogEntry );
        }

        public MainForm()
        {
            InitializeComponent();
        }
    }
}
