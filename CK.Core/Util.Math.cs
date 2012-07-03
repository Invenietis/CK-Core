#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.Math.cs) is part of CiviKey. 
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
        /// Offers simple mathematic functions.
        /// </summary>
        static public class Math
        {
            static int[] _multiplyDeBruijnBitPosition = 
				{ 
					0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
					31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 
				};


            /// <summary>
            /// Compute the Log2 (logarithm base 2) of a given number.
            /// </summary>
            /// <param name="v">Integer to compute</param>
            /// <returns>Log2 of the given integer</returns>
            [CLSCompliant( false )]
            static public int Log2( UInt32 v )
            {
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                v = (v >> 1) + 1;
                return _multiplyDeBruijnBitPosition[((v * 0x077CB531U)) >> 27];
            }

            /// <summary>
            /// Compute the Log2ForPower2 (logarithm base 2 power 2) of a given number.
            /// </summary>
            /// <param name="v">Integer to compute. It MUST be a power of 2.</param>
            /// <returns>Result</returns>
            [CLSCompliant( false )]
            static public int Log2ForPower2( UInt32 v )
            {
                return _multiplyDeBruijnBitPosition[(((uint)v * 0x077CB531U)) >> 27];
            }

            /// <summary>
            /// Counts the number of bits in the given byte.
            /// </summary>
            /// <param name="v">The value for which number of bits must be computed.</param>
            /// <returns>The number of bits.</returns>
            static public int BitCount( Byte v )
            {
                return (int)((v * 0x200040008001 & 0x111111111111111) % 0xF);
            }
        }        
    }
}
