using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    static class LocalSenderExtension
    {
        public static void SendLine( this IActivityMonitor @this, LogLevel level, string text, Exception ex, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            if( @this.ShouldLogLine( level, fileName, lineNumber ) )
            {
                @this.UnfilteredLog( null, level | LogLevel.IsFiltered, text, @this.NextLogTime(), ex, fileName, lineNumber );
            }
        }

        public static IDisposable OpenGroup( this IActivityMonitor @this, LogLevel level, string text, Exception ex, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            if( @this.ShouldLogGroup( level, fileName, lineNumber ) )
            {
                return @this.UnfilteredOpenGroup( new ActivityMonitorGroupData( level | LogLevel.IsFiltered, null, text, @this.NextLogTime(), ex, null, fileName, lineNumber ) );

            }
            return @this.UnfilteredOpenGroup( new ActivityMonitorGroupData() );
        }

    }
}
