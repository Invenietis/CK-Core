using System;

namespace CK.Core
{
    /// <summary>
    /// Thread safe <see cref="DateTimeStamp"/> provider: the ever increasing
    /// <see cref="Value"/> is protected by an internal lock: there should be very few contentions
    /// here (the update operation is very fast), so we keep it simple (lock is efficient when there
    /// is no contention).
    /// </summary>
    public sealed class DateTimeStampProvider
    {
        DateTimeStamp _value;
        readonly object _lock;

        /// <summary>
        /// Initializes a new <see cref="DateTimeStampProvider"/>.
        /// </summary>
        public DateTimeStampProvider()
        {
            _lock = new object();
            _value = DateTimeStamp.MinValue;
        }

        /// <summary>
        /// Gets the current value.
        /// This value is guaranteed to be ever increasing.
        /// Defaults to <see cref="DateTimeStamp.MinValue"/>.
        /// </summary>
        public DateTimeStamp Value => _value;

        /// <summary>
        /// Resets the current value to a given time stamp.
        /// </summary>
        public DateTimeStamp Reset( DateTime utcTime, byte uniquifier = 0 )
        {
            lock( _lock )
            {
                return _value = new DateTimeStamp( utcTime, uniquifier );
            }
        }

        /// <summary>
        /// Updates and gets the current value: it will be necessarily after the current one if
        /// <paramref name="utcTime"/> is before.
        /// </summary>
        /// <param name="utcTime">The new time to consider.</param>
        /// <returns>The updated current value.</returns>
        public DateTimeStamp GetNext( DateTime utcTime )
        {
            lock( _lock )
            {
                return _value = new DateTimeStamp( _value, utcTime );
            }
        }

        /// <summary>
        /// Updates and gets the current value based on <see cref="DateTime.UtcNow"/>.
        /// </summary>
        /// <returns>The updated current value.</returns>
        public DateTimeStamp GetNextNow()
        {
            lock( _lock )
            {
                return _value = new DateTimeStamp( _value, DateTime.UtcNow );
            }
        }
    }

}
