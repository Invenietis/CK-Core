using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Immutable capture of a double <see cref="LogLevelFilter"/>. One for <see cref="Line"/> and one for <see cref="Group"/>.
    /// This value type exposes predefined configured couples: <see cref="Debug"/> (full trace), <see cref="Verbose"/>, <see cref="Monitor"/>, 
    /// <see cref="Terse"/>, <see cref="Release"/> and <see cref="Off"/> (no log at all).
    /// </summary>
    [Serializable]
    public struct LogFilter
    {
        /// <summary>
        /// Undefined filter is <see cref="LogLevelFilter.None"/> for both <see cref="Line"/> and <see cref="Group"/>.
        /// </summary>
        static public readonly LogFilter Undefined = new LogFilter( LogLevelFilter.None, LogLevelFilter.None );

        /// <summary>
        /// Debug filter enables full <see cref="LogLevelFilter.Trace"/> for both <see cref="Line"/> and <see cref="Group"/>.
        /// </summary>
        static public readonly LogFilter Debug = new LogFilter( LogLevelFilter.Trace, LogLevelFilter.Trace );

        /// <summary>
        /// Verbose <see cref="LogLevelFilter.Trace"/> all <see cref="Group"/>s but limits <see cref="Line"/> to <see cref="LogLevelFilter.Info"/> level.
        /// </summary>
        static public readonly LogFilter Verbose = new LogFilter( LogLevelFilter.Info, LogLevelFilter.Trace );

        /// <summary>
        /// While monitoring, only errors and warnings are captured, whereas all <see cref="Group"/>s appear to get the detailed structure of the activity.
        /// </summary>
        static public readonly LogFilter Monitor = new LogFilter( LogLevelFilter.Warn, LogLevelFilter.Trace );

        /// <summary>
        /// Terse filter captures only errors for <see cref="Line"/> and limits <see cref="Group"/>s to <see cref="LogLevelFilter.Info"/> level.
        /// </summary>
        static public readonly LogFilter Terse = new LogFilter( LogLevelFilter.Error, LogLevelFilter.Info );
        
        /// <summary>
        /// Release filter captures only <see cref="LogLevelFilter.Error"/>s for both <see cref="Line"/> and <see cref="Group"/>.
        /// </summary>
        static public readonly LogFilter Release = new LogFilter( LogLevelFilter.Error, LogLevelFilter.Error );

        /// <summary>
        /// Off filter does not capture anything.
        /// </summary>
        static public readonly LogFilter Off = new LogFilter( LogLevelFilter.Off, LogLevelFilter.Off );

        /// <summary>
        /// Invalid must be used as a special value. It is <see cref="LogLevelFilter.Invalid"/> for both <see cref="Line"/> and <see cref="Group"/>.
        /// </summary>
        static public readonly LogFilter Invalid = new LogFilter( LogLevelFilter.Invalid, LogLevelFilter.Invalid );

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
        /// Combines this filter with another one only if <see cref="Line"/> or <see cref="Group"/> is <see cref="LogLevelFilter.None"/>.
        /// </summary>
        /// <param name="other">The other filter to combine with this one.</param>
        /// <returns>The resulting filter.</returns>
        public LogFilter CombineNoneOnly( LogFilter other )
        {
            var l = Line == LogLevelFilter.None ? other.Line : Line;
            var g = Group == LogLevelFilter.None ? other.Group : Group;
            return new LogFilter( l, g );
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
        /// Tests if <see cref="Combine(LogFilter)">combining</see> this and <paramref name="x"/> will result in a different filter than x.
        /// </summary>
        /// <param name="x">The other filter.</param>
        /// <returns>True if combining this filter and <paramref name="x"/> will change x.</returns>
        public bool HasImpactOn( LogFilter x )
        {
            return (Line != LogLevelFilter.None && Line < x.Line) || (Group != LogLevelFilter.None && Group < x.Group); 
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
