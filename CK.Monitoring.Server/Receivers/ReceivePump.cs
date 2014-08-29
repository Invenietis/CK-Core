using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Server
{
    class ReceivePump<T> : IDisposable
    {
        bool _shouldReceive;
        Thread _thread;

        readonly IActivityMonitor _monitor;
        readonly UdpClient _client;
        readonly Action<T> _syncCallback;
        readonly Func<T, Task> _taskCallback;

        readonly CancellationTokenSource _cancellationToken;

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

        public void Start( IUdpPacketComposer<T> composer )
        {
            var monitorReceiverToken = _monitor.DependentActivity().CreateTokenWithTopic( "ReceivePump Receiver Thread" );

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

            _thread = new Thread( async () =>
            {
                var m = monitorReceiverToken.CreateDependentMonitor();
                using( m.OpenInfo().Send( "Start receiver loop." ).ConcludeWith( () => "Receiver loop stopped" ) )
                {
                    while( _shouldReceive )
                    {
                        try
                        {
                            UdpReceiveResult result = await _client.ReceiveAsync();
                            m.Trace().Send( "Received {0} bytes", result.Buffer.Length );

                            composer.PushUdpDataGram( result.Buffer );
                        }
                        catch( ObjectDisposedException )
                        {
                            m.Warn().Send( "The underlying socket has been closed" );
                        }
                        catch( SocketException se )
                        {
                            m.Error().Send( se );
                        }
                    }
                }
            } );
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.Normal;
            _thread.Start();
        }

        public void Stop()
        {
            _shouldReceive = false;
        }

        public void Dispose()
        {
            Stop();

            _client.Close();
            if( _thread != null )
            {
                _thread.Join();
                GC.SuppressFinalize( this );
            }
        }

        ~ReceivePump()
        {
            if( _thread != null )
            {
                _thread.Abort();
            }
        }

    }

}
