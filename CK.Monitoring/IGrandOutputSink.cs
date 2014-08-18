#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\IGrandOutputSink.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Defines a sink that can be registered onto a <see cref="GrandOutput"/>
    /// to intercept any log event. It is also supported by <see cref="CK.Monitoring.GrandOutputHandlers.HandlerBase"/>.
    /// </summary>
    public interface IGrandOutputSink
    {
        /// <summary>
        /// This is initially called non concurrently from a dispatcher background thread:
        /// implementations do not need any synchronization mechanism by default except when <paramref name="parrallelCall"/> is true.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="parrallelCall">True when this method is called in parallel with other sinks.</param>
        void Handle( GrandOutputEventInfo logEvent, bool parrallelCall );
    }

}
