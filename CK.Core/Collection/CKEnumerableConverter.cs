#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKEnumerableConverter.cs) is part of CiviKey. 
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
	/// Wraps a <see cref="IEnumerable"/> of a <typeparamref name="TOuter"/> type around a <see cref="IEnumerable"/>
	/// of <typeparamref name="TOuter"/> thanks to a conversion function.
	/// </summary>
	/// <typeparam name="TOuter">Type of the object that must be exposed.</typeparam>
	/// <typeparam name="TInner">Actual type of enumerated objects.</typeparam>
    public class CKEnumerableConverter<TOuter, TInner> : IEnumerable<TOuter>
    {
        readonly Converter<TInner,TOuter> _converter;
        readonly IEnumerable<TInner> _inner;

		/// <summary>
		/// Initializes a new adapter.
		/// </summary>
		/// <param name="c">Enumerable to wrap.</param>
        /// <param name="converter">Conversion function to apply.</param>
        public CKEnumerableConverter( IEnumerable<TInner> c, Converter<TInner,TOuter> converter )
        {
            if( c == null ) throw new ArgumentNullException();
            if( c == this ) throw new ArgumentException( "Adapter plugged on itself." );
            _inner = c;
            _converter = converter;
		}

        /// <summary>
        /// Gets the wrapped enumerable.
        /// </summary>
        public IEnumerable<TInner> Inner { get { return _inner; } }

        /// <summary>
        /// Gets the converter associated to this <see cref="CKEnumerableConverter{TOuter,TInner}"/>.
        /// </summary>
        public Converter<TInner, TOuter> Converter
        {
            get { return _converter; }
        }

		/// <summary>
		/// Internal implementation of the enumerator.
		/// </summary>
		internal sealed class EnumeratorAdapter : IEnumerator<TOuter>
		{
            Converter<TInner,TOuter> _converter;
			IEnumerator<TInner> _enumerator;

            internal EnumeratorAdapter( IEnumerator<TInner> e, Converter<TInner, TOuter> converter )
			{
				_enumerator = e;
                _converter = converter;
			}

			public TOuter Current
			{
				get { return _converter( _enumerator.Current ); }
			}

			public void Dispose()
			{
				_enumerator.Dispose();
			}

			object IEnumerator.Current
			{
                get { return _converter( _enumerator.Current ); }
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}

		}

		/// <summary>
		/// Returns an enumerator that iterates through this enumerable.
		/// </summary>
		/// <returns>A IEnumerator that can be used to iterate through this enumerable.</returns>
        public IEnumerator<TOuter> GetEnumerator()
        {
            return new EnumeratorAdapter( Inner.GetEnumerator(), _converter );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

    }

}
