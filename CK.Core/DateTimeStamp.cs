using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// A date and time stamp encapsulates a <see cref="TimeUtc"/> (<see cref="DateTime"/> guaranteed to be in Utc) and a <see cref="Uniquifier"/>.
    /// </summary>
    /// <remarks>
    /// Simply use <see cref="ToString()"/> and <see cref="DateTimeStampExtension.MatchDateTimeStamp(Text.StringMatcher, out DateTimeStamp)">MatchDateTimeStamp</see>
    /// to serialize it.
    /// </remarks>
    [Serializable]
    public struct DateTimeStamp : IComparable<DateTimeStamp>, IEquatable<DateTimeStamp>
    {
        /// <summary>
        /// Represents the smallest possible value for a DateTimeStamp object.         
        /// </summary>
        static public readonly DateTimeStamp MinValue = new DateTimeStamp( Util.UtcMinValue, 0 );
        
        /// <summary>
        /// Represents an unknown, default, DateTimeStamp object.
        /// This is available to have a more expressive code than <c>new DateTimeStamp()</c>.
        /// </summary>
        static public readonly DateTimeStamp Unknown = new DateTimeStamp();
        
        /// <summary>
        /// Represents the largest possible value for a DateTimeStamp object.         
        /// </summary>
        static public readonly DateTimeStamp MaxValue = new DateTimeStamp( Util.UtcMaxValue, Byte.MaxValue );

        /// <summary>
        /// Represents an invalid DateTimeStamp object. See <see cref="IsInvalid"/>.
        /// </summary>
        static public readonly DateTimeStamp Invalid = new DateTimeStamp( 0 );

        /// <summary>
        /// DateTime in Utc.
        /// </summary>
        public readonly DateTime TimeUtc;

        /// <summary>
        /// Uniquifier: non zero when <see cref="TimeUtc"/> collides.
        /// </summary>
        public readonly Byte Uniquifier;

        DateTimeStamp( int justForInvalidOne )
        {
            TimeUtc = new DateTime( DateTime.MinValue.Ticks, DateTimeKind.Local );
            Uniquifier = 0;
        }

        /// <summary>
        /// Initializes a new <see cref="DateTimeStamp"/>.
        /// </summary>
        /// <param name="timeUtc">The log time. <see cref="DateTime.Kind"/> must be <see cref="DateTimeKind.Utc"/>.</param>
        /// <param name="uniquifier">Optional non zero uniquifier.</param>
        public DateTimeStamp( DateTime timeUtc, Byte uniquifier = 0 )
        {
            if( timeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( Impl.CoreResources.DateTimeMustBeUtc, nameof( timeUtc ) );
            TimeUtc = timeUtc;
            Uniquifier = uniquifier;
        }

        /// <summary>
        /// Initializes a new <see cref="DateTimeStamp"/> that is that is guaranteed to be unique and ascending (unless <paramref name="ensureGreaterThanLastOne"/> 
        /// is false) regarding <paramref name="lastOne"/>.
        /// </summary>
        /// <param name="lastOne">Last time stamp.</param>
        /// <param name="time">Time (generally current <see cref="DateTime.UtcNow"/>).</param>
        /// <param name="ensureGreaterThanLastOne">False to only check for time equality collision instead of guarantying ascending log time.</param>
        public DateTimeStamp( DateTimeStamp lastOne, DateTime time, bool ensureGreaterThanLastOne = true )
        {
            if( time.Kind != DateTimeKind.Utc ) throw new ArgumentException( Impl.CoreResources.DateTimeMustBeUtc, nameof( time ) );
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
        /// Initializes a new <see cref="DateTimeStamp"/> that is that is guaranteed to be unique and ascending regarding <paramref name="lastOne"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Uniquifier"/> is optimized if possible (this simply calls <see cref="DateTimeStamp(DateTimeStamp,DateTime,bool)"/> with ensureGreaterThanLastOne sets to true).
        /// </remarks>
        /// <param name="lastOne">Last time stamp.</param>
        /// <param name="newTime">DateTimeStamp to combine.</param>
        public DateTimeStamp( DateTimeStamp lastOne, DateTimeStamp newTime )
            : this( lastOne, newTime.TimeUtc, ensureGreaterThanLastOne:true )
        {
        }

        /// <summary>
        /// Gets whether this <see cref="DateTimeStamp"/> is initialized.
        /// The default constructor of a structure can not be defined and it initializes the <see cref="TimeUtc"/> with a zero that is <see cref="DateTime.MinValue"/>
        /// with a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        public bool IsKnown
        {
            get { return TimeUtc.Kind == DateTimeKind.Utc; }
        }

        /// <summary>
        /// Gets whether this <see cref="DateTimeStamp"/> is the <see cref="Invalid"/> one.
        /// <see cref="TimeUtc"/> has a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Local"/>.
        /// </summary>
        public bool IsInvalid
        {
            get { return TimeUtc.Kind == DateTimeKind.Local; }
        }

        /// <summary>
        /// Gets the current <see cref="DateTime.UtcNow"/> as a DateTimeStamp.
        /// </summary>
        public static DateTimeStamp UtcNow
        {
            get { return new DateTimeStamp( DateTime.UtcNow ); }
        }

        /// <summary>
        /// Compares this <see cref="DateTimeStamp"/> to another one.
        /// </summary>
        /// <param name="other">The other DateTimeStamp to compare.</param>
        /// <returns>Positive value when this is greater than other, 0 when they are equal, a negative value otherwise.</returns>
        public int CompareTo( DateTimeStamp other )
        {
            int cmp = TimeUtc.CompareTo( other.TimeUtc );
            if( cmp == 0 ) cmp = Uniquifier.CompareTo( other.Uniquifier );
            return cmp;
        }

        /// <summary>
        /// Checks equality.
        /// </summary>
        /// <param name="other">Other DateTimeStamp.</param>
        /// <returns>True when this is equal to other.</returns>
        public bool Equals( DateTimeStamp other )
        {
            return TimeUtc == other.TimeUtc && Uniquifier == other.Uniquifier;
        }

        /// <summary>
        /// Compares this DateTimeStamp to another object that must also be a stamp.
        /// </summary>
        /// <param name="value">The object to compare.</param>
        /// <returns>Positive value when this is greater than other, 0 when they are equal, a negative value otherwise.</returns>
        public int CompareTo( object value )
        {
            if( value == null ) return 1;
            if( !(value is DateTimeStamp) ) throw new ArgumentException();
            return CompareTo( (DateTimeStamp)value );
        }

        /// <summary>
        /// Overridden to check equality.
        /// </summary>
        /// <param name="other">Other object.</param>
        /// <returns>True when this is equal to other.</returns>
        public override bool Equals( object? other ) => other is DateTimeStamp o && Equals( o );

        /// <summary>
        /// Overridden to match <see cref="Equals(DateTimeStamp)"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => TimeUtc.GetHashCode() ^ Uniquifier;

        /// <summary>
        /// @"{0:yyyy-MM-dd HH\hmm.ss.fffffff}({1})" is the format that will be used to format log time when the <see cref="Uniquifier"/> is not zero.
        /// It is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> (that is used as-is when the Uniquifier is zero) for the date time format.
        /// </summary>
        static public readonly string FormatWhenUniquifier = "{0:" + FileUtil.FileNameUniqueTimeUtcFormat + "}({1})";

        /// <summary>
        /// Overridden to return a string based on <see cref="FormatWhenUniquifier"/> or <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/>.
        /// </summary>
        /// <returns>A string that can be successfully matched.</returns>
        public override string ToString()
        {
            return Uniquifier != 0
                    ? String.Format( FormatWhenUniquifier, TimeUtc, Uniquifier )
                    : TimeUtc.ToString( FileUtil.FileNameUniqueTimeUtcFormat, CultureInfo.InvariantCulture );
        }

        /// <summary>
        /// Checks equality.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps are equals.</returns>
        public static bool operator ==( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return t1.Equals( t2 );
        }

        /// <summary>
        /// Checks inequality.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps are different.</returns>
        public static bool operator !=( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return !t1.Equals( t2 );
        }

        /// <summary>
        /// Strict greater than operator.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps first is greater than second.</returns>
        public static bool operator >( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return t1.CompareTo( t2 ) > 0;
        }

        /// <summary>
        /// Large greater than operator.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps first is greater than or equal to second.</returns>
        public static bool operator >=( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return t1.CompareTo( t2 ) >= 0;
        }

        /// <summary>
        /// Strict lower than operator.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps first is lower than second.</returns>
        public static bool operator <( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return t1.CompareTo( t2 ) < 0;
        }

        /// <summary>
        /// Large lower than operator.
        /// </summary>
        /// <param name="t1">First stamp.</param>
        /// <param name="t2">Second stamp.</param>
        /// <returns>True when stamps first is lower than or equal to second.</returns>
        public static bool operator <=( DateTimeStamp t1, DateTimeStamp t2 )
        {
            return t1.CompareTo( t2 ) <= 0;
        }
    }

}
