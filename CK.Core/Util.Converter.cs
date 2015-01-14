#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.Converter.cs) is part of CiviKey. 
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
        /// Offers converter functions.
        /// </summary>
        static public class Converter
        {
            /// <summary>
            /// '0'...'F' array. This is public for performance reasons.
            /// Obviously: do NOT modify it! 
            /// </summary>
            public static readonly char[] HexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            
            /// <summary>
            /// '0'...'f' array. This is public for performance reasons.
            /// Obviously: do NOT modify it! 
            /// </summary>
            public static readonly char[] HexCharsLower = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

            /// <summary>
            /// Converts an array of bytes to an hexadecimal string.
            /// </summary>
            /// <param name="bytes">A non null array of bytes.</param>
            /// <param name="zeroxPrefix">False to not prefix the result with 0x.</param>
            /// <param name="lowerCase">True to use upper case A...F (instead of a...f).</param>
            /// <returns>The bytes expressed as a an hexadecimal string.</returns>
            public static string BytesToHexaString( byte[] bytes, bool zeroxPrefix = true, bool lowerCase = false )
            {
                if( bytes == null ) throw new ArgumentNullException();
                int len = bytes.Length;
                char[] r = new Char[ 2 * (zeroxPrefix ? len+1 : len) ];
                int j = -1;
                if( zeroxPrefix )
                {
                    r[0] = '0';
                    r[j = 1] = 'x';
                }
                char[] chars = lowerCase ? HexCharsLower : HexChars;
                int i = 0;
                while( len-- > 0 )
                {
                    byte b = bytes[i++];
                    r[++j] = chars[(b >> 4) & 0x0F];
                    r[++j] = chars[b & 0x0F];
                }
                return new String( r );
            }

        }
    }
}
