using CK.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Supports <see cref="LogFilter"/> and <see cref="LogLevelFilter"/> extension methods.
    /// </summary>
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
            if( !m.MatchText( "Undefined" ) )
            {
                if (m.MatchText("Debug"))
                {
                    f = LogFilter.Debug;
                }
                else if (m.MatchText("Trace"))
                {
                    f = LogFilter.Trace;
                }
                else if ( m.MatchText( "Verbose" ) )
                {
                    f = LogFilter.Verbose;
                }
                else if( m.MatchText( "Monitor" ) )
                {
                    f = LogFilter.Monitor;
                }
                else if( m.MatchText( "Terse" ) )
                {
                    f = LogFilter.Terse;
                }
                else if( m.MatchText( "Release" ) )
                {
                    f = LogFilter.Release;
                }
                else if( m.MatchText( "Off" ) )
                {
                    f = LogFilter.Off;
                }
                else if( m.MatchText( "Invalid" ) )
                {
                    f = LogFilter.Invalid;
                }
                else
                {
                    int savedIndex = m.StartIndex;

                    if( !m.MatchChar( '{' ) ) return m.BackwardAddError( savedIndex );
                    LogLevelFilter group, line;

                    m.MatchWhiteSpaces();
                    if( !m.MatchLogLevelFilter( out group ) ) return m.BackwardAddError( savedIndex );

                    m.MatchWhiteSpaces();
                    if( !m.MatchChar( ',' ) ) return m.BackwardAddError( savedIndex );

                    m.MatchWhiteSpaces();
                    if( !m.MatchLogLevelFilter( out line ) ) return m.BackwardAddError( savedIndex );
                    m.MatchWhiteSpaces();

                    if( !m.MatchChar( '}' ) ) return m.BackwardAddError( savedIndex );
                    f = new LogFilter( group, line );
                }
            }
            return true;
        }

        /// <summary>
        /// Matches a <see cref="LogLevelFilter"/>.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="level">Resulting level.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool MatchLogLevelFilter(this StringMatcher @this, out LogLevelFilter level)
        {
            level = LogLevelFilter.None;
            if (!@this.MatchText("None"))
            {
                if (@this.MatchText("Debug"))
                {
                    level = LogLevelFilter.Debug;
                }
                else if (@this.MatchText("Trace"))
                {
                    level = LogLevelFilter.Trace;
                }
                else if (@this.MatchText("Info"))
                {
                    level = LogLevelFilter.Info;
                }
                else if (@this.MatchText("Warn"))
                {
                    level = LogLevelFilter.Warn;
                }
                else if (@this.MatchText("Error"))
                {
                    level = LogLevelFilter.Error;
                }
                else if (@this.MatchText("Fatal"))
                {
                    level = LogLevelFilter.Fatal;
                }
                else if (@this.MatchText("Off"))
                {
                    level = LogLevelFilter.Off;
                }
                else if (@this.MatchText("Invalid"))
                {
                    level = LogLevelFilter.Invalid;
                }
                else return false;
            }
            return true;
        }
    }
}
