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
            clients.CriticalErrors.CollectionChanged += CriticalErrors_CollectionChanged;
            foreach( var appli in clients.Applications )
            {
                IClientApplicationView view =  CreateClientApplicationView( appli );
                view.BindClientApplication( appli );
            }
            var errorView = FindOrCreateCriticalErrorView();
            foreach( var error in clients.CriticalErrors )
            {
                errorView.AddCriticalError( error );
            }
        }

        void CriticalErrors_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach( string error in e.NewItems )
                {
                    FindOrCreateCriticalErrorView().AddCriticalError( error );
                }
            }
        }

        void Applications_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach( ClientApplication appli in e.NewItems )
                {
                    var view = CreateClientApplicationView( appli );
                    view.BindClientApplication( appli );
                }
            }
        }

        private ICriticalErrorView FindOrCreateCriticalErrorView()
        {
            TabPage criticalTabPage = Clients.TabPages["CriticalErrors"];
            if( criticalTabPage == null )
            {
                criticalTabPage = new TabPage( "CriticalErrors" )
                {
                    Name = "CriticalErrors"
                };
                var content = new CriticalErrorControl();
                content.Dock = DockStyle.Fill;
                criticalTabPage.Controls.Add( content );
                Clients.TabPages.Add( criticalTabPage );
            }
            return (ICriticalErrorView)criticalTabPage.Controls[0];
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
