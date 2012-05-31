#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\ILogErrorCulprit.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CK.Plugin
{
    /// <summary>
    /// Base interface that defines a log event that holds an <see cref="Exception"/>.
    /// </summary>
    public interface ILogErrorCulprit : ILogEntry
    {
        /// <summary>
        /// The culprit is actually required to define an error. 
        /// The specialized <see cref="ILogErrorCaught"/> holds an exception but there exist errors 
        /// that do not have any associated exception to expose.
        /// This is the case of <see cref="ILogEventNotRunningError"/>: when a plugin raises an event 
        /// while beeing stopped, it is an error (silently ignored by the kernel), but there is
        /// no exception to associate with.
        /// </summary>
        MemberInfo Culprit { get; }

    }
}
