using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Extends <see cref="StringMatcher"/> with useful (yet basic) methods.
    /// </summary>
    public static class StringMatcherTextExtension
    {
        /// <summary>
        /// Matches a Guid. No error is set if match fails.
        /// </summary>
        /// <remarks>
        /// Any of the 5 forms of Guid can be matched:
        /// <list type="table">
        /// <item><term>N</term><description>00000000000000000000000000000000</description></item>
        /// <item><term>D</term><description>00000000-0000-0000-0000-000000000000</description></item>
        /// <item><term>B</term><description>{00000000-0000-0000-0000-000000000000}</description></item>
        /// <item><term>P</term><description>(00000000-0000-0000-0000-000000000000)</description></item>
        /// <item><term>X</term><description>{0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}</description></item>
        /// </list>
        /// </remarks>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="id">The result Guid. <see cref="Guid.Empty"/> on failure.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchGuid( this StringMatcher @this, out Guid id )
        {
            var ro = @this.ROSpan;
            if( ro.TryMatchGuid( out id ) )
            {
                @this.UncheckedMove( @this.ROSpan.Length - ro.Length );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Matches hexadecimal values: between 1 and 16 0-9, A-F or a-f digits.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">Resulting value on success.</param>
        /// <param name="minDigit">Minimal digit count. Must be between 1 and 16 and smaller or equal to <paramref name="maxDigit"/>.</param>
        /// <param name="maxDigit">Maximal digit count. Must be between 1 and 16.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchHexNumber( this StringMatcher @this, out ulong value, int minDigit = 1, int maxDigit = 16 )
        {
            var ro = @this.ROSpan;
            if( ro.TryMatchHexNumber( out value, minDigit, maxDigit ) )
            {
                @this.UncheckedMove( @this.ROSpan.Length - ro.Length );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Matches Int32 values that must not start with '0' ('0' is valid but '0d', where d is any digit, is not).
        /// A signed integer starts with a '-'. '-0' is valid but '-0d' (where d is any digit) is not.
        /// If the value is too big for an Int32, it fails.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="i">The result integer. 0 on failure.</param>
        /// <param name="minValue">Optional minimal value.</param>
        /// <param name="maxValue">Optional maximal value.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool MatchInt32( this StringMatcher @this, out int i, int minValue = int.MinValue, int maxValue = int.MaxValue )
        {
            var m = new ROSpanCharMatcher( @this.ROSpan );
            if( m.TryMatchInt32( out i, false, minValue, maxValue ) )
            {
                @this.UncheckedMove( @this.ROSpan.Length - m.Head.Length );
                return true;
            }
            Debug.Assert( m.HasError );
            @this.SetError( m.GetErrors().Single().Expectation );
            return false;
        }


        /// <summary>
        /// The <see cref="Regex"/> that <see cref="TryMatchDoubleValue(StringMatcher)"/> uses to avoid
        /// calling <see cref="double.TryParse(string, out double)"/> when resolving the value is 
        /// useless.
        /// Note that this regex allow a leading minus (-) sign, but not a plus (+).
        /// </summary>
        static public readonly Regex RegexDouble = new Regex( @"^-?(0|[1-9][0-9]*)(\.[0-9]+)?((e|E)(\+|-)?[0-9]+)?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        /// <summary>
        /// Matches a double without getting its value nor setting an error if match fails.
        /// This uses <see cref="RegexDouble"/>.
        /// The text may start with a minus (-) but not with a plus (+).
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchDoubleValue( this StringMatcher @this )
        {
            var ro = @this.ROSpan;
            if( ro.TrySkipDouble() )
            {
                @this.UncheckedMove( @this.ROSpan.Length - ro.Length );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Matches a double and gets its value. No error is set if match fails.
        /// The text may start with a minus (-) but not with a plus (+).
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">The read value on success.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchDoubleValue( this StringMatcher @this, out double value )
        {
            var ro = @this.ROSpan;
            if( ro.TryMatchDouble( out value ) )
            {
                @this.UncheckedMove( @this.ROSpan.Length - ro.Length );
                return true;
            }
            return false;
        }
    }
}
