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
            composer.OnObjectRestored( async logEntry =>
            {
                try
                {
                    if( _syncCallback != null ) _syncCallback( logEntry );
                    if( _taskCallback != null ) await _taskCallback( logEntry );
                }
                catch( Exception ex )
                {
                    _monitor.Error().Send( ex, "Error during callback execution." );
                }
            } );

            using( _monitor.OpenInfo().Send( "Start receiver loop." ).ConcludeWith( () => "Receiver loop stopped" ) )
            {
                while( _shouldReceive )
                {
                    try
                    {
                        UdpReceiveResult receiveResult = await _client.ReceiveAsync();
                        _monitor.Trace().Send( "Received {0} bytes", receiveResult.Buffer.Length );

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
