using System;
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
            if( sinks != null )
            {
                foreach( var l in sinks )
                {
                    try
                    {
                        l.Handle( logEvent, parrallelCall );
                    }
                    catch( Exception exCall )
                    {
                        ActivityMonitor.CriticalErrorCollector.Add( exCall, l.GetType().FullName );
                        Util.InterlockedRemove( ref _sinks, l );
                    }
                }
            }
        }
    }
}
