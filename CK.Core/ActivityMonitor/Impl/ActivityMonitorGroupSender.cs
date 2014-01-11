using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CK.Core
{
    internal class ActivityMonitorGroupSender : ActivityMonitorGroupData, IActivityMonitorGroupSender
    {
        internal readonly IActivityMonitor Monitor;

        /// <summary>
        /// Gets wether the log has been rejected.
        /// </summary>
        public bool IsRejected { get { return Level == LogLevel.None; } }

        /// <summary>
        /// Used only by filtering extension methods (level is always filtered).
        /// </summary>
        internal ActivityMonitorGroupSender( IActivityMonitor monitor, LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
            Debug.Assert( monitor != null );
            Debug.Assert( ((level & LogLevel.IsFiltered) != 0 && MaskedLevel != LogLevel.None),
                "The level is already filtered and not None or we are initializing the monitor's FakeLineSender." );
            Monitor = monitor;
        }

        /// <summary>
        /// Used only to initialize a ActivityMonitorGroupSender for rejected opened group.
        /// </summary>
        internal ActivityMonitorGroupSender( IActivityMonitor monitor )
        {
            Debug.Assert( monitor != null );
            Monitor = monitor;
        }

        internal IDisposableGroup InitializeAndSend( Exception exception, CKTrait tags, string text )
        {
            Debug.Assert( !IsRejected );
            Initialize( text, exception, tags, Monitor.NextLogTime() );
            return Monitor.UnfilteredOpenGroup( this );
        }

    }
}
