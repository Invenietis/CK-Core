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
    public partial class CriticalErrorControl : UserControl, ICriticalErrorView
    {
        public CriticalErrorControl()
        {
            InitializeComponent();
        }

        public void AddCriticalError( string error )
        {
            this.ErrorText.Text += error + Environment.NewLine;
        }
    }
}
