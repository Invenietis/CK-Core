#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\LogLevel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;
using System.Globalization;

namespace CK.Core
{
    /// <summary>
    /// Log time are built by <see cref="IActivityMonitor.CreateLogTime"/>.
    /// They are unique per monitor.
    /// </summary>
    [Serializable]
    public struct LogTimestamp : IComparable<LogTimestamp>, IEquatable<LogTimestamp>
    {
        /// <summary>
        /// Represents the smallest possible value for a LogTime object.         
        /// </summary>
        static public readonly LogTimestamp MinValue = new LogTimestamp( Util.UtcMinValue, 0 );
        
        /// <summary>
        /// Represents the largest possible value for a LogTime object.         
        /// </summary>
        static public readonly LogTimestamp MaxValue = new LogTimestamp( Util.UtcMaxValue, Byte.MaxValue );

        /// <summary>
        /// DateTime of the log entry.
        /// </summary>
        public readonly DateTime TimeUtc;

        /// <summary>
        /// Uniquifier: non zero when <see cref="TimeUtc"/> collides.
        /// </summary>
        public readonly Byte Uniquifier;

        /// <summary>
        /// Initializes a new <see cref="LogTimestamp"/>.
        /// </summary>
        /// <param name="logTimeUtc">The log time. <see cref="DateTime.Kind"/> must be <see cref="DateTimeKind.Utc"/>.</param>
        /// <param name="uniquifier">Optional non zero uniquifier.</param>
        public LogTimestamp( DateTime logTimeUtc, Byte uniquifier = 0 )
        {
            if( logTimeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );
            TimeUtc = logTimeUtc;
            Uniquifier = uniquifier;
        }

        /// <summary>
        /// Initializes a new <see cref="LogTimestamp"/> that is that is guaranteed to be unique and ascending (unless <paramref name="ensureGreaterThanLastOne"/> 
        /// is false) regarding <paramref name="lastOne"/>.
        /// </summary>
        /// <param name="lastOne">Last log time.</param>
        /// <param name="time">Time of the log.</param>
        /// <param name="ensureGreaterThanLastOne">False to only check for time equality collision instead of guarantying ascending log time.</param>
        public LogTimestamp( LogTimestamp lastOne, DateTime time, bool ensureGreaterThanLastOne = true )
        {
            if( time.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "time" );
            if( ensureGreaterThanLastOne ? time <= lastOne.TimeUtc : time != lastOne.TimeUtc )
            {
                if( lastOne.Uniquifier == Byte.MaxValue )
                {
                    TimeUtc = new DateTime( lastOne.TimeUtc.Ticks + 1, DateTimeKind.Utc );
                    Uniquifier = 1;
                }
                else
                {
                    TimeUtc = lastOne.TimeUtc;
                    Uniquifier = (Byte)(lastOne.Uniquifier + 1);
                }
            }
            else
            {
                TimeUtc = time;
                Uniquifier = 0;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="LogTimestamp"/> that is that is guaranteed to be unique and ascending regarding <paramref name="lastOne"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Uniquifier"/> is optimized if possible (this simply calls <see cref="LogTime(LogTimestamp,DateTime)"/>).
        /// </remarks>
        /// <param name="lastOne">Last log time.</param>
        /// <param name="newTime">Time of the log.</param>
        public LogTimestamp( LogTimestamp lastOne, LogTimestamp newTime )
            : this( lastOne, newTime.TimeUtc )
        {
        }

        /// <summary>
        /// Gets whether this <see cref="LogTimestamp"/> is initialized.
        /// The default constructor of a struct can not be defined and it initializes the <see cref="TimeUtc"/> with a zero that is <see cref="DateTime.MinValue"/>
        /// with a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        public bool IsDefined
        {
            get { return TimeUtc.Kind == DateTimeKind.Utc; }
        }

        /// <summary>
        /// Gets the current <see cref="DateTime.UtcNow"/> as a LogTime.
        /// </summary>
        public static LogTimestamp UtcNow
        {
            get { return new LogTimestamp( DateTime.UtcNow ); }
        }

        /// <summary>
        /// Tries to match a <see cref="LogTimestamp"/> at a given index in the string.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="startAt">Index where the match must start. On success, index of the end of the match.</param>
        /// <param name="time">Result time.</param>
        /// <returns>True if the time has been matched.</returns>
        static public bool Match( string s, ref int startAt, out LogTimestamp time )
        {
            bool ret = false;
            DateTime t;
            Byte uniquifier = 0;
            if( FileUtil.MatchFileNameUniqueTimeUtcFormat( s, ref startAt, out t ) )
            {
                if( startAt < s.Length - 3 && s[startAt] == '(' )
                {
                    int iStartNum = startAt + 1;
                    int iCloseB = s.IndexOf( ')', iStartNum );
                    if( iCloseB > 0 )
                    {
                        if( Byte.TryParse( s.Substring( iStartNum, iCloseB - iStartNum ), out uniquifier ) )
                        {
                            startAt = iCloseB + 1;
                        }
                    }
                }
                ret = true;
            }
            time = new LogTimestamp( t, uniquifier );
            return ret;
        }

        /// <summary>
        /// Compares this <see cref="LogTimestamp"/> to another one.
        /// </summary>
        /// <param name="other">The other log time to compare.</param>
        /// <returns>Positive value when this is greater than other, 0 when they are equal, a negative value otherwise.</returns>
        public int CompareTo( LogTimestamp other )
        {
            int cmp = TimeUtc.CompareTo( other.TimeUtc );
            if( cmp == 0 ) cmp = Uniquifier.CompareTo( other.Uniquifier );
            return cmp;
        }

        /// <summary>
        /// Checks equality.
        /// </summary>
        /// <param name="other">Other log file time.</param>
        /// <returns>True when this is equal to other.</returns>
        public bool Equals( LogTimestamp other )
        {
            return TimeUtc == other.TimeUtc && Uniquifier == other.Uniquifier;
        }

        /// <summary>
        /// Compares this log time to another one.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int CompareTo( object value )
        {
            if( value == null ) return 1;
            if( !(value is LogTimestamp) ) throw new ArgumentException();
            return CompareTo( (LogTimestamp)value );
        }

        /// <summary>
        /// Overridden to check equality.
        /// </summary>
        /// <param name="other">Other object.</param>
        /// <returns>True when this is equal to other.</returns>
        public override bool Equals( object obj )
        {
            return (obj is LogTimestamp) && Equals( (LogTimestamp)obj );
        }

        /// <summary>
        /// Overridden to match <see cref="Equals(object)"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return TimeUtc.GetHashCode() ^ Uniquifier;
        }

        /// <summary>
        /// @"{0:yyyy-MM-dd HH\hmm.ss.fffffff}({1})" is the format that will be used to format log time when the <see cref="Uniquifier"/> is not zero.
        /// It is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> (that is used as-is when the Uniquifier is zero) for the date time format.
        /// </summary>
        static public readonly string FormatWhenUniquifier = "{0:" + FileUtil.FileNameUniqueTimeUtcFormat + "}({1})";

        /// <summary>
        /// Overridden to return a string based on <see cref="FormatWhenUniquifier"/> or <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/>.
        /// </summary>
        /// <returns>A string that can be successfully <see cref="Match"/>ed.</returns>
        public override string ToString()
        {
            return Uniquifier != 0 ? String.Format( FormatWhenUniquifier, TimeUtc, Uniquifier ) : TimeUtc.ToString( FileUtil.FileNameUniqueTimeUtcFormat, CultureInfo.InvariantCulture );
        }

        public static bool operator ==( LogTimestamp d1, LogTimestamp d2 )
        {
            return d1.Equals( d2 );
        }

        public static bool operator >( LogTimestamp t1, LogTimestamp t2 )
        {
            return t1.CompareTo( t2 ) > 0;
        }

        public static bool operator >=( LogTimestamp t1, LogTimestamp t2 )
        {
            return t1.CompareTo( t2 ) >= 0;
        }

        public static bool operator !=( LogTimestamp d1, LogTimestamp d2 )
        {
            return !d1.Equals( d2 );
        }

        public static bool operator <( LogTimestamp t1, LogTimestamp t2 )
        {
            return t1.CompareTo( t2 ) < 0;
        }

        public static bool operator <=( LogTimestamp t1, LogTimestamp t2 )
        {
            return t1.CompareTo( t2 ) <= 0;
        }
    }

}
