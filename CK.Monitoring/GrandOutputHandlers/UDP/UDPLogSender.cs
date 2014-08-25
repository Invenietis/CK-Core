using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers.UDP
{
    class UDPLogSender : ILogSender
    {
        readonly int _port;
        readonly UdpClient _client;

        const int UDPMAXPACKETSIZE = 512;

        public UDPLogSender( int port )
        {
            _port = port;
            _client = new UdpClient();
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
            _client.Send( buffer, buffer.Length );
        }

        public Task SendLogAsync( IMulticastLogEntry entry )
        {
            var buffer = PrepareSend( entry );
            return _client.SendAsync( buffer, buffer.Length );
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
