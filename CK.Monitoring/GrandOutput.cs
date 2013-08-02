using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    public partial class GrandOutput
    {
        readonly List<WeakRef<GrandOutputClient>> _clients;
        readonly GrandOutputCompositeSink _commonSink;

        public GrandOutput()
        {
            _clients = new List<WeakRef<GrandOutputClient>>();
            _commonSink = new GrandOutputCompositeSink();
        }

        public GrandOutputClient Register( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            var c = monitor.Output.Clients.OfType<GrandOutputClient>().Where( b => b.Central == this ).FirstOrDefault();
            if( c == null )
            {
                c = new GrandOutputClient( this );
                monitor.Output.RegisterClient( c );
                lock( _clients ) 
                {
                    _clients.Add( new WeakRef<GrandOutputClient>( c ) ); 
                }
            }
            return c;
        }

        public void RegisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Add( sink );
        }

        public void UnregisterGlobalSink( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            _commonSink.Remove( sink );
        }

        internal GrandOutputChannel ObtainChannel( string channelName )
        {
            return null;
        }

        private void DoGarbageDeadClients()
        {
            throw new NotImplementedException();
        }

        class ChannelManager
        {
            public GrandOutputChannel ObtainChannel( string channelName )
            {
              return null;
          }

        }

    }
}
