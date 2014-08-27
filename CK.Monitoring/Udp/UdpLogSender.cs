using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Udp
{
    class UdpLogSender : ILogSender
    {
        readonly int _port;
        readonly UdpClient _client;
        readonly UdpPacketSplitter _splitter;

        public UdpLogSender( int port, int maxUdpPacketSize = 1280 )
        {
            _port = port;
            _client = new UdpClient();
            _splitter = new UdpPacketSplitter( maxUdpPacketSize );
        }

        public void Initialize( Core.IActivityMonitor monitor )
        {
            IPEndPoint endPoint = new IPEndPoint( IPAddress.Broadcast, _port );

            monitor.Trace().Send( "Connecting UdpClient to {0}.", endPoint.ToString() );
            _client.Connect( endPoint );
            monitor.Trace().Send( "Connected." );
        }

        public void SendLog( IMulticastLogEntry entry )
        {
            var buffer = PrepareSend( entry );
            foreach( var envelope in _splitter.Split( buffer ) )
            {
                byte[] dataGram = envelope.ToByteArray();
                _client.Send( dataGram, dataGram.Length );
            }
        }

        public async Task SendLogAsync( IMulticastLogEntry entry )
        {
            var buffer = PrepareSend( entry );
            foreach( var envelope in _splitter.Split( buffer ) )
            {
                byte[] dataGram = envelope.ToByteArray();
                await _client.SendAsync( dataGram, dataGram.Length );
            }
        }

        private byte[] PrepareSend( IMulticastLogEntry entry )
        {
            IPEndPoint groupEP = new IPEndPoint( IPAddress.Broadcast, _port );

            using( System.IO.MemoryStream ms = new System.IO.MemoryStream() )
            using( System.IO.BinaryWriter w = new System.IO.BinaryWriter( ms ) )
            {
                entry.WriteLogEntry( w );
                w.Flush();
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            Close( null );
        }

        public void Close( IActivityMonitor monitor )
        {
            try
            {
                _client.Close();
            }
            catch( Exception ex )
            {
                if( monitor != null )
                {
                    monitor.Error().Send( ex );
                }
            }
        }
    }
}
