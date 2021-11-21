using System;
using System.Collections.Generic;
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
            id = Guid.Empty;
            if( @this.Length < 32 ) return false;
            if( @this.Head == '{' )
            {
                // Form "B" or "X".
                if( @this.Length < 38 ) return false;
                if( @this.Text[@this.StartIndex+37] == '}' )
                {
                    // The "B" form.
                    if( Guid.TryParseExact( @this.Text.Substring( @this.StartIndex, 38 ), "B", out id ) )
                    {
                        return @this.UncheckedMove( 38 );
                    }
                    return false;
                }
                // The "X" form.
                if( @this.Length >= 68  && Guid.TryParseExact( @this.Text.Substring( @this.StartIndex, 68 ), "X", out id ) )
                {
                    return @this.UncheckedMove( 68 );
                }
                return false;
            }
            if( @this.Head == '(' )
            {
                // Can only be the "P" form.
                if( @this.Length >= 38 && Guid.TryParseExact( @this.Text.Substring( @this.StartIndex, 38 ), "P", out id ) )
                {
                    return @this.UncheckedMove( 38 );
                }
                return false;
            }
            if( @this.Head.HexDigitValue() >= 0 )
            {
                // The "N" or "D" form.
                if( @this.Length >= 36 && @this.Text[@this.StartIndex + 8] == '-' )
                {
                    // The ""D" form.
                    if( Guid.TryParseExact( @this.Text.Substring( @this.StartIndex, 36 ), "D", out id ) )
                    {
                        return @this.UncheckedMove( 36 );
                    }
                    return false;
                }
                if( Guid.TryParseExact( @this.Text.Substring( @this.StartIndex, 32 ), "N", out id ) )
                {
                    return @this.UncheckedMove( 32 );
                }
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
            if( minDigit <= 0 ) throw new ArgumentException( "Must be at least 1 digit.", nameof( minDigit ) );
            if( maxDigit > 16 ) throw new ArgumentException( "Must be at most 16 digits.", nameof( maxDigit ) );
            if( minDigit > maxDigit ) throw new ArgumentException( "Must be smaller than maxDigit.", nameof( minDigit ) );
            value = 0;
            if( @this.IsEnd ) return false;
            int i = @this.StartIndex;
            int len = @this.Length;
            while( --maxDigit >= 0 && --len >= 0 )
            {
                int cN = @this.Head.HexDigitValue();
                if( cN < 0 || cN > 15 ) break;
                @this.UncheckedMove( 1 );
                value <<= 4;
                value |= (uint)cN;
            }
            if( (@this.StartIndex - i) >= minDigit ) return true;
            @this.UncheckedMove( i - @this.StartIndex );
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
            i = 0;
            int savedIndex = @this.StartIndex;
            int value = 0;
            bool signed;
            if( @this.IsEnd ) return @this.SetError();
            if( (signed = @this.TryMatchChar( '-' )) && @this.IsEnd ) return @this.BackwardAddError( savedIndex );

            char c;
            if( @this.TryMatchChar( '0' ) )
            {
                if( !@this.IsEnd && (c = @this.Head) >= '0' && c <= '9' ) return @this.BackwardAddError( savedIndex, "0...9" );
                return @this.ClearError();
            }
            unchecked
            {
                long iMax = Int32.MaxValue;
                if( signed ) iMax++;
                while( !@this.IsEnd && (c = @this.Head) >= '0' && c <= '9' )
                {
                    value = value * 10 + (c - '0');
                    if( value > iMax ) break;
                    @this.UncheckedMove( 1 );
                }
            }
            if( @this.StartIndex > savedIndex )
            {
                if( signed ) value = -value;
                if( value < minValue || value > maxValue )
                {
                    return @this.BackwardAddError( savedIndex, String.Format( CultureInfo.InvariantCulture, "value between {0} and {1}", minValue, maxValue ) );
                }
                i = (int)value;
                return @this.ClearError();
            }
            return @this.SetError();
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
            Match m = RegexDouble.Match( @this.Text, @this.StartIndex, @this.Length );
            if( !m.Success ) return false;
            return @this.UncheckedMove( m.Length );
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
            Match m = RegexDouble.Match( @this.Text, @this.StartIndex, @this.Length );
            if( !m.Success )
            {
                value = 0;
                return false;
            }
            if( !double.TryParse( @this.Text.AsSpan( @this.StartIndex, m.Length ), NumberStyles.Float, CultureInfo.InvariantCulture, out value ) ) return false;
            return @this.UncheckedMove( m.Length );
        }
    }
}
