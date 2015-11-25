#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Configuration\SourceFilterApplyMode.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Defines how the <see cref="GrandOutputConfiguration"/> applies its <see cref="GrandOutputConfiguration.SourceOverrideFilter"/>
    /// to the application domain's global <see cref="ActivityMonitor.SourceFilter.FilterSource"/>.
    /// </summary>
    public enum SourceFilterApplyMode
    {
        /// <summary>
        /// Source filters is ignored.
        /// </summary>
        None = 0,

        /// <summary>
        /// Clears the current <see cref="ActivityMonitor.SourceFilter.FilterSource"/>.
        /// </summary>
        Clear = 1,

        /// <summary>
        /// Clears the current <see cref="ActivityMonitor.SourceFilter.FilterSource"/> and then applies the new ones.
        /// </summary>
        ClearThenApply = 2,

        /// <summary>
        /// Applies the filters.
        /// </summary>
        Apply = 3
    }

}
