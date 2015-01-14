#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\IProtoRoute.cs) is part of CiviKey. 
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

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Intermediate objects that captures the first step of configuration resolution.
    /// At this step we manipulate <see cref="MetaConfiguration"/> objects.
    /// </summary>
    public interface IProtoRoute
    {
        /// <summary>
        /// Gets the associated <see cref="RouteConfiguration"/> object.
        /// </summary>
        RouteConfiguration Configuration { get; }

        /// <summary>
        /// Gets the namespace of this route.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the full name of this route.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the list of <see cref="MetaConfiguration"/> objects such as <see cref="MetaAddActionConfiguration"/> or <see cref="MetaRemoveActionConfiguration"/>.
        /// </summary>
        IReadOnlyList<MetaConfiguration> MetaConfigurations { get; }

        /// <summary>
        /// Finds a previously declared action.
        /// The action can exist in the parent routes if <see cref="SubRouteConfiguration.ImportParentDeclaredActionsAbove"/> is true (which is the default).
        /// </summary>
        /// <param name="name">Name of an existing action.</param>
        /// <returns>Null or the action with the name.</returns>
        ActionConfiguration FindDeclaredAction( string name );

        /// <summary>
        /// Gets the list of subordinated route.
        /// </summary>
        IReadOnlyList<IProtoSubRoute> SubRoutes { get; }
    }

}
