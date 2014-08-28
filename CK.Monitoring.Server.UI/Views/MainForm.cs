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
    public partial class MainForm : Form, IMainView
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void BindClients( ClientMonitorDatabase clients )
        {
            clients.Applications.CollectionChanged += Applications_CollectionChanged;

            foreach( var appli in clients.Applications )
            {
                IClientApplicationView view =  CreateClientApplicationView( appli );
                view.BindClientApplication( appli );
            }
        }

        void Applications_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach( ClientApplication appli in e.NewItems )
                {
                    var view = CreateClientApplicationView( appli );
                    view.BindClientApplication( appli);
                }
            }
        }


        private IClientApplicationView CreateClientApplicationView( ClientApplication appli )
        {
            var page = new TabPage( appli.Signature );
            page.Name = appli.Signature;

            TabContentControl content = new TabContentControl();
            content.Dock = DockStyle.Fill;
            page.Controls.Add( content );

            Clients.TabPages.Add( page );
            return content;
        }

    }
}
