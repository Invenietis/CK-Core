using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Exposes extension methods on <see cref="DateTimeStamp"/>.
    /// </summary>
    public static class DateTimeStampExtension
    {
        /// <summary>
        /// Matches a <see cref="DateTimeStamp"/>.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="time">Resulting time stamp on successful match; <see cref="DateTimeStamp.Unknown"/> otherwise.</param>
        /// <returns>True if the time stamp has been matched.</returns>
        static public bool MatchDateTimeStamp( this StringMatcher @this, out DateTimeStamp time )
        {
            time = DateTimeStamp.Unknown;
            int savedIndex = @this.StartIndex;
            if( !@this.MatchFileNameUniqueTimeUtcFormat( out DateTime t ) ) return @this.SetError();
            byte uniquifier = 0;
            if( @this.MatchChar( '(' ) )
            {
                if( !@this.MatchInt32( out int unique, 0, 255 ) || !@this.TryMatchChar( ')' ) ) return @this.BackwardAddError( savedIndex );
                uniquifier = (byte)unique;
            }
            time = new DateTimeStamp( t, uniquifier );
            return @this.Forward( 0 );
        }

    }
}
