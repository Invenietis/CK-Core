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
        /// Provides methods to combine hash values: use <see cref="StartValue"/> and then 
        /// chain calls to the <see cref="M:Combine"/> methods.
        /// Based on Daniel J. Bernstein algorithm (http://cr.yp.to/cdb/cdb.txt).
        /// </summary>
        public static class Hash
        {

            /// <summary>
            /// Gets a very classical start value (see remarks) that can be then be used 
            /// by the multiple <see cref="M:Combine"/> methods. Use <see cref="Int64.GetHashCode"/> to
            /// obtain a final integer (Int32) hash code.
            /// </summary>
            /// <remarks>
            /// It seems that this value has nothing special (mathematically speaking) except that it 
            /// has been used and reused by many people since DJB choose it.
            ///</remarks>
            public static Int64 StartValue { get { return 5381; } }

            /// <summary>
            /// Combines an existing hash value with a new one.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="value">Value to combine.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, int value )
            {
                return ((hash << 5) + hash) ^ value;
            }

            /// <summary>
            /// Combines an existing hash value with an object's hash (object can be null).
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="o">Object whose hash must be combined (can be null).</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, object o )
            {
                return Combine( hash, o != null ? o.GetHashCode() : 0 );
            }

            /// <summary>
            /// Combines an existing hash value with multiples object's hash.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="c">Multiple objects. Can be null.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, IEnumerable c )
            {
                int nb = 0;
                if( c != null )
                {
                    foreach( object o in c )
                    {
                        hash = Combine( hash, o );
                        nb++;
                    }
                }
                return Combine( hash, nb );
            }

            /// <summary>
            /// Combines an existing hash value with multiples object's written directly as parameters.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="objects">Multiple objects.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, params object[] objects )
            {
                return Combine( hash, (IEnumerable)objects );
            }

        }

    }
}
