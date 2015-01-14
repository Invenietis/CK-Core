#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\SubRouteConfigurationResolved.cs) is part of CiviKey. 
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
using CK.RouteConfig.Impl;

namespace CK.RouteConfig
{
    /// <summary>
    /// Describes the configuration of a subordinated route once configuration is resolved.
    /// </summary>
    public class SubRouteConfigurationResolved : RouteConfigurationResolved
    {
        readonly Func<string,bool> _routePredicate;

        internal SubRouteConfigurationResolved( IProtoSubRoute c, IReadOnlyList<ActionConfigurationResolved> actions )
            : base( c.FullName, c.Configuration.ConfigData, actions )
        {
            _routePredicate = c.Configuration.RoutePredicate;
        }

        /// <summary>
        /// Gets the filter that route must respect to enter this sub route.
        /// </summary>
        public Func<string, bool> RoutePredicate { get { return _routePredicate; } }
    }

}
