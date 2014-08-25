using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.Core;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using CK.Monitoring.GrandOutputHandlers.UDP;

namespace CK.Monitoring.Tests.Live
{
    [TestFixture( Category = "ActivityMonitor.Live" )]
    public class UDPSenderReceiverTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Test]
        public void UDPLogSender_SendsLogEntryAsString_Through_A_Specific_Port()
        {
            AutoResetEvent e = new AutoResetEvent( false );
            using( ILogReceiver receiver = new UDPLogReceiver( 3712 ) )
            {
                receiver.ReceiveLog( ( logEntry ) =>
                {
                    Assert.That( logEntry.Text, Is.EqualTo( "This is a log entry" ) );
                    e.Set();
                } );

                using( ILogSender sender = new UDPLogSender( 3712 ) )
                {
                    sender.Initialize( new ActivityMonitor() );
                    sender.SendLog( "This is a log entry" );
                }

                e.WaitOne();
            }
        }

        [Test]
        public void UDPLogReceiver_ExceptionDuring_CallBack_Should_Not_Interrupt_The_WholeProcess()
        {
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
            GrandOutput.EnsureActiveDefaultWithDefaultSettings();
            IActivityMonitor monitor = new ActivityMonitor();

            bool secondMessageReceived = false;
            AutoResetEvent e = new AutoResetEvent( false );
            using( ILogReceiver receiver = new UDPLogReceiver( 3712, monitor ) )
            {
                receiver.ReceiveLog( ( logEntry ) =>
                {
                    if( logEntry.Text == "This is a log entry" )
                    {
                        throw new ApplicationException( "This is a manual triggered exception" );
                    }
                    else
                    {
                        secondMessageReceived = true;
                        e.Set();
                    }
                } );

                using( ILogSender sender = new UDPLogSender( 3712 ) )
                {
                    sender.Initialize( monitor );
                    sender.SendLog( "This is a log entry" );
                    sender.SendLog( "This is a log entry with no exception." );
                }

                e.WaitOne( TimeSpan.FromSeconds( 2 ) );
                Assert.That( secondMessageReceived );
            }
        }

        [Test]
        [TestCase( 1000 )]
        [TestCase( 10000 )]
        public void UDPLogSender_Sends_MultipleEntries_ReadAllEntries( int entries )
        {
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
            GrandOutput.EnsureActiveDefaultWithDefaultSettings();

            IActivityMonitor monitor = new ActivityMonitor();

            AutoResetEvent e = new AutoResetEvent( false );
            using( ILogReceiver receiver = new UDPLogReceiver( 3712, monitor ) )
            {
                Stopwatch receiverWatch = new Stopwatch();
                receiverWatch.Start();
                receiver.ReceiveLog( ( logEntry ) =>
                {
                    string textEntry = logEntry.Text;
                    monitor.Trace().Send( textEntry );

                    string part = "This is log entry n°";
                    StringAssert.StartsWith( part, textEntry );

                    string subString = textEntry.Remove( 0, part.Length );

                    int logEntryInc = Int32.Parse( subString );
                    if( logEntryInc == entries )
                    {
                        e.Set();
                    }
                } );

                using( ILogSender sender = new UDPLogSender( 3712 ) )
                {
                    sender.Initialize( monitor );
                    Stopwatch senderWatch = new Stopwatch();
                    senderWatch.Start();
                    for( int i = 1; i <= entries; ++i )
                    {
                        sender.SendLog( String.Format( "This is log entry n°{0}", i ) );
                    }
                    senderWatch.Stop();
                    Console.WriteLine( "Send {0} log entries in {1}", entries, senderWatch.Elapsed );
                }

                e.WaitOne();
                receiverWatch.Stop();
                Console.WriteLine( "Receive {0} log entries in {1}", entries, receiverWatch.Elapsed );

            }
        }

        public interface ILogReceiver : IDisposable
        {
            void ReceiveLog( Action<IMulticastLogEntry> onLogEntryReceived );

            void ReceiveLogAsync( Func<IMulticastLogEntry, Task> onLogEntryReceived );
        }

        class UDPLogReceiver : ILogReceiver, IDisposable
        {
            ReceivePump<IMulticastLogEntry> _receivePump;

            readonly int _port;
            readonly object _receivePumpSyncRoot;
            readonly IActivityMonitor _monitor;

            public UDPLogReceiver( int port, IActivityMonitor monitor = null )
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
                        _receivePump.Start( new DummyUdpPackageComposer() );
                    }
                    catch( Exception )
                    {
                        _receivePump = null;
                        throw;
                    }
                }
            }
        }

        class DummyUdpPackageComposer : IUdpPacketComposer<IMulticastLogEntry>
        {
            Action<IMulticastLogEntry> _callback;

            public void PushBuffer( byte[] dataGram )
            {
                using( MemoryStream ms = new MemoryStream( dataGram ) )
                using( BinaryReader reader = new BinaryReader( ms ) )
                {
                    bool badEOF;
                    IMulticastLogEntry entry = (IMulticastLogEntry)LogEntry.Read( reader, 0, out badEOF );
                    _callback( entry );
                }
            }

            public void OnObjectRestored( Action<IMulticastLogEntry> callback )
            {
                if( callback == null ) throw new ArgumentNullException( "callback" );
                _callback = callback;
            }
        }

        class ReceivePump<T> : IDisposable
        {
            bool _shouldReceive;
            Task _receivingTask;
            System.Collections.Concurrent.BlockingCollection<T> _logEntriesCollected;

            readonly  IActivityMonitor _monitor;
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

        public interface IUdpPacketComposer<T>
        {
            void PushBuffer( byte[] dataGram );

            void OnObjectRestored( Action<T> callback );
        }
    }

    public static class LogSenderExtension
    {
        public static void SendLog( this ILogSender sender, string logEntry )
        {
            var e = LogEntry.CreateMulticastLog( Guid.NewGuid(), LogEntryType.Line, DateTimeStamp.UtcNow, 0, logEntry, DateTimeStamp.UtcNow, LogLevel.Info, "", 0, null, null );
            sender.SendLog( e );
        }
    }

}
