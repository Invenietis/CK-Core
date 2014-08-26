using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Udp
{
    public abstract class UdpPacketComposer<T> : IUdpPacketComposer<T>
    {
        Action<T> _callback;
        Dictionary<Guid, UdpPacketEnvelope[]> _envelopes = new Dictionary<Guid, UdpPacketEnvelope[]>();

        public void PushBuffer( byte[] dataGram )
        {
            UdpPacketEnvelope e = UdpPacketEnvelope.FromByteArray( dataGram );

            UdpPacketEnvelope[] col;
            if( _envelopes.TryGetValue( e.CorrelationId, out col ) )
            {
                col[e.SequenceNumber] = e;
            }
            else
            {
                var ar = new UdpPacketEnvelope[e.Count];
                ar[e.SequenceNumber] = e;
                _envelopes.Add( e.CorrelationId, ar );
            }

            T entry;
            if( TryGetFullItem( e.CorrelationId, out entry ) )
            {
                _callback( entry );
                // Cleanup the dictionary
                _envelopes.Remove( e.CorrelationId );
            }
        }

        private bool TryGetFullItem( Guid guid, out T entry )
        {
            UdpPacketEnvelope[] envelopes = _envelopes[guid];
            for( int i = 0; i < envelopes.Length; ++i )
            {
                if( envelopes[i] == null )
                {
                    entry = default( T );
                    return false;
                }
            }

            entry = Recompose( envelopes );
            return true;
        }

        protected abstract T Recompose( UdpPacketEnvelope[] envelopes );

        public void OnObjectRestored( Action<T> callback )
        {
            if( callback == null ) throw new ArgumentNullException( "callback" );
            _callback = callback;
        }
    }

}
