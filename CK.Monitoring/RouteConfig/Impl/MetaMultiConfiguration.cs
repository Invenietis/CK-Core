#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\MetaMultiConfiguration.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Base class for meta configuration object that handles one or more items.
    /// </summary>
    public abstract class MetaMultiConfiguration<T> : MetaConfiguration
    {
        readonly List<T> _items;

        /// <summary>
        /// Initializes a configuration with at least one item.
        /// </summary>
        /// <param name="first">First and required item.</param>
        /// <param name="other">Optional multiple items.</param>
        public MetaMultiConfiguration( T first, params T[] other )
        {
            _items = new List<T>();
            if( first != null ) _items.Add( first );
            _items.AddRange( other.Where( i => i != null ) );
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        protected IReadOnlyList<T> Items
        {
            get { return _items.AsReadOnlyList(); }
        }

        /// <summary>
        /// Adds a new item.
        /// </summary>
        /// <param name="item">Item to add.</param>
        protected void Add( T item )
        {
            if( item != null ) _items.Add( item );
        }

    }
}
