#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\EnumerableAdapter.cs) is part of CiviKey. 
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace CK.Core
{

	/// <summary>
	/// Wraps a <see cref="IEnumerable"/> of a <typeparamref name="TOuter"/> type around a <see cref="IEnumerable"/>
	/// of <typeparamref name="TInner"/> where the latter is a specialization of the former.
    /// This is a special case of the <see cref="EnumerableConverter{TOuter,TInner}"/> where the conversion function is a simple cast
    /// between the two types.
	/// </summary>
	/// <typeparam name="TOuter">Type of the object that must be exposed.</typeparam>
	/// <typeparam name="TInner">Actual type of enumerated objects.</typeparam>
	public class EnumerableAdapter<TOuter, TInner> : Wrapper<IEnumerable<TInner>>, IEnumerable<TOuter>
        where TInner : TOuter
    {
		/// <summary>
		/// Initializes a new adapter.
		/// </summary>
		/// <param name="c">Enumerable to wrap.</param>
        public EnumerableAdapter( IEnumerable<TInner> c )
            : base( c )
        {
            if( c == this ) throw new ArgumentException( "Adapter plugged on itself." );
        }

		/// <summary>
		/// Internal implementation of the enumerator.
		/// </summary>
		internal sealed class EnumeratorAdapter : IEnumerator<TOuter>
		{
			IEnumerator<TInner> _enumerator;

			internal EnumeratorAdapter(IEnumerator<TInner> e)
			{
				_enumerator = e;
			}

			public TOuter Current
			{
				get { return (TOuter)_enumerator.Current; }
			}

			public void Dispose()
			{
				_enumerator.Dispose();
			}

			object IEnumerator.Current
			{
				get { return _enumerator.Current; }
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
            return new EnumeratorAdapter(Inner.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

    }

}
