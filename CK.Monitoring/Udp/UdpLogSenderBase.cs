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
        readonly UdpClient _client;
        readonly UdpPacketSplitter _splitter;
        readonly IActivityMonitor _monitor;

        public UdpLogSenderBase( string serverAddress, int port, int maxUdpPacketSize = 1280, IActivityMonitor monitor = null )
        {
            if( String.IsNullOrEmpty( serverAddress ) ) throw new ArgumentNullException( "serverAddress" );

            _monitor = monitor ?? new ActivityMonitor();
            _client = new UdpClient();
            _splitter = new UdpPacketSplitter( maxUdpPacketSize );

            IPAddress address;
            if( !IPAddress.TryParse( serverAddress, out address ) )
            {
                _monitor.Error().Send( "The IPAddress: {0} is not valid... Fallback to Loopback address." );
                address = IPAddress.Loopback;
            }
            else
            {
                IPEndPoint endPoint = new IPEndPoint( address, port );

                _monitor.Trace().Send( "Connecting UdpClient to IPAddress: {0}.", endPoint.ToString() );
                _client.Connect( endPoint );
                _monitor.Trace().Send( "Connected." );
            }
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
            try
            {
                _client.Close();
            }
            catch( Exception ex )
            {
                if( _monitor != null )
                {
                    _monitor.Error().Send( ex );
                }
            }
        }
    }
}
