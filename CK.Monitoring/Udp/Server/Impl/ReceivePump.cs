using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Udp
{
    class ReceivePump<T> : IDisposable
    {
        bool _shouldReceive;

        readonly IActivityMonitor _monitor;
        readonly UdpClient _client;
        readonly Action<T> _syncCallback;
        readonly Func<T, Task> _taskCallback;


        public ReceivePump( int port, IActivityMonitor monitor, Action<T> syncCallback )
            : this( port, monitor )
        {
            _syncCallback = syncCallback;
        }

        public ReceivePump( int port, IActivityMonitor monitor, Func<T, Task> taskCallback )
            : this( port, monitor )
        {
            _taskCallback = taskCallback;
        }

        public ReceivePump( int port, IActivityMonitor monitor )
        {
            _monitor = monitor;
            _shouldReceive = true;
            _client = new UdpClient( port );
        }

        public async void Start( IUdpPacketComposer<T> composer )
        {
            ActivityMonitor.DependentToken monitorToken = _monitor.DependentActivity().CreateToken();

            composer.OnObjectRestored( logEntry =>
            {
                var monitor = monitorToken.CreateDependentMonitor();
                try
                {
                    if( _syncCallback != null ) _syncCallback( logEntry );
                    if( _taskCallback != null ) _taskCallback( logEntry );
                }
                catch( Exception ex )
                {
                    monitor.Error().Send( ex, "Error during callback execution." );
                }
            } );

            while( _shouldReceive )
            {
                try
                {
                    UdpReceiveResult receiveResult = await _client.ReceiveAsync();
                    composer.PushUdpDataGram( receiveResult.Buffer );
                }
                catch( ObjectDisposedException )
                {
                    _monitor.Warn().Send( "The underlying socket has been closed" );
                }
                catch( SocketException se )
                {
                    _monitor.Error().Send( se );
                }
            }
        }

        public void Stop()
        {
            _shouldReceive = false;
        }

        public void Dispose()
        {
            Stop();

            lock( _receiveLock )
            {
                _client.Close();
            }

        }

        static object _receiveLock = new object();
    }

}
