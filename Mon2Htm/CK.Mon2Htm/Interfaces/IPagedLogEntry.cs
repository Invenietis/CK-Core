#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CK.Mon2Htm\Interfaces\IPagedLogEntry.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
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
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public interface IPagedLogEntry : ILogEntry
    {
        /// <summary>
        /// Children contained in a group. Can be null (not an OpenGroup) or empty.
        /// </summary>
        IReadOnlyList<IPagedLogEntry> Children { get; }

        /// <summary>
        /// Page at which a group started. 0 when the group started in the same page.
        /// </summary>
        int GroupStartsOnPage { get; }

        /// <summary>
        /// Page at which a group ended. 0 when the group ended in the same page.
        /// </summary>
        int GroupEndsOnPage { get; }
    }
}
