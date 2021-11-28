using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Typical "pubternal" implementation of the <see cref="ISystemClock"/>.
    /// </summary>
    public sealed class SystemClock : ISystemClock
    {
        sealed class NoSystemClock : ISystemClock
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a default clock directly bound to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public static ISystemClock Default = new NoSystemClock(); 

        /// <summary>
        /// Gets or sets the offset to apply to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public TimeSpan Offset { get; set; }

        /// <summary>
        /// Gets the <see cref="DateTime.Now"/> offset by this <see cref="Offset"/> value.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow.Add( Offset );
    }
}
