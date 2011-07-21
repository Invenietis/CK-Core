#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\ReadOnlyListTypeAdapter.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections.Generic;
using System;

namespace CK.Core
{
    /// <summary>
    /// Will be obsolete in .Net framework 4.0 (this is only here to support covariance).
    /// </summary>
    /// <typeparam name="TOuter">Type of the object that must be exposed.</typeparam>
    /// <typeparam name="TInner">Actual type of the objects contained in the <see cref="Inner"/> collection.</typeparam>
    public class ReadOnlyListTypeAdapter<TOuter, TInner> : ReadOnlyCollectionTypeAdapter<TOuter, TInner>, IReadOnlyList<TOuter>
        where TInner : TOuter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c"></param>
        public ReadOnlyListTypeAdapter( IList<TInner> c )
            : base(c)
        {
        }

        /// <summary>
        /// Wrapped list.
        /// </summary>
        public new IList<TInner> Inner
        {
            get { return (IList<TInner>)base.Inner; }
        }

        /// <summary>
        /// Gets an item index.
        /// </summary>
        /// <param name="item">Item to challenge.</param>
        /// <returns>-1 if the item is not contained in the collection.</returns>
        public int IndexOf( object item )
        {
            return item is TInner ? Inner.IndexOf( (TInner)item ) : -1;
        }

        /// <summary>
        /// Gets the element by index.
        /// </summary>
        /// <param name="i">Index of the element.</param>
        /// <returns>The element at index i.</returns>
        public TOuter this[ int i ]
        {
            get { return (TOuter)Inner[i]; }
        }

    }

    /// <summary>
    /// Obsolete, use <see cref="ReadOnlyListTypeAdapter{TOuter, TInner}"/> instead.
    /// </summary>
    /// <typeparam name="TOuter"></typeparam>
    /// <typeparam name="TInner"></typeparam>
    [Obsolete( "Use ReadOnlyListTypeAdapter instead.", true )]
    public class ReadOnlyListAdapter<TOuter, TInner> : ReadOnlyListTypeAdapter<TOuter, TInner>
        where TInner : TOuter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c"></param>
        public ReadOnlyListAdapter( IList<TInner> c )
            : base(c)
        {
        }
    }
}

