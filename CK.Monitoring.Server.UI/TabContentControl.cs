using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK.Monitoring.Server.UI
{
    public partial class TabContentControl : UserControl
    {
        public TabContentControl()
        {
            InitializeComponent();
        }

        internal void Bind( List<ClientMonitor> list )
        {
            foreach( var m in list )
            {
                TreeNode node = this.ClientMonitorTreeView.Nodes["Monitors"].Nodes[m.MonitorId.ToString()];
                if( node == null )
                {
                    node = new TreeNode( m.MonitorId.ToString() );
                    node.Name = m.MonitorId.ToString();
                    this.ClientMonitorTreeView.Nodes["Monitors"].Nodes.Add( node );
                }

                this.LogView.Text = String.Join( Environment.NewLine, m.Entries.Select( x => x.Text ) );
            }
        }
    }
}
