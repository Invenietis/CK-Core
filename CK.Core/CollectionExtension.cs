#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\CollectionExtension.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for collection &amp; list interfaces.
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">Collection items' type.</typeparam>
        /// <param name="c">This collection.</param>
        /// <param name="items">Multiple items to add. Can not be null.</param>
        public static void AddRange<T>( this ICollection<T> c, IEnumerable<T> items )
        {
            if( items == null ) throw new ArgumentNullException( "items" );
            foreach( var i in items ) c.Add( i );
        }

        /// <summary>
        /// Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">Collection items' type.</typeparam>
        /// <param name="c">This collection.</param>
        /// <param name="items">Items to add.</param>
        public static void AddRangeArray<T>( this ICollection<T> c, params T[] items )
        {
            foreach( var i in items ) c.Add( i );
        }

    }
}
