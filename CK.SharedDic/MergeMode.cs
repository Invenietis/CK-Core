#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\MergeMode.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Storage;
using System.Linq;

namespace CK.SharedDic
{
    public enum MergeMode
    {
        /// <summary>
        /// No merge: existing data is lost.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// A data for an existing object/plugin key replaces the previous one.
        /// </summary>
        ReplaceExisting,

        /// <summary>
        /// The behavior will be very close to <value>ReplaceExisting</value>.
        /// If the same data is detected in the source and target site we will try to merge them 
        /// thanks to <see cref="IMergeable"/> interface implementation.
        /// </summary>
        ReplaceExistingTryMerge,

        /// <summary>
        /// A data for an existing object/plugin must be ignored: the current value is preserved, new data is lost.
        /// </summary>
        PreserveExisting,

        /// <summary>
        /// No duplicate should exist: any attempt to add data for an existing object/plugin key
        /// raises an exception.
        /// </summary>
        ErrorOnDuplicate,
    }
}
