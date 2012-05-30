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
        /// Offers converter functions.
        /// </summary>
        static public class Converter
        {
            /// <summary>
            /// Converts an array of bytes to an hexadecimal string.
            /// </summary>
            /// <param name="bytes">A non null array of bytes.</param>
            /// <param name="zeroxPrefix">False to not prefix the result with 0x.</param>
            /// <returns>The bytes expressed as a an hexadecimal string.</returns>
            public static string BytesToString( byte[] bytes, bool zeroxPrefix = true )
            {
                if( bytes == null ) throw new ArgumentNullException();
                int len = zeroxPrefix ? (1 + bytes.Length) * 2 : bytes.Length * 2;
                char[] r = new Char[len];
                int j = -1;
                if( zeroxPrefix )
                {
                    r[0] = '0';
                    r[j = 1] = 'x';
                }
                int i = 0;
                while( --len > 0 )
                {
                    byte b = bytes[i++];
                    r[++j] = _hexChars[(b >> 4) & 0x0F];
                    r[++j] = _hexChars[b & 0x0F];
                }
                return new String( r );
            }
        }
    }
}
