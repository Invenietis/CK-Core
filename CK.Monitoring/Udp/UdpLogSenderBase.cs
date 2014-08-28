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
    abstract class UdpLogSenderBase<T> : ILogSender<T>
    {
        readonly int _port;
        readonly UdpClient _client;
        readonly UdpPacketSplitter _splitter;

        public UdpLogSenderBase( int port, int maxUdpPacketSize )
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

        public virtual void SendLog( T entry )
        {
            var buffer = PrepareSend( entry );
            foreach( var envelope in _splitter.Split( buffer ) )
            {
                byte[] dataGram = envelope.ToByteArray();
                _client.Send( dataGram, dataGram.Length );
            }
        }

        public virtual async Task SendLogAsync( T entry )
        {
            var buffer = PrepareSend( entry );
            foreach( var envelope in _splitter.Split( buffer ) )
            {
                byte[] dataGram = envelope.ToByteArray();
                await _client.SendAsync( dataGram, dataGram.Length );
            }
        }

        protected abstract byte[] PrepareSend( T entry );

        public void Dispose()
        {
            Close( null );
        }

        public virtual void Close( IActivityMonitor monitor )
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
