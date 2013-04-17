#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\IWritableCollection.cs) is part of CiviKey. 
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
    /// Contravariant interface for a collection that allows to <see cref="Clear"/> and <see cref="Remove"/>
    /// element.
    /// </summary>
    /// <typeparam name="T">Base type for the elements of the collection.</typeparam>
    public interface ICKWritableCollection<in T> : ICKWritableCollector<T>
    {
        /// <summary>
        /// Clears the collection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes the element if it exists.
        /// </summary>
        /// <param name="e">Element to remove.</param>
        /// <returns>True if the element has been removed, false otherwise.</returns>
        bool Remove( T e );
    
    }
}
