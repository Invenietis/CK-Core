#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.Matcher.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// </summary>
	static public partial class Util
	{    
        /// <summary>
        /// Provides simple methods to that follows the Match and Forward pattern.
        /// The string must never be null.
        /// On success, the startAt index is updated so that is positioned after the match.
        /// The startAt index (where the match must start) can not be negative. It may be equal to or greater than maxLength: the match fails without throwing an exception.
        /// The maxLength can not be greater than the length of the string, but may be 0 or negative (the match fails).
        /// </summary>
        public static class Matcher
        {
            /// <summary>
            /// Checks standard arguments for Match and Forward pattern.
            /// </summary>
            /// <param name="s">The string. Can not be null.</param>
            /// <param name="startAt">Can not be negative.</param>
            /// <param name="maxLength">Can not be greater than the length of the string, but may be 0 or negative (the match fails)</param>
            /// <returns>False if startAt is greater or equal to maxLength. True if the match is possible.</returns>
            public static bool CheckMatchArguments( string s, int startAt, int maxLength )
            {
                if( s == null ) throw new ArgumentNullException( "s" );
                if( startAt < 0 ) throw new ArgumentOutOfRangeException( "startAt" );
                if( maxLength > s.Length ) throw new ArgumentException( "maxLength" );
                return startAt < maxLength;
            }
            
            /// <summary>
            /// Matches an exact single character at a given position in a string.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <param name="startAt">
            /// Index where the match must start (can be equal to or greater than <paramref name="maxLength"/>: the match fails).
            /// On success, index of the end of the match.
            /// </param>
            /// <param name="maxLength">
            /// Maximum index to consider in the string (it shortens the default <see cref="String.Length"/>), it can be zero or negative.
            /// If maxLength is greater than String.Length an <see cref="ArgumentException"/> is thrown.
            /// </param>
            /// <param name="match">The character that must match.</param>
            /// <returns>True on success, false if the match failed.</returns>
            public static bool Match( string s, ref int startAt, int maxLength, char match )
            {
                if( !CheckMatchArguments( s, startAt, maxLength ) ) return false;
                if( s[startAt] == match )
                {
                    ++startAt;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Matches a string at a given position in a string.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <param name="startAt">
            /// Index where the match must start (can be equal to or greater than <paramref name="maxLength"/>: the match fails).
            /// On success, index of the end of the match.
            /// </param>
            /// <param name="match">The string that must match. Can not be null.</param>
            /// <param name="maxLength">
            /// Maximum index to consider in the string (it shortens the default <see cref="String.Length"/>), it can be zero or negative.
            /// If maxLength is greater than String.Length an <see cref="ArgumentException"/> is thrown.
            /// </param>
            /// <param name="comparisonType">Specifies the culture, case, and sort rules.</param>
            /// <returns>True on success, false if the match failed.</returns>
            public static bool Match( string s, ref int startAt, int maxLength, string match, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
            {
                if( match == null ) throw new ArgumentNullException( "match" );
                if( !CheckMatchArguments( s, startAt, maxLength ) ) return false;
                
                int len = match.Length;
                if( startAt + len > maxLength || String.Compare( s, startAt, match, 0, len, comparisonType ) != 0 ) return false;
                startAt += len;
                return true;
            }

            /// <summary>
            /// Matches a sequence of white spaces.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <param name="startAt">
            /// Index where the match must start (can be equal to or greater than the length of the string: the match fails).
            /// On success, index of the end of the match.
            /// </param>
            /// <param name="maxLength">
            /// Maximum index to consider in the string (it shortens the default <see cref="String.Length"/>), it can be zero or negative.
            /// If maxLength is greater than String.Length an <see cref="ArgumentException"/> is thrown.
            /// </param>
            /// <returns>True on success, false if the match failed.</returns>
            public static bool MatchWhiteSpaces( string s, ref int startAt, int maxLength )
            {
                if( !CheckMatchArguments( s, startAt, maxLength ) ) return false;
                int i = startAt;
                while( i != maxLength && Char.IsWhiteSpace( s, i ) ) ++i;
                bool forwarded = i > startAt;
                startAt = i;
                return forwarded;
            }

            /// <summary>
            /// Directly calls <see cref="FileUtil.MatchFileNameUniqueTimeUtcFormat"/>.
            /// </summary>
            /// <param name="s">The string to match.</param>
            /// <param name="startAt">
            /// Index where the match must start (can be equal to or greater than the length of the string: the match fails).
            /// On success, index of the end of the match.
            /// </param>
            /// <param name="maxLength">
            /// Maximum index to consider in the string (it can shorten the default <see cref="String.Length"/> if 
            /// set to a positive value, otherwise it is set to String.Length).
            /// If maxLength is greater than String.Length an <see cref="ArgumentException"/> is thrown.
            /// </param>
            /// <param name="time">Result time.</param>
            /// <returns>True if the time has been matched.</returns>
            [ExcludeFromCodeCoverage]
            public static bool MatchFileNameUniqueTimeUtcFormat( string s, ref int startAt, int maxLength, out DateTime time )
            {
                return FileUtil.MatchFileNameUniqueTimeUtcFormat( s, ref startAt, maxLength, time: out time );
            }

        }

    }
}
