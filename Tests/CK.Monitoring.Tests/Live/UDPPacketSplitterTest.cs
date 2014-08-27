using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.Server;
using CK.Monitoring.Udp;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    [TestFixture( Category = "ActivityMonitor.Live" )]
    public class UDPPacketSplitterTest
    {
        IMulticastLogEntry CreateLog( string text )
        {
            IMulticastLogEntry e = LogEntry.CreateMulticastLog( Guid.NewGuid(), LogEntryType.Line, DateTimeStamp.UtcNow, 0, text, DateTimeStamp.UtcNow, LogLevel.Info, @"D:\Invenietis\Dev\Github\ck-core\Tests\CK.Monitoring.Tests\Live\UDPPacketSplitterTest.cs", 16, null, CKExceptionData.CreateFrom( ThrowAggregatedException() ) );
            return e;
        }

        [Test]
        public void SplitLogEntryTest()
        {
            int MaxUdpPacketSize = 1280;

            var log = CreateLog( "I'm a simple log entry. I'm a simple log entry. I'm a simple log entry. I'm a simple log entry. I'm a simple log entry. I'm a simple log entry." );

            UdpPacketSplitter splitter = new UdpPacketSplitter( MaxUdpPacketSize );

            using( var ms = new MemoryStream() )
            {
                using( var bw = new BinaryWriter( ms, Encoding.UTF8, true ) ) log.WriteLogEntry( bw );

                UdpPacketEnvelope[] envelopes = splitter.Split( ms.ToArray() ).ToArray();
                Assert.That( envelopes.Length > 1 );
                Assert.That( envelopes[0].CorrelationId == envelopes[1].CorrelationId );
                Assert.That( envelopes[0].SequenceNumber < envelopes[1].SequenceNumber );
                Assert.That( envelopes[0].Count == envelopes[1].Count );
                Assert.That( envelopes[0].Count == envelopes.Length );

                Assert.That( envelopes[0].SequenceNumber == 0 );
                Assert.That( envelopes[1].SequenceNumber == 1 );

                foreach( var e in envelopes )
                {
                    byte[] buffer = e.ToByteArray();
                    UdpPacketEnvelope ee = UdpPacketEnvelope.FromByteArray( buffer );
                    Assert.That( e.CorrelationId, Is.EqualTo( ee.CorrelationId ) );
                    Assert.That( e.SequenceNumber, Is.EqualTo( ee.SequenceNumber ) );
                    Assert.That( e.Payload, Is.EqualTo( ee.Payload ) );
                }

                bool logFullyComposed = false;
                IUdpPacketComposer<IMulticastLogEntry> composer = new MultiCastLogEntryComposer();
                composer.OnObjectRestored( entry =>
                {
                    logFullyComposed = true;
                    Assert.That( entry.FileName, Is.EqualTo( log.FileName ) );
                    Assert.That( entry.Text, Is.EqualTo( log.Text ) );
                } );

                foreach( var e in envelopes )
                {
                    composer.PushUdpDataGram( e.ToByteArray() );
                }

                Assert.That( logFullyComposed, Is.True );
            }
        }


        internal static AggregateException ThrowAggregatedException()
        {
            AggregateException eAgg = null;
            try
            {
                Parallel.For( 0, 50, i =>
                {
                    if( i % 1 == 0 ) throw new Exception( String.Format( "Ex n°{0}", i ), ThrowExceptionWithInner() );
                    else throw new Exception( String.Format( "Ex n°{0}", i ) );
                } );
            }
            catch( AggregateException ex )
            {
                eAgg = ex;
            }
            return eAgg;
        }

        internal static Exception ThrowExceptionWithInner()
        {
            Exception e;
            try
            {
                throw new Exception( "Outer", ThrowSimpleException( "Inner" ) );
            }
            catch( Exception ex )
            {
                e = ex;
            }
            return e;
        }

        internal static Exception ThrowSimpleException( string message )
        {
            Exception e;
            try { throw new Exception( message ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

    }
}
