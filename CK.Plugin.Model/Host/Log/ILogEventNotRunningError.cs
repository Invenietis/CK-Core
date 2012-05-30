#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\ILogEventNotRunningError.cs) is part of CiviKey. 
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
using System.Reflection;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Log event emitted when service that is not running raised an event.
    /// </summary>
    public interface ILogEventNotRunningError : ILogInterceptionEntry, ILogErrorCulprit
    {
        /// <summary>
        /// The event raised when the service not in a running state: it is the <see cref="ILogErrorCulprit.Culprit"/>.
        /// </summary>
        EventInfo Event { get; }

        /// <summary>
        /// True if the service is actually disabled, false if it is not running.
        /// </summary>
        bool ServiceIsDisabled { get; }
    }
}
