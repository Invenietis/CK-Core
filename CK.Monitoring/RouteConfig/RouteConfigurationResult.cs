#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\RouteConfigurationResult.cs) is part of CiviKey. 
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
    /// Encapsulates the resolution of routes configuration: the <see cref="RouteConfiguration.Resolve"/> method computes it.
    /// </summary>
    public class RouteConfigurationResult
    {
        readonly RouteConfigurationResolved _root;
        readonly Dictionary<string, SubRouteConfigurationResolved> _namedSubRoutes;
        readonly IReadOnlyCollection<SubRouteConfigurationResolved> _namedSubRoutesEx;

        internal RouteConfigurationResult( RouteConfigurationResolved root, Dictionary<string, SubRouteConfigurationResolved> namedSubRoutes )
        {
            _root = root;
            _namedSubRoutes = namedSubRoutes;
            _namedSubRoutesEx = new CKReadOnlyCollectionOnICollection<SubRouteConfigurationResolved>( _namedSubRoutes.Values );
        }

        /// <summary>
        /// Gets the resolved root route.
        /// </summary>
        public RouteConfigurationResolved Root
        {
            get { return _root; }
        } 

        /// <summary>
        /// Gets all the subordinated routes.
        /// </summary>
        public IReadOnlyCollection<SubRouteConfigurationResolved> AllSubRoutes
        {
            get { return _namedSubRoutesEx; }
        }

        /// <summary>
        /// Finds a subordinated route by its name.
        /// </summary>
        /// <param name="name">Name of the route.</param>
        /// <returns>The route or null if it does not exist.</returns>
        public SubRouteConfigurationResolved FindSubRouteByName( string name )
        {
            return _namedSubRoutes.GetValueWithDefault( name, null );
        }

    }
}
