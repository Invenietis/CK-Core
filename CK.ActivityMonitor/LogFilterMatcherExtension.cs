using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public static class LogFilterMatcherExtension
    {
        /// <summary>
        /// Matches a <see cref="LogLevelFilter"/>.
        /// </summary>
        /// <param name="level">Resulting level.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool MatchLogLevelFilter( this StringMatcher @this, out LogLevelFilter level )
        {
            level = LogLevelFilter.None;
            if( !@this.TryMatchString( "None" ) )
            {
                if( @this.TryMatchString( "Trace" ) )
                {
                    level = LogLevelFilter.Trace;
                }
                else if( @this.TryMatchString( "Info" ) )
                {
                    level = LogLevelFilter.Info;
                }
                else if( @this.TryMatchString( "Warn" ) )
                {
                    level = LogLevelFilter.Warn;
                }
                else if( @this.TryMatchString( "Error" ) )
                {
                    level = LogLevelFilter.Error;
                }
                else if( @this.TryMatchString( "Fatal" ) )
                {
                    level = LogLevelFilter.Fatal;
                }
                else if( @this.TryMatchString( "Off" ) )
                {
                    level = LogLevelFilter.Off;
                }
                else if( @this.TryMatchString( "Invalid" ) )
                {
                    level = LogLevelFilter.Invalid;
                }
                else return @this.SetError();
            }
            return @this.SetSuccess();
        }

        /// <summary>
        /// Tries to parse a <see cref="LogFilter"/>: it can be a predefined filter as ("Undefined", "Debug", "Verbose", etc.)  
        /// or as {GroupLogLevelFilter,LineLogLevelFilter} pairs like "{None,None}", "{Error,Trace}".
        /// </summary>
        /// <param name="s">Filter to parse.</param>
        /// <param name="f">Resulting filter.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool MatchLogFilter( this StringMatcher @this, out LogFilter f )
        {
            f = LogFilter.Undefined;
            int savedIndex = @this.StartIndex;
            if( !@this.TryMatchString( "Undefined" ) )
            {
                if( @this.TryMatchString( "Debug" ) )
                {
                    f = LogFilter.Debug;
                }
                else if( @this.TryMatchString( "Verbose" ) )
                {
                    f = LogFilter.Verbose;
                }
                else if( @this.TryMatchString( "Monitor" ) )
                {
                    f = LogFilter.Monitor;
                }
                else if( @this.TryMatchString( "Terse" ) )
                {
                    f = LogFilter.Terse;
                }
                else if( @this.TryMatchString( "Release" ) )
                {
                    f = LogFilter.Release;
                }
                else if( @this.TryMatchString( "Off" ) )
                {
                    f = LogFilter.Off;
                }
                else if( @this.TryMatchString( "Invalid" ) )
                {
                    f = LogFilter.Invalid;
                }
                else
                {
                    if( @this.Head != '{' || @this.Length < 9 ) return @this.BackwardSetError( savedIndex );
                    @this.UncheckedMove( 1 );
                    LogLevelFilter group, line;

                    @this.MatchWhiteSpaces( 0 );
                    if( !@this.MatchLogLevelFilter( out group ) ) return @this.BackwardSetError( savedIndex );

                    @this.MatchWhiteSpaces( 0 );
                    if( !@this.MatchChar( ',' ) ) return @this.BackwardSetError( savedIndex );

                    @this.MatchWhiteSpaces( 0 );
                    if( !@this.MatchLogLevelFilter( out line ) ) return @this.BackwardSetError( savedIndex );
                    @this.MatchWhiteSpaces( 0 );

                    if( !@this.MatchChar( '}' ) ) return @this.BackwardSetError( savedIndex );
                    f = new LogFilter( group, line );
                }
            }
            return @this.SetSuccess();
        }

    }

}
