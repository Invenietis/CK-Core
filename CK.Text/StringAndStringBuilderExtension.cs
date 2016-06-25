#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\StringAndStringBuilderExtension.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.Text
{
    /// <summary>
    /// Defines useful extension methods on string and StringBuilder.
    /// </summary>
    public static class StringAndStringBuilderExtension
    {

        class EOLNormalizer
        {
            public static readonly bool IsCRLF;
            static readonly Regex _rLFOnly;

            static EOLNormalizer()
            {
                IsCRLF = Environment.NewLine == "\r\n";
                _rLFOnly = new Regex( @"(?<!\r)\n", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled );
            }

            static public string ToEnvironment( string s )
            {
                return IsCRLF ? ToCRLF( s ) : ToLF( s );
            }

            static public string ToCRLF( string s )
            {
                return s != null ? _rLFOnly.Replace( s, "\r\n" ) : null;
            }

            static public string ToLF( string s )
            {
                return s != null ? s.Replace( "\r\n", "\n" ) : null;
            }
        }

        /// <summary>
        /// Concatenates multiple strings with an internal separator.
        /// </summary>
        /// <param name="this">Set of strings.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string> @this, string separator = ", " )
        {
            return new StringBuilder().AppendStrings( @this, separator ).ToString();
        }

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
        /// Returns a string where \r\n or \n alone are normalized to <see cref="Environment.NewLine"/>.
        /// </summary>
        /// <param name="this">This string.</param>
        /// <returns>Normalized string.</returns>
        public static string NormalizeEOL( this string @this )
        {
            return EOLNormalizer.ToEnvironment( @this );
        }

        /// <summary>
        /// Returns a string where \r\n are normalized to \n.
        /// </summary>
        /// <param name="this">This string.</param>
        /// <returns>Normalized string.</returns>
        public static string NormalizeEOLToLF( this string @this )
        {
            return EOLNormalizer.ToLF( @this );
        }

        /// <summary>
        /// Returns a string where \n alone are normalized to \r\n.
        /// </summary>
        /// <param name="this">This string.</param>
        /// <returns>Normalized string.</returns>
        public static string NormalizeEOLToCRLF( this string @this )
        {
            return EOLNormalizer.ToCRLF( @this );
        }

        /// <summary>
        /// Gets wether the <see cref="Environment.NewLine"/> is \r\n.
        /// Otherwise it is \n.
        /// </summary>
        public static bool IsCRLF => EOLNormalizer.IsCRLF;


        /// <summary>
        /// Gets the value (0...15) for this character ('0'...'9', 'a'...'f' or 'A.'..'F'),
        /// or -1 if this is not an hexadecimal digit.
        /// </summary>
        /// <param name="c">This character.</param>
        /// <returns>The value for this character.</returns>
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
