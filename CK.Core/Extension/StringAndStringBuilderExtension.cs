using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Defines useful extension methods on string and StringBuilder.
    /// </summary>
    public static class StringAndStringBuilderExtension
    {
        static readonly bool _isCRLF = Environment.NewLine == "\r\n";

        /// <summary>
        /// Concatenates multiple strings with an internal separator.
        /// </summary>
        /// <param name="this">Set of strings.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string?> @this, string separator = ", " ) => String.Join( separator, @this );

        /// <summary>
        /// Concatenates multiple strings with an internal character separator.
        /// </summary>
        /// <param name="this">Set of strings.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string?> @this, char separator ) => String.Join( separator, @this );

        /// <summary>
        /// Concatenates multiple chars with an internal separator.
        /// </summary>
        /// <param name="this">Set of chars.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<char> @this, string separator ) => String.Join( separator, @this );

        /// <summary>
        /// Concatenates multiple chars without any internal separator.
        /// </summary>
        /// <param name="this">Set of chars.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<char> @this ) => @this.Concatenate( string.Empty );

        /// <summary>
        /// Concatenates multiple chars with an internal character separator.
        /// </summary>
        /// <param name="this">Set of chars.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<char> @this, char separator ) => String.Join( separator, @this );

        /// <summary>
        /// Appends a set of strings with an internal separator.
        /// (This should be named 'Append' but appropriate overload is not always detected by the compiler.)
        /// </summary>
        /// <param name="this">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="strings">Set of strings. Can be null.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The builder itself.</returns>
        public static StringBuilder AppendStrings( this StringBuilder @this, IEnumerable<string> strings, string separator = ", " )
        {
            if( strings != null )
            {
                using( var e = strings.GetEnumerator() )
                {
                    if( e != null && e.MoveNext() )
                    {
                        @this.Append( e.Current );
                        while( e.MoveNext() )
                        {
                            @this.Append( separator ).Append( e.Current );
                        }
                    }
                }
            }
            return @this;
        }

        /// <summary>
        /// Appends multiple time the same string.
        /// </summary>
        /// <param name="this">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="s">The string to repeat. Can be null.</param>
        /// <param name="repeatCount">The number of repetition.</param>
        /// <returns>The builder itself.</returns>
        public static StringBuilder Append( this StringBuilder @this, string s, int repeatCount )
        {
            while( --repeatCount >= 0 ) @this.Append( s );
            return @this;
        }

        /// <summary>
        /// Gets whether the <see cref="Environment.NewLine"/> is \r\n.
        /// Otherwise it is \n.
        /// </summary>
        public static bool IsCRLF => _isCRLF;

        /// <summary>
        /// Gets the value (0...15) for this character ('0'...'9', 'a'...'f' or 'A.'..'F'),
        /// or -1 if this is not an hexadecimal digit.
        /// </summary>
        /// <param name="c">This character.</param>
        /// <returns>The value for this character or -1 if this is not an hexadecimal digit.</returns>
        public static int HexDigitValue( this char c )
        {
            int cN = c - '0';
            if( cN >= 49 ) cN -= 39;
            else if( cN >= 17 ) cN -= 7;
            return cN >= 0 && cN < 16 ? cN : -1;
        }

        /// <summary>
        /// Appends a block of text to this StringBuilder with a prefix on each line.
        /// <c>b.AppendMultiLine( prefix, text, false, true )</c> is the same as the naive approach,
        /// that is to add <c>text.Replace( Environment.NewLine, Environment.NewLine + prefix )</c>.
        /// This method is faster (in release build), normalizes EOL (\n, \r and \r\n)
        /// to <see cref="Environment.NewLine"/> and offer a better and easier control with its
        /// parameters <paramref name="prefixOnFirstLine"/> and <paramref name="prefixLastEmptyLine"/>.
        /// </summary>
        /// <param name="this">This string builder.</param>
        /// <param name="prefix">The prefix to add. When null or empty, the text is appended as-is.</param>
        /// <param name="text">The multiple lines text to append. Can be null or empty.</param>
        /// <param name="prefixOnFirstLine">True to append the prefix to the first line. False to skip this first prefix.</param>
        /// <param name="prefixLastEmptyLine">
        /// True to append the prefix to the last empty line (when the text ends with an EOL).
        /// By default, when text ends with a EOL, it is ignored.
        /// </param>
        /// <returns></returns>
        public static StringBuilder AppendMultiLine( this StringBuilder @this,
                                                     string prefix,
                                                     string text,
                                                     bool prefixOnFirstLine,
                                                     bool prefixLastEmptyLine = false )
        {
            if( string.IsNullOrEmpty( prefix ) ) return @this.Append( text ?? string.Empty );
            if( string.IsNullOrEmpty( text ) ) return prefixOnFirstLine
                                                        ? @this.Append( prefix ?? string.Empty )
                                                        : @this;
            bool lastIsNewLine = false;
            int lenLine, prevIndex = 0, i = 0;
            while( i < text.Length )
            {
                char c = text[i];
                if( c == '\r' || c == '\n' )
                {
                    lenLine = i - prevIndex;
                    if( prevIndex > 0 || prefixOnFirstLine )
                    {
                        if( prevIndex > 0 ) @this.AppendLine();
                        @this.Append( prefix );
                    }
                    @this.Append( text, prevIndex, lenLine );
                    if( ++i < text.Length && c == '\r' && text[i] == '\n' ) ++i;
                    prevIndex = i;
                    lastIsNewLine = true;
                }
                else
                {
                    ++i;
                    lastIsNewLine = false;
                }
            }
            lenLine = i - prevIndex;
            if( lenLine > 0 || (lastIsNewLine && prefixLastEmptyLine) )
            {
                if( prevIndex == 0 )
                {
                    if( prefixOnFirstLine ) @this.Append( prefix );
                    @this.Append( text );
                }
                else
                {
                    @this.AppendLine();
                    @this.Append( prefix ).Append( text, prevIndex, lenLine );
                }
            }
            return @this;
        }

    }
}

