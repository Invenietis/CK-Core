using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Server
{
    class UdpLogReceiver<T> : ILogReceiver<T>, IDisposable
    {
        ReceivePump<T> _receivePump;

        readonly int _port;
        readonly object _receivePumpSyncRoot;
        readonly IActivityMonitor _monitor;

        readonly IUdpPacketComposer<T> _udpPacketComposer;

        public UdpLogReceiver( IUdpPacketComposer<T> udpPacketComposer, int port, IActivityMonitor monitor = null )
        {
            if( monitor == null ) monitor = new ActivityMonitor( "UDPLogReceiver" );

            _udpPacketComposer = udpPacketComposer;
            _port = port;
            _monitor = monitor;
            _receivePumpSyncRoot = new object();
        }

        public void ReceiveLog( Action<T> onLogEntryReceived )
        {
            if( onLogEntryReceived == null )
                throw new ArgumentNullException( "onLogEntryReceived" );

            OnLogReceived( new ReceivePump<T>( _port, _monitor, onLogEntryReceived ) );
        }

        public void ReceiveLogAsync( Func<T, Task> onLogEntryReceived )
        {
            if( onLogEntryReceived == null )
                throw new ArgumentNullException( "onLogEntryReceived" );

            OnLogReceived( new ReceivePump<T>( _port, _monitor, onLogEntryReceived ) );
        }

        public void Dispose()
        {
            if( _receivePump != null )
            {
                _receivePump.Stop();
                _receivePump.Dispose();
            }
        }

        private void OnLogReceived( ReceivePump<T> pump )
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
                    _receivePump.Start( _udpPacketComposer );
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
