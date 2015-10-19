using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public static class DateTimeStampMatcherExtension
    {
        /// <summary>
        /// Matches a <see cref="DateTimeStamp"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="time">Resulting time stamp on successful match; <see cref="DateTimeStamp.Unknown"/> otherwise.</param>
        /// <returns>True if the time stamp has been matched.</returns>
        static public bool MatchDateTimeStamp( this StringMatcher @this, out DateTimeStamp time )
        {
            time = DateTimeStamp.Unknown;
            int savedIndex = @this.StartIndex;
            DateTime t;
            if( !@this.MatchFileNameUniqueTimeUtcFormat( out t ) ) return @this.SetError();
            byte uniquifier = 0;
            if( @this.MatchChar( '(' ) )
            {
                int unique;
                if( !@this.MatchInt32( out unique, 0, 255 ) && !@this.TryMatchChar( ')' ) ) return @this.BackwardSetError( savedIndex );
                uniquifier = (byte)unique;
            }
            time = new DateTimeStamp( t, uniquifier );
            return @this.Forward( 0 );
        }

    }
}
