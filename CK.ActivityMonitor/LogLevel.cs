#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\LogLevel.cs) is part of CiviKey. 
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
    /// Five standard log levels in increasing order used by <see cref="IActivityMonitor"/>.
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// No logging level.
        /// </summary>
        None = 0,
        /// <summary>
        /// Debug logging level (the most verbose level).
        /// </summary>
        Debug = 1,
        /// <summary>
        /// A trace logging level (quite verbose level).
        /// </summary>
        Trace = 2,
        /// <summary>
        /// An info logging level.
        /// </summary>
        Info = 4,
        /// <summary>
        /// A warn logging level.
        /// </summary>
        Warn = 8,
        /// <summary>
        /// An error logging level: denotes an error for the current activity. 
        /// This error does not necessarily abort the activity.
        /// </summary>
        Error = 16,
        /// <summary>
        /// A fatal error logging level: denotes an error that breaks (aborts)
        /// the current activity. This kind of error may have important side effects
        /// on the system.
        /// </summary>
        Fatal = 32,

        /// <summary>
        /// Mask that covers actual levels to easily ignore <see cref="IsFiltered"/> bit.
        /// </summary>
        Mask = 63,

        /// <summary>
        /// Flag that denotes a log level that has been filtered.
        /// When this flag is not set, the <see cref="IActivityMonitor.UnfilteredOpenGroup"/> or <see cref="IActivityMonitor.UnfilteredLog"/> has been 
        /// called directly. When set, the log has typically been emitted through the extension methods that challenge the 
        /// monitor's <see cref="IActivityMonitor.ActualFilter">actual filter</see> and <see cref="ActivityMonitor.DefaultFilter"/> static configuration.
        /// </summary>
        IsFiltered = 64,

        /// <summary>
        /// Number of bits actually covered by this bit flag.
        /// </summary>
        NumberOfBits = 7
    }

}
