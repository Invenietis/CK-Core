#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\RouteConfigurationResolved.cs) is part of CiviKey. 
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

namespace CK.RouteConfig
{
    /// <summary>
    /// Describes the configuration of a route once configuration is resolved (actions and subordinate routes are known).
    /// </summary>
    public class RouteConfigurationResolved
    {
        readonly string _name;
        readonly object _configData;
        readonly IReadOnlyList<ActionConfigurationResolved> _actions;
        IReadOnlyList<SubRouteConfigurationResolved> _routes;

        internal RouteConfigurationResolved( string name, object configData, IReadOnlyList<ActionConfigurationResolved> actions )
        {
            _name = name;
            _configData = configData;
            _actions = actions;
        }

        /// <summary>
        /// Gets the name of the route.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the configuration data object associated to this route.
        /// </summary>
        public object ConfigData { get { return _configData; } }

        /// <summary>
        /// Gets the subordinated routes that this route contains.
        /// </summary>
        public IReadOnlyList<SubRouteConfigurationResolved> SubRoutes 
        {
            get { return _routes; }
            internal set { _routes = value; }
        }

        /// <summary>
        /// Gets the actions that apply to this route.
        /// </summary>
        public IReadOnlyList<ActionConfigurationResolved> ActionsResolved { get { return _actions; } }
    }

}
