#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\LogEntryType.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Qualifies the actual type of <see cref="ILogInterceptionEntry"/>.
    /// </summary>
    public enum LogEntryType
    {
        /// <summary>
        /// The log entry is a <see cref="ILogEventEntry"/>.
        /// </summary>
        Event,

        /// <summary>
        /// The log entry is a <see cref="ILogEventError"/>.
        /// </summary>
        EventError,

        /// <summary>
        /// The log entry is a <see cref="ILogEventNotRunningError"/>.
        /// </summary>
        EventNotRunningError,

        /// <summary>
        /// The log entry is a <see cref="ILogMethodEntry"/>.
        /// </summary>
        Method,

        /// <summary>
        /// The log entry is a <see cref="ILogMethodError"/>.
        /// </summary>
        MethodError,

        /// <summary>
        /// The log entry is a <see cref="ILogExternal"/>.
        /// </summary>
        External,
        
        /// <summary>
        /// The log entry is a <see cref="ILogExternalError"/>.
        /// </summary>
        ExternalError
    }

}
