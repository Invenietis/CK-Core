#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\Wrapper.cs) is part of CiviKey. 
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace CK.Core
{
	/// <summary>
	/// Simple wrapper: <see cref="Inner"/> property is the wrapped object.
	/// </summary>
	/// <typeparam name="T">Type of wrapped object.</typeparam>
	public class Wrapper<T> 
    {
        T _inner;

		/// <summary>
		/// Initializes a wrapper around an object.
		/// </summary>
		/// <param name="inner">Wrapped object.</param>
        public Wrapper(T inner)
        {
            if( inner == null ) throw new ArgumentNullException();
            _inner = inner;
        }

		/// <summary>
		/// Gets the wrapped object.
		/// </summary>
        public T Inner { get { return _inner; } }

        /// <summary>
        /// Creates a <see cref="IEnumerator"/> on a <see cref="IEnumerable"/> of another type.
        /// </summary>
        /// <typeparam name="TInner">Actual entity.</typeparam>
        /// <param name="e">Source enumerable.</param>
        /// <param name="converter">Converter from <typeparamref name="TInner"/> to <typeparamref name="T"/>.</param>
        /// <returns>An enumerator for the more abstract type.</returns>
        static public IEnumerator<T> CreateEnumerator<TInner>( IEnumerable<TInner> e, Converter<TInner, T> converter )
        {
            return new EnumerableConverter<T, TInner>.EnumeratorAdapter( e.GetEnumerator(), converter );
        }

     }

}
