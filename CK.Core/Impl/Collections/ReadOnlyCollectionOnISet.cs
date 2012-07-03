#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\ReadOnlyCollectionOnISet.cs) is part of CiviKey. 
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
using System.Collections;
using System;
using System.Diagnostics;
using System.Runtime;

namespace CK.Core
{
	/// <summary>
	/// Adapts a <see cref="HashSet{T}"/> object to the <see cref="IReadOnlyCollection{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">Type of the element.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.ReadOnlyCollectionDebuggerView<> ) ), DebuggerDisplay( "Count = {Count}" )]
    public sealed class ReadOnlyCollectionOnISet<T> : IReadOnlyCollection<T>, ICollection<T>
    {
		ISet<T> _c;

		/// <summary>
        /// Initializes a new <see cref="ReadOnlyCollectionOnISet{T}"/> around a <see cref="ISet{T}"/>.
		/// </summary>
		/// <param name="c">Hash set to wrap.</param>
        [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
        public ReadOnlyCollectionOnISet( ISet<T> c )
        {
			_c = c;
        }

		/// <summary>
		/// Gets whether an item is contained or not.
		/// </summary>
		/// <param name="item">Item to challenge.</param>
		/// <returns>True if the item is contained in the collection.</returns>
        [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
        public bool Contains( object item )
        {
            return item is T ? _c.Contains( (T)item ) : false;
        }

		/// <summary>
		/// Gets the number of items of the collection.
		/// </summary>
        public int Count
        {
            get { return _c.Count; }
        }

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
        [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
        public IEnumerator<T> GetEnumerator()
		{
			return _c.GetEnumerator();
		}

        [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
        IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


        #region ICollection<T> Members

        void ICollection<T>.Add( T item )
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains( T item )
        {
            return _c.Contains( item );
        }

        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
        {
            _c.CopyTo( array, arrayIndex );
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove( T item )
        {
            throw new NotSupportedException();
        }

        #endregion

    }
}
