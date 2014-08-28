using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CK.Monitoring.Server.UI
{
    public class Presenter
    {
        readonly IMainView _appView;
        readonly ClientMonitorDatabase _database;

        public Presenter( IMainView applicationView, ClientMonitorDatabase database )
        {
            _appView = applicationView;
            _database = database;
        }

        public void Start()
        {
            _appView.BindClients( _database );
        }
    }
}
