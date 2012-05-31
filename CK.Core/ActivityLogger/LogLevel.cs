#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\LogLevel.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Five standard log levels in increasing order used by <see cref="IActivityLogger"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// No logging level.
        /// </summary>
        None = 0,
        /// <summary>
        /// A trace logging level (the most verbose level).
        /// </summary>
        Trace = 1,
        /// <summary>
        /// An info logging level.
        /// </summary>
        Info = 2,
        /// <summary>
        /// A warn logging level.
        /// </summary>
        Warn = 3,
        /// <summary>
        /// An error logging level: denotes an error for the current activity. 
        /// This error does not necessarily abort the activity.
        /// </summary>
        Error = 4,
        /// <summary>
        /// A fatal error logging level: denotes an error that breaks (aborts)
        /// the current activity. This kind of error may have important side effects
        /// on the system.
        /// </summary>
        Fatal = 5
    }

}
