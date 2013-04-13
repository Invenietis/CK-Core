#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKEnumeratorEmpty.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Defines a unique empty enumerator.
    /// Use <see cref="CKReadOnlyListEmpty{T}.Empty"/> singleton for an empty <see cref="IEnumerable{T}"/>.
    /// </summary>
	public sealed class CKEnumeratorEmpty<T> : IEnumerator<T>
	{
        /// <summary>
        /// Gets the default <see cref="CKEnumeratorEmpty{T}"/>.
        /// This field is static readonly and is thread safe by design.
        /// </summary>
        public static readonly IEnumerator<T> Empty = new CKEnumeratorEmpty<T>();

        /// <summary>
        /// Gets the current element (the default value for the type of elements).
        /// </summary>
		public T Current
		{
			get { return default( T ); }
		}

        /// <summary>
        /// Dispose the enumerator.
        /// </summary>
		public void Dispose()
		{
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}

        /// <summary>
        /// Move to the next element of the enumerator.
        /// </summary>
        /// <returns></returns>
		public bool MoveNext()
		{
			return false;
		}

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
		public void Reset()
		{
		}
	}

}
