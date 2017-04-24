#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\LogLevelFilter.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Defines filters for <see cref="LogLevel"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="LogFilter"/> capture two levels: one for lines and one for groups.
    /// </remarks>
    public enum LogLevelFilter : short
    {
        /// <summary>
        /// No filter: can have the same effect as Trace but SHOULD indicate
        /// an unknown or undefined filter that, when combined with other level filters to 
        /// compute the final (minimal) filter level to take into account, must be ignored.
        /// </summary>
        None = 0,
        /// <summary>
        /// Everything is logged (<see cref="LogLevel.Debug"/>).
        /// </summary>
        Debug = 1,
        /// <summary>
        /// <see cref="LogLevel.Trace"/> and above is logged..
        /// </summary>
        Trace = 2,
        /// <summary>
        /// Only <see cref="LogLevel.Info"/> and above is logged.
        /// </summary>
        Info = 4,
        /// <summary>
        /// Only <see cref="LogLevel.Warn"/> and above is logged.
        /// </summary>
        Warn = 8,
        /// <summary>
        /// Only <see cref="LogLevel.Error"/> and above is logged.
        /// </summary>
        Error = 16,
        /// <summary>
        /// Only <see cref="LogLevel.Fatal"/> is logged.
        /// </summary>
        Fatal = 32,
        /// <summary>
        /// Do not log anything.
        /// </summary>
        Off = 64,
        /// <summary>
        /// Invalid filter can be use to designate an unknown filter. 
        /// Since its value is -1, in the worst case it will not filter anything.
        /// </summary>
        Invalid = -1
    }
}