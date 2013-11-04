using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    internal class GrandOutputCompositeSink : IGrandOutputSink
    {
        IGrandOutputSink[] _sinks;

        public void Add( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            Util.InterlockedAdd( ref _sinks, sink );
        }

        public void Remove( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            Util.InterlockedRemove( ref _sinks, sink );
        }

        void IGrandOutputSink.Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            var sinks = _sinks;
            foreach( var l in sinks )
            {
                try
                {
                    l.Handle( logEvent, parrallelCall );
                }
                catch( Exception exCall )
                {
                    ActivityMonitor.MonitoringError.Add( exCall, l.GetType().FullName );
                    Util.InterlockedRemove( ref _sinks, l );
                }
            }
        }
    }
}
