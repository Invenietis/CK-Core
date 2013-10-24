using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    public class ActivityMonitorLineSender : ActivityMonitorData
    {
        internal readonly IActivityMonitor Monitor;

        internal ActivityMonitorLineSender( IActivityMonitor monitor, LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
            Debug.Assert( (level & LogLevel.IsFiltered) != 0, "The level is already filtered." );
            Monitor = monitor;
        }

        internal void InitializeAndSend( string text, Exception exception, CKTrait tags, DateTime logTimeUtc )
        {
            Initialize( text, exception, tags, logTimeUtc );
            Monitor.UnfilteredLog( this );
        }


    }
}
