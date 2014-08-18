#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\ActionConfiguration.cs) is part of CiviKey. 
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
    /// Configuration for an action defines a <see cref="Name"/>.
    /// This name must be unique among its sequence.
    /// </summary>
    public abstract class ActionConfiguration
    {
        string _name;

        /// <summary>
        /// Initializes a new <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="name">Required non empty name that identifies this configuration. Can not be null.</param>
        protected ActionConfiguration( string name )
        {
            if( name == null ) throw new ArgumentNullException( "name" );
            _name = name;
        }

        /// <summary>
        /// Gets or sets the name of this configuration. Can not be null.
        /// </summary>
        /// <remarks>
        /// Whether this name is a valid one or not is not checked by the configuration itself
        /// but by the <see cref="Impl.MetaConfiguration"/> that "covers" it: different rules may be 
        /// implemented for the name of a configuration depending of the way it is used, this is 
        /// the role of the meta configuration.
        /// </remarks>
        public string Name 
        {
            get { return _name; }
            set 
            {
                if( value == null ) throw new ArgumentNullException( "name" );
                _name = value; 
            } 
        }

        /// <summary>
        /// Checks the configuration validity. By default returns true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains the configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        public virtual bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        /// <summary>
        /// Gets whether this configuration is clone-able.
        /// Defaults to false.
        /// </summary>
        public virtual bool IsCloneable
        {
            get { return false; }
        }

        /// <summary>
        /// Clones this configuration.
        /// By default throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>A clone.</returns>
        public virtual ActionConfiguration Clone()
        {
            throw new NotSupportedException();
        }

    }
}
