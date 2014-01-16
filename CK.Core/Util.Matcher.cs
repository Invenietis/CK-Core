#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.Hash.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// </summary>
	static public partial class Util
	{    
        /// <summary>
        /// Provides simple methods to that follows the Match and Forward pattern.
        /// The startAt index where the match must start can be equal to or greater than the length of the string: the match fails without throwing an exception.
        /// On success, the index is updated so that is positioned after the match.
        /// </summary>
        public static class Matcher
        {
            /// <summary>
            /// Matches a string at a given position in a string.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <param name="startAt">
            /// Index where the match must start (can be equal to or greater than the length of the string: the match fails).
            /// On success, index of the end of the match.
            /// </param>
            /// <param name="match">The string that must match.</param>
            /// <param name="comparisonType">Specifies the culture, case, and sort rules.</param>
            /// <returns>True on success, false if the match failed.</returns>
            public static bool Match( string s, ref int startAt, string match, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
            {
                if( match == null ) throw new ArgumentNullException( "match" );
                int len = match.Length;
                if( startAt >= s.Length || String.Compare( s, startAt, match, 0, len, comparisonType ) != 0 ) return false;
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
            /// <returns>True on success, false if the match failed.</returns>
            public static bool MatchWhiteSpaces( string s, ref int startAt )
            {
                if( s == null ) throw new ArgumentNullException( "s" );
                if( startAt >= s.Length ) return false;
                int i = startAt;
                while( i != s.Length && Char.IsWhiteSpace( s, i ) ) ++i;
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
            /// <param name="time">Result time.</param>
            /// <returns>True if the time has been matched.</returns>
            public static bool MatchFileNameUniqueTimeUtcFormat( string s, ref int startAt, out DateTime time )
            {
                return FileUtil.MatchFileNameUniqueTimeUtcFormat( s, ref startAt, out time );
            }

        }

    }
}
