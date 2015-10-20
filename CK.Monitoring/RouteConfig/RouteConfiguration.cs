#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\RouteConfiguration.cs) is part of CiviKey. 
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
    /// Primary configuration object that contains multiple <see cref="SubRouteConfiguration"/>s and <see cref="ActionConfiguration"/>s.
    /// </summary>
    public class RouteConfiguration
    {
        readonly string _name;
        readonly List<MetaConfiguration> _configurations;
        string _namespace;
        object _configData;

        /// <summary>
        /// Initializes a new root <see cref="RouteConfiguration"/>.
        /// </summary>
        public RouteConfiguration()
            : this( String.Empty )
        {
        }

        /// <summary>
        /// Initializes a specialized <see cref="RouteConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of the route. Can not be null.</param>
        protected RouteConfiguration( string name )
        {
            if( name == null ) throw new ArgumentNullException( "name" );
            _name = name;
            _configurations = new List<MetaConfiguration>();
            _namespace = String.Empty;
        }

        /// <summary>
        /// Gets the name of this configuration.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets or sets any configuration data for this route.
        /// </summary>
        public object ConfigData 
        {
            get { return _configData; }
            set { _configData = value; } 
        }

        /// <summary>
        /// Gets or sets an optional namespace for this route: declared <see cref="SubRouteConfiguration"/>
        /// are automatically prefixed with this namespace. Never null.
        /// This namespace has no effect on any <see cref="ActionConfiguration.Name"/>.
        /// </summary>
        public string Namespace 
        { 
            get { return _namespace; } 
            set { _namespace = value ?? String.Empty; } 
        }

        /// <summary>
        /// Gets the content of this configuration: a list of <see cref="MetaConfiguration"/> that 
        /// encapsulates <see cref="ActionConfiguration"/> and/or <see cref="SubRouteConfiguration"/>.
        /// </summary>
        public IReadOnlyList<MetaConfiguration> Configurations { get { return _configurations; } }

        /// <summary>
        /// Adds one or more <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="a">The first configuration.</param>
        /// <param name="otherActions">Optional other configurations.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration AddAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaAddActionConfiguration( a, otherActions ) );
            return this;
        }

        /// <summary>
        /// Declares one or more <see cref="ActionConfiguration"/>. It can be inserted later thanks to <see cref="InsertAction"/>.
        /// </summary>
        /// <param name="a">The first configuration to declare.</param>
        /// <param name="otherActions">Optional other configurations to declare.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration DeclareAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaDeclareActionConfiguration( a, otherActions ) );
            return this;
        }

        /// <summary>
        /// Overrides one or more existing <see cref="ActionConfiguration"/> (lookup is done by name).
        /// </summary>
        /// <param name="a">The first configuration to override.</param>
        /// <param name="otherActions">Optional other configurations to override.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration OverrideAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaOverrideActionConfiguration( a, otherActions ) );
            return this;
        }

        /// <summary>
        /// Removes one or more existing <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="name">The first configuration name to remove.</param>
        /// <param name="otherNames">Optional other configurations' name to remove.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration RemoveAction( string name, params string[] otherNames )
        {
            _configurations.Add( new MetaRemoveActionConfiguration( name, otherNames ) );
            return this;
        }

        /// <summary>
        /// Inserts a previously <see cref="DeclareAction">declared</see> action.
        /// </summary>
        /// <param name="name">The name of the inserted configuration.</param>
        /// <param name="declarationName">The name of the previously declared action.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration InsertAction( string name, string declarationName )
        {
            _configurations.Add( new MetaInsertActionConfiguration( name, declarationName ) );
            return this;
        }

        /// <summary>
        /// Declare a new subordinated route.
        /// </summary>
        /// <param name="route">The subordinated route configuration.</param>
        /// <returns>This object.</returns>
        public RouteConfiguration DeclareRoute( SubRouteConfiguration route )
        {
            _configurations.Add( new MetaDeclareRouteConfiguration( route ) );
            return this;
        }

        /// <summary>
        /// Protected method to actually add any <see cref="MetaConfiguration"/> object.
        /// </summary>
        /// <param name="m">A meta configuration to add.</param>
        protected void AddMeta( MetaConfiguration m )
        {
            _configurations.Add( m );
        }

        /// <summary>
        /// Attempts to resolve the configuration. Null if an error occurred.
        /// </summary>
        /// <param name="monitor">Monitor to use. Must not be null.</param>
        /// <returns>Null or a set of resolved route configuration.</returns>
        public RouteConfigurationResult Resolve( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            RouteConfigurationResult result;
            bool hasError = false;
            using( monitor.OnError( () => hasError = true ) )
            {
                var r = new RouteResolver( monitor, this );
                result = new RouteConfigurationResult( r.Root, r.NamedSubRoutes );
            }
            return hasError ? null : result;
        }

    }
}
