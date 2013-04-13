#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyCollectionTypeConverter.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace CK.Core
{
	/// <summary>
	/// Wraps a <see cref="IReadOnlyCollection{T}"/> of a <typeparamref name="TOuter"/> type around a <see cref="ICollection{T}"/>
	/// of <typeparamref name="TInner"/> (the <see cref="Inner"/> collection).
    /// The converter from inner objects to outer objects is required (to expose the content). 
    /// An optional converter (outer to inner) enables O(1) <see cref="Contains"/> method if the inner collection
    /// supports O(1) <see cref="ICollection{TInner}.Contains"/> method (this is the case of dictionary Keys collection).
	/// </summary>
	/// <typeparam name="TOuter">Type of the object that must be exposed.</typeparam>
	/// <typeparam name="TInner">Actual type of the objects contained in the <see cref="Inner"/> collection.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.ReadOnlyCollectionDebuggerView<,> ) ), DebuggerDisplay( "Count = {Count}" )]
    public class CKReadOnlyCollectionTypeConverter<TOuter, TInner> : CKEnumerableConverter<TOuter, TInner>, ICKReadOnlyCollection<TOuter>
    {
        Converter<TOuter, TInner> _outerToInner;

        /// <summary>
        /// Initializes a new <see cref="CKReadOnlyCollectionTypeConverter{TOuter,TInner}"/> around a <see cref="ICollection{TInner}"/>
        /// thanks to a <see cref="Converter{TInner,TOuter}"/> and an optional <see cref="Converter{TInner,TOuter}"/>.
        /// </summary>
        /// <param name="c">Collection to wrap.</param>
        /// <param name="innerToOuter">The converter function from <typeparamref name="TInner"/> to <typeparamref name="TOuter"/>.</param>
        /// <param name="outerToInner">
        /// Optional converter from <typeparamref name="TOuter"/> to <typeparamref name="TInner"/>. 
        /// When null, the <see cref="Contains"/> method is O(n) instead of O(1) if the inner collection is an index.
        /// </param>
        public CKReadOnlyCollectionTypeConverter( ICollection<TInner> c, Converter<TInner, TOuter> innerToOuter, Converter<TOuter, TInner> outerToInner )
            : base( c, innerToOuter )
        {
            _outerToInner = outerToInner;
        }

        /// <summary>
        /// Initializes a new <see cref="CKReadOnlyCollectionTypeConverter{TOuter,TInner}"/> around a <see cref="ICollection{TInner}"/>
        /// thanks to a <see cref="Converter{TInner,TOuter}"/>.
        /// </summary>
        /// <param name="c">Collection to wrap.</param>
        /// <param name="innerToOuter">The converter function from <typeparamref name="TInner"/> to <typeparamref name="TOuter"/>.</param>
        public CKReadOnlyCollectionTypeConverter( ICollection<TInner> c, Converter<TInner, TOuter> innerToOuter )
            : base( c, innerToOuter )
        {
        }

        /// <summary>
        /// Wrapped <see cref="ICollection{TInner}"/>.
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
            // If the object is not TOuter, this is necessarily false.
            if( item is TOuter )
            {
                TOuter o = (TOuter)item;
                // If we have a converter, use it to benefit of inner.Contains implementation.
                if( _outerToInner != null ) return Inner.Contains( _outerToInner( o ) );
                // If no converter is provided, takes the hard path.
                foreach( TInner i in Inner )
                {
                    if( o.Equals( Converter(i) ) ) return true;
                }
            }
            return false;
        }

		/// <summary>
		/// Gets the number of items of the collection.
		/// </summary>
        public int Count
        {
            get { return Inner.Count; }
        }

    }

}
