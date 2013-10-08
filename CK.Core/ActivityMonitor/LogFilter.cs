using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Immutable capture of a double <see cref="LogLevelFilter"/>. One for <see cref="Line"/> and one for <see cref="Group"/>.
    /// </summary>
    public struct LogFilter
    {
        /// <summary>
        /// The filter that applies to log lines (Trace, Info, Warn, Error and Fatal). 
        /// </summary>
        public readonly LogLevelFilter Line;

        /// <summary>
        /// The filter that applies to groups (<see cref="IAcitvityMonitor.OpenGroup"/>). 
        /// </summary>
        public readonly LogLevelFilter Group;

        /// <summary>
        /// Initializes a new <see cref="LogFilter"/> with a level for <see cref="Line"/> logs and <see cref="Group"/>s.
        /// </summary>
        /// <param name="line">Filter for lines.</param>
        /// <param name="group">Filter for groups.</param>
        public LogFilter( LogLevelFilter line, LogLevelFilter group )
        {
            Line = line;
            Group = group;
        }

        /// <summary>
        /// Combines this filter with another one. <see cref="Line"/> and <see cref="Group"/> level filters
        /// are combined with <see cref="Combine(LogLevelFilter,LogLevelFilter)"/>.
        /// </summary>
        /// <param name="other">The other filter to combine with this one.</param>
        /// <returns>The resulting filter.</returns>
        public LogFilter Combine( LogFilter other )
        {
            return new LogFilter( Combine( Line, other.Line ), Combine( Group, other.Group ) );
        }

        /// <summary>
        /// Returns a <see cref="LogFilter"/> with a given <see cref="LogLevelFilter"/> for the <see cref="Line"/>.
        /// </summary>
        /// <param name="line">Filter for the line.</param>
        /// <returns>The filter with the line level.</returns>
        public LogFilter SetLine( LogLevelFilter line )
        {
            return new LogFilter( line, Group );
        }

        /// <summary>
        /// Returns a <see cref="LogFilter"/> with a given <see cref="LogLevelFilter"/> for the <see cref="Group"/>.
        /// </summary>
        /// <param name="group">Filter for the group.</param>
        /// <returns>The filter with the group level.</returns>
        public LogFilter SetGroup( LogLevelFilter group )
        {
            return new LogFilter( Line, group );
        }

        /// <summary>
        /// Combines two enums <see cref="LogLevelFilter"/> into one.
        /// The resulting filter is the more verbose one (the smallest level).
        /// This operation is commutative and associative: different order of combination always give the same result.
        /// </summary>
        /// <param name="x">First filter level.</param>
        /// <param name="y">Second filter level.</param>
        /// <returns>The resulting level.</returns>
        static public LogLevelFilter Combine( LogLevelFilter x, LogLevelFilter y )
        {
            if( x == LogLevelFilter.None ) return y;
            if( y == LogLevelFilter.None ) return x;
            if( y < x ) return y;
            return x;
        }

        /// <summary>
        /// Overridden to compare <see cref="Line"/> and <see cref="Group"/>.
        /// </summary>
        /// <param name="obj">Other object.</param>
        /// <returns>True if Line and Group are equal.</returns>
        public override bool Equals( object obj )
        {
            if( obj is LogFilter )
            {
                LogFilter x = (LogFilter)obj;
                return x.Line == Line && x.Group == Group;
            }
            return false;
        }

        /// <summary>
        /// Overridden to compute hash based on <see cref="Line"/> and <see cref="Group"/> values.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return ((int)Line) << 16 + (int)Group;
        }

        /// <summary>
        /// Overridden to show the line and the group level.
        /// </summary>
        /// <returns>A detailed string.</returns>
        public override string ToString()
        {
            return String.Format( "LogFilter: Line={0}, Group={1}.", Line, Group );
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="x">First filter.</param>
        /// <param name="y">Second filter.</param>
        /// <returns>True if <see cref="Line"/> and <see cref="Group"/> are the same for the two filters.</returns>
        public static bool operator ==( LogFilter x, LogFilter y )
        {
            return x.Line == y.Line && x.Group == y.Group;
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="x">First filter.</param>
        /// <param name="y">Second filter.</param>
        /// <returns>True if <see cref="Line"/> and <see cref="Group"/> are the same for the two filters.</returns>
        public static bool operator !=( LogFilter x, LogFilter y )
        {
            return x.Line != y.Line || x.Group != y.Group;
        }


    }

}
