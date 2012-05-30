#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\RunningRequirements.cs) is part of CiviKey. 
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
    /// Describes how a service or a plugin is required. 
    /// A requirement is a gradation between <see cref="Optional"/> and <see cref="MustExistAndRun"/>.
    /// </summary>
    [Flags]
    public enum RunningRequirement
    {
        /// <summary>
        /// The service or plugin is optional: it can be unavailable.
        /// </summary>
        Optional = 0,
        
        /// <summary>
        /// If it is available the service or plugin should be started.
        /// </summary>
        OptionalTryStart = 1,
        
        /// <summary>
		/// The service or plugin must be available (but it can be stopped).
        /// </summary>
        MustExist = 2,
        
        /// <summary>
		/// The service or plugin must be available and, if possible, should be started.
        /// </summary>
        MustExistTryStart = 2+1,
        
        /// <summary>
		/// The service or plugin must be available and must run.
        /// </summary>
        MustExistAndRun = 2+4
    }
}
