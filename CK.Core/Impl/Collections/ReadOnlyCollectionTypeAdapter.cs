#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\ReadOnlyCollectionTypeAdapter.cs) is part of CiviKey. 
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
	/// Wraps a <see cref="IReadOnlyCollection{T}"/> of a <typeparamref name="TOuter"/> type around a <see cref="ICollection{T}"/>
	/// of <typeparamref name="TInner"/> (the <see cref="Inner"/> collection).
    /// Will be obsolete in .Net framework 4.0 (this is only here to support covariance).
	/// </summary>
	/// <typeparam name="TOuter">Type of the object that must be exposed.</typeparam>
	/// <typeparam name="TInner">Actual type of the objects contained in the <see cref="Inner"/> collection.</typeparam>
	public class ReadOnlyCollectionTypeAdapter<TOuter, TInner> : EnumerableAdapter<TOuter, TInner>, IReadOnlyCollection<TOuter>
        where TInner : TOuter
    {
		/// <summary>
        /// Initializes a new <see cref="ReadOnlyCollectionAdapter{TOuter,TInner}"/> around a <see cref="ICollection{TInner}"/>.
		/// </summary>
		/// <param name="c">Collection to wrap.</param>
        public ReadOnlyCollectionTypeAdapter( ICollection<TInner> c )
            : base(c)
        {
        }

		/// <summary>
		/// Wrapped collection.
		/// </summary>
        public new ICollection<TInner> Inner
        {
            get { return (ICollection<TInner>)base.Inner; }
        }

		/// <summary>
		/// Gets whether an item is contained or not.
		/// </summary>
		/// <param name="item">Item to challenge.</param>
		/// <returns>True if the item is contained in the collection.</returns>
        public bool Contains( object item )
        {
            return item is TInner ? Inner.Contains( (TInner)item ) : false;
        }

		/// <summary>
		/// Gets the number of items of the collection.
		/// </summary>
        public int Count
        {
            get { return Inner.Count; }
        }

    }

    /// <summary>
    /// Obsolete, use <see cref="ReadOnlyCollectionTypeAdapter{TOuter, TInner}"/> instead.
    /// </summary>
    /// <typeparam name="TOuter"></typeparam>
    /// <typeparam name="TInner"></typeparam>
    [Obsolete( "Use ReadOnlyCollectionTypeAdapter instead.", true )]
    public class ReadOnlyCollectionAdapter<TOuter, TInner> : ReadOnlyCollectionTypeAdapter<TOuter, TInner>
        where TInner : TOuter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c"></param>
        public ReadOnlyCollectionAdapter( ICollection<TInner> c )
            : base(c)
        {
        }
    }

}
