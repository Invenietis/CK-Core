#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\SetOperation.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Defines the six basic operations available between two sets.
    /// </summary>
    public enum SetOperation
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None,
        
        /// <summary>
        /// Union of the sets (keeps items of first or second set).
        /// </summary>
        Union,
        
        /// <summary>
        /// Intersection of the sets (keeps only items that belong to both sets).
        /// </summary>
        Intersect,
        
        /// <summary>
        /// Exclusion (keeps only items of the first that do not belong to the second one).
        /// </summary>
        Except,
        
        /// <summary>
        /// Symetric exclusion (keeps items that belong to first or second set but not to both) - The XOR operation.
        /// </summary>
        SymetricExcept,

        /// <summary>
        /// Replace the first set by the second one.
        /// </summary>
        Replace
    }

}
