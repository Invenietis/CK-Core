using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Monitoring.GrandOutputHandlers
{
    class FakeHandler : HandlerBase
    {
        int _extraLoad;

        public static int TotalHandleCount;
        public static int HandlePerfTraceCount;
        public static int SizeHandled;

        public FakeHandler( FakeHandlerConfiguration config )
            : base( config )
        {
            _extraLoad = config.ExtraLoad;
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            ++TotalHandleCount;
            if( logEvent.Entry.LogType == LogEntryType.Line && logEvent.Entry.Text.StartsWith( "PerfTrace:" ) ) ++HandlePerfTraceCount;
            ComputeSize( logEvent, true );
            for( int i = 0; i < _extraLoad; ++i ) ComputeSize( logEvent, false );
        }

        void ComputeSize( GrandOutputEventInfo logEvent, bool increment )
        {
            using( MemoryStream m = new MemoryStream() )
            using( BinaryWriter w = new BinaryWriter( m ) )
            {
                logEvent.Entry.WriteLogEntry( w );
                if( increment ) Interlocked.Add( ref SizeHandled, (int)m.Position );
            }
        }

    }
}
