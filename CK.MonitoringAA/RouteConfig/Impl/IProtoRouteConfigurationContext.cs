#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\IProtoRouteConfigurationContext.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Temporary context used to resolve the routes/actions associations.
    /// </summary>
    public interface IProtoRouteConfigurationContext
    {
        /// <summary>
        /// Gets the monitor to use.
        /// </summary>
        IActivityMonitor Monitor { get; }

        /// <summary>
        /// Adds a new subordinated route.
        /// </summary>
        /// <param name="route">The new subordinated route.</param>
        /// <returns>True on success, false if an error occurred such as a name clash for the route.</returns>
        bool AddRoute( SubRouteConfiguration route );

        /// <summary>
        /// Declares an action that can be an override of an existing one.
        /// </summary>
        /// <param name="a">Action to declare.</param>
        /// <param name="overridden">True if the action overrides an existing one.</param>
        /// <returns>True on success, false if an error occurred such as a name clash for the action and it is not an override.</returns>
        bool DeclareAction( ActionConfiguration a, bool overridden );
        
        /// <summary>
        /// Adds a <see cref="MetaConfiguration"/> to the route.
        /// </summary>
        /// <param name="meta">The meta configuration.</param>
        void AddMeta( MetaConfiguration meta );
    }
}
