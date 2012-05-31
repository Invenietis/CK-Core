#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\ServiceLogEventOptions.cs) is part of CiviKey. 
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
    /// Bit flags that describes the way an event is intercepted.
    /// </summary>
    [Flags]
    public enum ServiceLogEventOptions
    {
        /// <summary>
        /// "Naked mode". Nothing is logged (even exceptions). 
        /// </summary>
        None = 0,

        /// <summary>
        /// Errors (exceptions occuring during event raising) are logged.
        /// </summary>
        LogErrors = 1,

        /// <summary>
        /// Logs the beginning of the event raising.
        /// </summary>
        StartRaise = 2,
        
        /// <summary>
        /// Logs the parameters of the event.
        /// </summary>
        LogParameters = 4,
                
        /// <summary>
        /// Logs the end of the event raising.
        /// </summary>
        EndRaise = 8,

        /// <summary>
        /// Covers event configuration flags (excludes <see cref="LogErrors"/>, <see cref="SilentEventRunningStatusError"/> and <see cref="SilentEventError"/>)
        /// that triggers the creation of an entry.
        /// </summary>
        CreateEntryMask = StartRaise | LogParameters | EndRaise,

        /// <summary>
        /// Ignores any error when a service raises an event while not running.
        /// Since we intercept the raising of the event, this corrects the bad behavior.
        /// </summary>
        SilentEventRunningStatusError = 16,

        /// <summary>
        /// When <see cref="SilentEventRunningStatusError"/> is set, this triggers the log of an error.
        /// </summary>
        LogSilentEventRunningStatusError = 32,

        /// <summary>
        /// Exceptions raised while receivers handle the event will be ignored.
        /// Remaining subscribers of the event will receive the event.
        /// (This flag is independant of <see cref="LogErrors"/>.)
        /// </summary>
        SilentEventError = 64,
    }

}
