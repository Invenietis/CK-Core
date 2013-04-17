#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyCollectionDebuggerView.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core.Impl
{
    /// <summary>
    /// Debugger object for <see cref="IReadOnlyCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>

    [ExcludeFromCodeCoverage]
    sealed class CKReadOnlyCollectionDebuggerView<T>
    {
        readonly IReadOnlyCollection<T> _collection;
        
        /// <summary>
        /// Called by the debugger when needed.
        /// </summary>
        /// <param name="collection">The collection to debug.</param>
        public CKReadOnlyCollectionDebuggerView( IReadOnlyCollection<T> collection )
        {
            if( collection == null ) throw new ArgumentNullException( "collection" );
            _collection = collection;
        }

        /// <summary>
        /// Gets the items as a flattened array view.
        /// </summary>
        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
        public T[] Items
        {
            get
            {
                T[] a = new T[_collection.Count];
                int i = 0; 
                foreach( var e in _collection ) a[i++] = e;
                return a;
            }
        }
    }

    /// <summary>
    /// Debugger for adapters with two types (an exposed type and an inner type).
    /// </summary>
    /// <typeparam name="T">Type of the exposed element.</typeparam>
    /// <typeparam name="TInner">Type of the inner element.</typeparam>

    [ExcludeFromCodeCoverage]
    sealed class ReadOnlyCollectionDebuggerView<T, TInner>
    {
        readonly IReadOnlyCollection<T> _collection;

        /// <summary>
        /// Called by the debugger when needed.
        /// </summary>
        /// <param name="collection">The collection to debug.</param>
        public ReadOnlyCollectionDebuggerView( IReadOnlyCollection<T> collection )
        {
            if( collection == null ) throw new ArgumentNullException( "collection" );
            _collection = collection;
        }

        /// <summary>
        /// Gets the items as a flattened array view.
        /// </summary>
        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
        public T[] Items
        {
            get
            {
                T[] a = new T[_collection.Count];
                int i = 0;
                foreach( var e in _collection ) a[i++] = e;
                return a;
            }
        }

    }

}
