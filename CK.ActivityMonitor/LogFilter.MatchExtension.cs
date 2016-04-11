using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    public static class LogFilterMatcherExtension
    {
        /// <summary>
        /// Matches a <see cref="LogFilter"/>: it can be a predefined filter as ("Undefined", "Debug", "Verbose", etc.)  
        /// or as {GroupLogLevelFilter,LineLogLevelFilter} pairs like "{None,None}", "{Error,Trace}".
        /// </summary>
        /// <param name="m">This <see cref="StringMatcher"/>.</param>
        /// <param name="f">Resulting filter.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool MatchLogFilter( this StringMatcher m, out LogFilter f )
        {
            f = LogFilter.Undefined;
            if( !m.MatchString( "Undefined" ) )
            {
                if( m.MatchString( "Debug" ) )
                {
                    f = LogFilter.Debug;
                }
                else if( m.MatchString( "Verbose" ) )
                {
                    f = LogFilter.Verbose;
                }
                else if( m.MatchString( "Monitor" ) )
                {
                    f = LogFilter.Monitor;
                }
                else if( m.MatchString( "Terse" ) )
                {
                    f = LogFilter.Terse;
                }
                else if( m.MatchString( "Release" ) )
                {
                    f = LogFilter.Release;
                }
                else if( m.MatchString( "Off" ) )
                {
                    f = LogFilter.Off;
                }
                else if( m.MatchString( "Invalid" ) )
                {
                    f = LogFilter.Invalid;
                }
                else
                {
                    int savedIndex = m.StartIndex;

                    if( !m.MatchChar( '{' ) ) return m.BackwardSetError( savedIndex );
                    LogLevelFilter group, line;

                    m.MatchWhiteSpaces();
                    if( !m.MatchLogLevelFilter( out group ) ) return m.BackwardSetError( savedIndex );

                    m.MatchWhiteSpaces();
                    if( !m.MatchChar( ',' ) ) return m.BackwardSetError( savedIndex );

                    m.MatchWhiteSpaces();
                    if( !m.MatchLogLevelFilter( out line ) ) return m.BackwardSetError( savedIndex );
                    m.MatchWhiteSpaces();

                    if( !m.MatchChar( '}' ) ) return m.BackwardSetError( savedIndex );
                    f = new LogFilter( group, line );
                }
            }
            return true;
        }

        /// <summary>
        /// Matches a <see cref="LogLevelFilter"/>.
        /// </summary>
        /// <param name="s">This <see cref="StringMatcher"/>.</param>
        /// <param name="level">Resulting level.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool MatchLogLevelFilter( this StringMatcher @this, out LogLevelFilter level )
        {
            level = LogLevelFilter.None;
            if( !@this.MatchString( "None" ) )
            {
                if( @this.MatchString( "Trace" ) )
                {
                    level = LogLevelFilter.Trace;
                }
                else if( @this.MatchString( "Info" ) )
                {
                    level = LogLevelFilter.Info;
                }
                else if( @this.MatchString( "Warn" ) )
                {
                    level = LogLevelFilter.Warn;
                }
                else if( @this.MatchString( "Error" ) )
                {
                    level = LogLevelFilter.Error;
                }
                else if( @this.MatchString( "Fatal" ) )
                {
                    level = LogLevelFilter.Fatal;
                }
                else if( @this.MatchString( "Off" ) )
                {
                    level = LogLevelFilter.Off;
                }
                else if( @this.MatchString( "Invalid" ) )
                {
                    level = LogLevelFilter.Invalid;
                }
                else return false;
            }
            return true;
        }
    }
}
