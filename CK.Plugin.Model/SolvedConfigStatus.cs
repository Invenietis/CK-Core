#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\SolvedConfigStatus.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Represents a final configuration status that applies to a plugin or a service.
    /// Adds the Disabled notion to the <see cref="RunningRequirements"/> enumeration.
    /// </summary>
    [Flags]
    public enum SolvedConfigStatus
    {
        /// <summary>
        /// Plugin is optional.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// Plugin is optional, but if it exists it should be started.
        /// </summary>
        OptionalTryStart = 1,

        /// <summary>
        /// Plugin must exist.
        /// </summary>
        MustExist = 2,

        /// <summary>
        /// Plugin must exist and we should try to start it.
        /// </summary>
        MustExistTryStart = 2 + 1,

        /// <summary>
        /// Plugin must exist and must be started.
        /// </summary>
        MustExistAndRun = 2 + 4,

        /// <summary>
        /// Plugin is disabled.
        /// </summary>
        Disabled = 8
    }
}
