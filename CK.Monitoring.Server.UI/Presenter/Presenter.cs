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
            _database.Applications.CollectionChanged += Applications_CollectionChanged;
            _database.CriticalErrors.CollectionChanged += CriticalErrors_CollectionChanged;
            
            foreach( var appli in _database.Applications )
            {
                _appView.BindClientApplication( appli );
            }
            foreach( var error in _database.CriticalErrors )
            {
                _appView.BindCriticalError( error );
            }
        }


        void CriticalErrors_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach( string error in e.NewItems )
                {
                    _appView.BindCriticalError( error );
                }
            }
        }

        void Applications_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach( ClientApplication appli in e.NewItems )
                {
                    _appView.BindClientApplication( appli );
                }
            }
        }
    }
}
