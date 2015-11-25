#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\MetaConfiguration.cs) is part of CiviKey. 
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
    /// Base class for meta configuration object: those objects configure the configuration.
    /// </summary>
    public abstract class MetaConfiguration
    {
        /// <summary>
        /// Check the configuration validity.
        /// </summary>
        /// <param name="routeName">Name of the route that contains this configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        protected internal abstract bool CheckValidity( string routeName, IActivityMonitor monitor );

        /// <summary>
        /// Applies the configuration (first step).
        /// By default, adds this meta configuration to the context so that <see cref="Apply(IRouteConfigurationContext)"/> will be called.
        /// </summary>
        /// <param name="protoContext">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal virtual void Apply( IProtoRouteConfigurationContext protoContext )
        {
            protoContext.AddMeta( this );
        }

        /// <summary>
        /// Applies the configuration (second step).
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal abstract void Apply( IRouteConfigurationContext context );

        /// <summary>
        /// Implements standard name checking for <see cref="ActionConfiguration.Name"/>. 
        /// The provided <paramref name="nameToCheck"/> must not be null or empty or contains only whitespaces nor '/' character.
        /// The '/' is reserved to structure the namespace.
        /// </summary>
        /// <param name="routeName">The name of the route that contains the action.</param>
        /// <param name="monitor">The monitor that will receive error descriptions.</param>
        /// <param name="nameToCheck">The name to check.</param>
        /// <returns>True if the name is valid. False otherwise.</returns>
        static public bool CheckActionNameValidity( string routeName, IActivityMonitor monitor, string nameToCheck )
        {
            if( String.IsNullOrWhiteSpace( nameToCheck ) ) monitor.Error().Send( "Invalid name '{0}' in route '{1}'. Name must not be empty or contains only white space.", nameToCheck, routeName );
            else if( nameToCheck.Contains( '/' ) ) monitor.Error().Send( "Invalid name '{0}' in route '{1}'. Name must not contain '/'.", nameToCheck, routeName );
            else return true;
            return false;
        }

    }
}
