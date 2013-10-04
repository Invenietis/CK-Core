using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.RouteConfig.Impl
{
    class RouteConfigurationLockShell : IRouteConfigurationLock
    {
        readonly CountdownEvent _lock;
        bool _closed;

        internal RouteConfigurationLockShell( CountdownEvent l )
        {
            _lock = l;
            _closed = true;
        }

        internal void Open()
        {
            Debug.Assert( _closed == true );
            _closed = false;
        }

        public void Lock()
        {
            if( _closed ) throw new InvalidOperationException( "RouteConfigurationLock must be used only when routes are ready." );
            _lock.AddCount();
        }

        public void Unlock()
        {
            if( _closed ) throw new InvalidOperationException( "RouteConfigurationLock must be used only when routes are ready." );
            _lock.Signal();
        }
    }
}
