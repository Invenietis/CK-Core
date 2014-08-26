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
        Task _receivingTask;
        System.Collections.Concurrent.BlockingCollection<T> _logEntriesCollected;

        readonly IActivityMonitor _monitor;
        readonly UdpClient _client;
        readonly CancellationTokenSource _cancellationTokenSource;
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
            _logEntriesCollected = new System.Collections.Concurrent.BlockingCollection<T>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async void Start( IUdpPacketComposer<T> composer )
        {
            _receivingTask = null;
            ActivityMonitor.DependentToken monitorToken = _monitor.DependentActivity().CreateTokenWithTopic( "ReceivingTask" );
            if( _syncCallback != null )
            {
                _receivingTask = new Task( () =>
                {
                    IActivityMonitor monitor = monitorToken.CreateDependentMonitor();
                    using( monitor.OpenTrace().Send( "Receiving task." ) )
                    {
                        for( ; ; )
                        {

                            if( _logEntriesCollected != null )
                            {
                                T logEntry = _logEntriesCollected.Take( _cancellationTokenSource.Token );
                                monitor.Trace().Send( "Entry received. Calling callback." );
                                try
                                {
                                    _syncCallback( logEntry );
                                    monitor.Trace().Send( "Callback executed successfully." );
                                }
                                catch( Exception ex )
                                {
                                    monitor.Error().Send( ex, "Error during callback execution." );
                                }
                            }
                        }
                    }
                }, _cancellationTokenSource.Token );
            }
            if( _taskCallback != null )
            {
                _receivingTask = new Task( async () =>
                {
                    IActivityMonitor monitor = monitorToken.CreateDependentMonitor();
                    using( monitor.OpenTrace().Send( "Receiving task." ) )
                    {
                        for( ; ; )
                        {

                            if( _logEntriesCollected != null )
                            {
                                T logEntry = _logEntriesCollected.Take( _cancellationTokenSource.Token );
                                monitor.Trace().Send( "Entry received. Calling callback." );
                                try
                                {
                                    await _taskCallback( logEntry );
                                    monitor.Trace().Send( "Callback executed successfully." );
                                }
                                catch( Exception ex )
                                {
                                    monitor.OpenError().Send( ex, "Error during callback execution." );
                                }
                            }
                        }
                    }
                }, _cancellationTokenSource.Token );
            }

            if( _receivingTask == null )
                throw new InvalidOperationException( "There is no receiver callback registered." );

            _receivingTask.Start();


            composer.OnObjectRestored( logEntry =>
            {
                if( _logEntriesCollected != null )
                {
                    lock( _receiveLock )
                    {
                        if( _logEntriesCollected != null )
                        {
                            _logEntriesCollected.Add( logEntry, _cancellationTokenSource.Token );
                        }
                    }
                }
            } );

            while( _shouldReceive )
            {
                try
                {
                    UdpReceiveResult receiveResult = await _client.ReceiveAsync();
                    composer.PushBuffer( receiveResult.Buffer );
                }
                catch( ObjectDisposedException )
                {
                    _monitor.Warn().Send( "The underlying socket has been closed" );
                }

            }
        }

        public void Stop()
        {
            _shouldReceive = false;
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();

            lock( _receiveLock )
            {
                _client.Close();

                _logEntriesCollected.Dispose();
                _logEntriesCollected = null;
            }

        }

        static object _receiveLock = new object();
    }

}
