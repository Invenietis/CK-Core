using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Impl
{
    static class LogEntry
    {
        public static ILogEntry CreateLog( string text, DateTime t, LogLevel l, CKTrait tags = null )
        {
            if( tags == null ) return new LELog( text, t, l );
            return new LELogWithTrait( text, t, l, tags );
        }

        public static ILogEntry CreateOpenGroup( string text, DateTime t, LogLevel l, CKTrait tags = null, CKExceptionData ex = null )
        {
            if( ex == null )
            {
                if( tags == null ) return new LEOpenGroup( text, t, l );
                else return new LEOpenGroupWithTrait( text, t, l, tags );
            }
            return new LEOpenGroupWithException( text, t, l, tags, ex );
        }

        public static ILogEntry CreateCloseGroup( DateTime t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LECloseGroup( t, level, c );
        }
    }
}
