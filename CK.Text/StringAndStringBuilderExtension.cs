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
using System.Threading.Tasks;

namespace CK.Text
{
    /// <summary>
    /// Defines useful extension methods on string and StringBuilder.
    /// </summary>
    public static class StringAndStringBuilderExtension
    {
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

    }
}
