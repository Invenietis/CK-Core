using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Server
{
    class UdpLogReceiver : ILogReceiver, IDisposable
    {
        ReceivePump<IMulticastLogEntry> _receivePump;

        readonly int _port;
        readonly object _receivePumpSyncRoot;
        readonly IActivityMonitor _monitor;

        public UdpLogReceiver( int port, IActivityMonitor monitor = null )
        {
            if( monitor == null ) monitor = new ActivityMonitor( "UDPLogReceiver" );

            _port = port;
            _monitor = monitor;
            _receivePumpSyncRoot = new object();
        }

        public void ReceiveLog( Action<IMulticastLogEntry> onLogEntryReceived )
        {
            if( onLogEntryReceived == null )
                throw new ArgumentNullException( "onLogEntryReceived" );

            OnLogReceived( new ReceivePump<IMulticastLogEntry>( _port, _monitor, onLogEntryReceived ) );
        }

        public void ReceiveLogAsync( Func<IMulticastLogEntry, Task> onLogEntryReceived )
        {
            if( onLogEntryReceived == null )
                throw new ArgumentNullException( "onLogEntryReceived" );

            OnLogReceived( new ReceivePump<IMulticastLogEntry>( _port, _monitor, onLogEntryReceived ) );
        }

        public void Dispose()
        {
            if( _receivePump != null )
            {
                _receivePump.Stop();
                _receivePump.Dispose();
            }
        }

        private void OnLogReceived( ReceivePump<IMulticastLogEntry> pump )
        {
            lock( _receivePumpSyncRoot )
            {
                if( _receivePump != null )
                {
                    throw new InvalidOperationException( "OnLogReceived has already been called" );
                }
                try
                {
                    _receivePump = pump;
                    _receivePump.Start( new MultiCastLogEntryComposer() );
                }
                catch( Exception )
                {
                    _receivePump = null;
                    throw;
                }
            }
        }
    }
}
