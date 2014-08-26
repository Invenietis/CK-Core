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
using CK.Monitoring.Udp;

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
            using( ILogReceiver receiver = new UdpLogReceiver( 3712 ) )
            {
                receiver.ReceiveLog( ( logEntry ) =>
                {
                    Assert.That( logEntry.Text, Is.EqualTo( "This is a log entry" ) );
                    e.Set();
                } );

                using( ILogSender sender = new UdpLogSender( 3712 ) )
                {
                    sender.Initialize( new ActivityMonitor() );
                    sender.SendLog( "This is a log entry" );
                }

                Assert.That( e.WaitOne( 1000 ), Is.True );
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
            using( ILogReceiver receiver = new UdpLogReceiver( 3712, monitor ) )
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

                using( ILogSender sender = new UdpLogSender( 3712 ) )
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
        public void UDPLogSender_Sends_MultipleEntries_Receiver_ReadAllEntries( int entries )
        {
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
            GrandOutput.EnsureActiveDefaultWithDefaultSettings();

            IActivityMonitor monitor = new ActivityMonitor();

            AutoResetEvent e = new AutoResetEvent( false );
            using( ILogReceiver receiver = new UdpLogReceiver( 3712, monitor ) )
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

                using( ILogSender sender = new UdpLogSender( 3712 ) )
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

                Assert.That( e.WaitOne( 15000 ) );

                receiverWatch.Stop();
                Console.WriteLine( "Receive {0} log entries in {1}", entries, receiverWatch.Elapsed );

            }
        }

    }

    public static class LogSenderExtension
    {
        static CKExceptionData exception;
        static LogSenderExtension()
        {
            exception = CKExceptionData.CreateFrom( UDPPacketSplitterTest.ThrowAggregatedException() );
        }

        public static void SendLog( this ILogSender sender, string logEntry )
        {
            var e = LogEntry.CreateMulticastLog( Guid.NewGuid(), LogEntryType.Line, DateTimeStamp.UtcNow, 0, logEntry, DateTimeStamp.UtcNow, LogLevel.Info, "", 0, null, exception );
            sender.SendLog( e );
        }
    }

}
