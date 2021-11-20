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
        /// Matches a JSON quoted string without setting an error if match fails.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="content">Extracted content.</param>
        /// <param name="allowNull">True to allow 'null'.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchJSONQuotedString( this StringMatcher @this, out string content, bool allowNull = false )
        {
            content = null;
            if( @this.IsEnd ) return false;
            int i = @this.StartIndex;
            if( @this.Text[i++] != '"' )
            {
                return allowNull && @this.TryMatchText( "null" );
            }
            int len = @this.Length - 1;
            StringBuilder b = null;
            while( len >= 0 )
            {
                if( len == 0 ) return false;
                char c = @this.Text[i++];
                --len;
                if( c == '"' ) break;
                if( c == '\\' )
                {
                    if( len == 0 ) return false;
                    if( b == null ) b = new StringBuilder( @this.Text, @this.StartIndex + 1, i - @this.StartIndex - 2, 1024 );
                    switch( (c = @this.Text[i++]) )
                    {
                        case 'r': c = '\r'; break;
                        case 'n': c = '\n'; break;
                        case 'b': c = '\b'; break;
                        case 't': c = '\t'; break;
                        case 'f': c = '\f'; break;
                        case 'u':
                            {
                                if( --len == 0 ) return false;
                                int cN;
                                cN = StringMatcherExtension.ReadHexDigit( @this.Text[i++] );
                                if( cN < 0 || cN > 15 ) return false;
                                int val = cN << 12;
                                if( --len == 0 ) return false;
                                cN = StringMatcherExtension.ReadHexDigit( @this.Text[i++] );
                                if( cN < 0 || cN > 15 ) return false;
                                val |= cN << 8;
                                if( --len == 0 ) return false;
                                cN = StringMatcherExtension.ReadHexDigit( @this.Text[i++] );
                                if( cN < 0 || cN > 15 ) return false;
                                val |= cN << 4;
                                if( --len == 0 ) return false;
                                cN = StringMatcherExtension.ReadHexDigit( @this.Text[i++] );
                                if( cN < 0 || cN > 15 ) return false;
                                val |= cN;
                                c = (char)val;
                                break;
                            }
                    }
                }
                if( b != null ) b.Append( c );
            }
            int lenS = i - @this.StartIndex;
            if( b != null ) content = b.ToString();
            else content = @this.Text.Substring( @this.StartIndex + 1, lenS - 2 );
            return @this.UncheckedMove( lenS );
        }


        /// <summary>
        /// Matches a quoted string without extracting its content.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="allowNull">True to allow 'null'.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public static bool TryMatchJSONQuotedString( this StringMatcher @this, bool allowNull = false )
        {
            if( @this.IsEnd ) return false;
            int i = @this.StartIndex;
            if( @this.Text[i++] != '"' )
            {
                return allowNull && @this.TryMatchText( "null" );
            }
            int len = @this.Length - 1;
            while( len >= 0 )
            {
                if( len == 0 ) return false;
                char c = @this.Text[i++];
                --len;
                if( c == '"' ) break;
                if( c == '\\' )
                {
                    i++;
                    --len;
                }
            }
            return @this.UncheckedMove( i - @this.StartIndex );
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
            if( !double.TryParse( @this.Text.Substring( @this.StartIndex, m.Length ), NumberStyles.Float, CultureInfo.InvariantCulture, out value ) ) return false;
            return @this.UncheckedMove( m.Length );
        }

        /// <summary>
        /// Matches a JSON terminal value: a "string", null, a number (double value), true or false.
        /// This method ignores the actual value and does not set any error if match fails.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <returns>True if a JSON value has been matched, false otherwise.</returns>
        public static bool TryMatchJSONTerminalValue( this StringMatcher @this )
        {
            return @this.TryMatchJSONQuotedString( true )
                    || @this.TryMatchDoubleValue()
                    || @this.TryMatchText( "true" )
                    || @this.TryMatchText( "false" );
        }

        /// <summary>
        /// Obsolete: renamed to TryMatchJSONTerminalValue.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">The parsed value. Can be null.</param>
        /// <returns>True if a JSON value has been matched, false otherwise.</returns>
        [Obsolete( "Use TryMatchJSONTerminalValue(@this, out value) instead." )]
        public static bool TryMatchJSONValue( this StringMatcher @this, out object value ) => TryMatchJSONTerminalValue( @this, out value );

        /// <summary>
        /// Matches a JSON terminal value: a "string", null, a number (double value), true or false.
        /// No error is set if match fails.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">The parsed value. Can be null.</param>
        /// <returns>True if a JSON value has been matched, false otherwise.</returns>
        public static bool TryMatchJSONTerminalValue( this StringMatcher @this, out object value )
        {
            string s;
            if( @this.TryMatchJSONQuotedString( out s, true ) )
            {
                value = s;
                return true;
            }
            double d;
            if( @this.TryMatchDoubleValue( out d ) )
            {
                value = d;
                return true;
            }
            if( @this.TryMatchText( "true" ) )
            {
                value = true;
                return true;
            }
            if( @this.TryMatchText( "false" ) )
            {
                value = false;
                return true;
            }
            value = null;
            return false;
        }
    }
}
