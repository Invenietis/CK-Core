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

        public delegate void InvokeDelegate();

        public void BindCriticalError( string error )
        {
            BeginInvoke( new InvokeDelegate( () =>
            {
                FindOrCreateCriticalErrorView().AddCriticalError( error );
            } ) );
        }

        public void BindClientApplication( ClientApplicationViewModel appli )
        {
            BeginInvoke( new InvokeDelegate( () =>
            {
                IClientApplicationView view =  CreateClientApplicationView( appli );
                view.BindClientApplication( appli );
            } ) );
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

        private IClientApplicationView CreateClientApplicationView( ClientApplicationViewModel appli )
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
