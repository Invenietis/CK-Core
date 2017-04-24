using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityMonitor"/> and other types from the Activity monitor framework.
    /// </summary>
    public static partial class ActivityMonitorSenderExtension
    {

        /// <summary>
        /// Private method used by XXX (Trace, Info,..., Fatal) extension methods.
        /// </summary>
        static IActivityMonitorLineSender FilterLogLine( this IActivityMonitor @this, LogLevel level, string fileName, int lineNumber )
        {
            System.Diagnostics.Debug.Assert( (level & LogLevel.IsFiltered) == 0 );
            if( @this.ShouldLogLine( level, fileName, lineNumber ) )
            {
                return new ActivityMonitorLineSender( @this, level | LogLevel.IsFiltered, fileName, lineNumber );
            }
            return ActivityMonitorLineSender.FakeLineSender;
        }

        /// <summary>
        /// Private method used by OpenXXX (Trace, Info,..., Fatal) extension methods.
        /// </summary>
        static IActivityMonitorGroupSender FilteredGroup( IActivityMonitor @this, LogLevel level, string fileName, int lineNumber )
        {
            System.Diagnostics.Debug.Assert( (level & LogLevel.IsFiltered) == 0 );
            if( @this.ShouldLogGroup( level, fileName, lineNumber ) )
            {
                return new ActivityMonitorGroupSender( @this, level | LogLevel.IsFiltered, fileName, lineNumber );
            }
            return new ActivityMonitorGroupSender( @this );
        }

    }
}
