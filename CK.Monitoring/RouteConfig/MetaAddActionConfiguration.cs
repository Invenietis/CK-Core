#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\MetaAddActionConfiguration.cs) is part of CiviKey. 
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

namespace CK.RouteConfig
{
    /// <summary>
    /// Declares one or more <see cref="ActionConfiguration"/> and inserts them with their names.
    /// </summary>
    public class MetaAddActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        /// <summary>
        /// Initializes a <see cref="MetaAddActionConfiguration"/> with at least one action to add.
        /// </summary>
        /// <param name="action">First, required, action to add.</param>
        /// <param name="otherActions">Optional other actions to add.</param>
        public MetaAddActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        /// <summary>
        /// Gets the <see cref="ActionConfiguration"/>s that must be declared and inserted with their names.
        /// </summary>
        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Adds an <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="action">The action to add.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public new MetaAddActionConfiguration Add( ActionConfiguration action )
        {
            base.Add( action );
            return this;
        }

        /// <summary>
        /// Checks the validity: each action's name must be valid and each <see cref="ActionConfiguration.CheckValidity"/> must return true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains this configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            bool result = true;
            foreach( var a in Actions ) result &= CheckActionNameValidity( routeName, monitor, a.Name ) && a.CheckValidity( routeName, monitor );
            return result;
        }

        /// <summary>
        /// Applies the configuration (first step) by <see cref="Impl.IProtoRouteConfigurationContext.DeclareAction">declaring</see> all the actions
        /// and adding itself to the protoContext so that <see cref="Apply(Impl.IRouteConfigurationContext)"/> will be called.
        /// </summary>
        /// <param name="protoContext">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            foreach( var a in Actions ) protoContext.DeclareAction( a, overridden: false );
            protoContext.AddMeta( this );
        }

        /// <summary>
        /// Applies the configuration (second step) by calling <see cref="Impl.IRouteConfigurationContext.AddDeclaredAction"/> for the <see cref="Actions"/>.
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            foreach( var a in Actions ) context.AddDeclaredAction( a.Name, a.Name, fromMetaInsert: true );
        }
    }
}
