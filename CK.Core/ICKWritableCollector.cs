#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\IWritableCollector.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Contravariant interface for a collector: one can only add elements to a collector and know how muwh elements
    /// there are (Note that if you do not need the <see cref="Count"/>, you should use a simple Fun&lt;T,bool&gt;).
    /// </summary>
    /// <typeparam name="T">Base type for the elements of the collector.</typeparam>
    public interface ICKWritableCollector<in T>
    {
        /// <summary>
        /// Gets the count of elements in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds an element to the collection. The exact behavior of this operation
        /// depends on the concrete implementation (duplicates, filters, etc.).
        /// </summary>
        /// <param name="e">Element to add.</param>
        /// <returns>True if the element has been added, false otherwise.</returns>
        bool Add( T e );
    
    }
}
