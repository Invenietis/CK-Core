using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    internal class ActivityMonitorLineSender : ActivityMonitorLogData, IActivityMonitorLineSender
    {
        readonly IActivityMonitor _monitor;

        /// <summary>
        /// We can use a singleton with a null monitor since to NOT send a line, we do not need the monitor.
        /// This is not the same for groups: to reject a group opening we need to open a "rejected group" in order to track
        /// closing, hence we do need to have a monitor: for groups, we must build a GroupSender that references it.
        /// </summary>
        static internal readonly ActivityMonitorLineSender FakeLineSender = new ActivityMonitorLineSender( null, LogLevel.None, null, 0 );

        /// <summary>
        /// Used only by filtering extension methods (level is filtered and not None) or by static FakeLineSender (level is None).
        /// </summary>
        internal ActivityMonitorLineSender( IActivityMonitor monitor, LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
            Debug.Assert( FakeLineSender == null || ((level & LogLevel.IsFiltered) != 0 && MaskedLevel != LogLevel.None), 
                "The level is already filtered and not None or we are initializing the static FakeLineSender." );
            _monitor = monitor;
        }

        public bool IsRejected
        {
            get { return _monitor == null; }
        }

        internal void InitializeAndSend( Exception exception, CKTrait tags, string text )
        {
            Debug.Assert( !IsRejected );
            Initialize( text, exception, tags, _monitor.NextLogTime() );
            _monitor.UnfilteredLog( this );
        }
    }
}
