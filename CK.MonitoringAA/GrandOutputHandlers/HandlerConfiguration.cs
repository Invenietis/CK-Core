#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\HandlerConfiguration.cs) is part of CiviKey. 
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

using System.Xml.Linq;
using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Base class for handler configuration.
    /// </summary>
    public abstract class HandlerConfiguration : ActionConfiguration
    {
        /// <summary>
        /// Initializes a new handler configuration.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        protected HandlerConfiguration( string name )
            : base( name )
        {
        }

        /// <summary>
        /// Gets or sets the minimal filter for this handler.
        /// Defaults to <see cref="LogFilter.Undefined"/>: unless specified, the handler will not 
        /// participate to <see cref="IActivityMonitor.ActualFilter"/> resolution of monitors that eventually 
        /// are handled by this handler.See remarks. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is NOT a filter: this is the minimal filter that guaranties that, at least, the specified 
        /// levels will reach this handler.
        /// </para>
        /// <para>
        /// A concrete handler can, if needed, define a true filter: it is its business to retain or forget 
        /// what it wants.
        /// </para>
        /// </remarks>
        public LogFilter MinimalFilter { get; set; }

        internal void DoInitialize( IActivityMonitor m, XElement xml )
        {
            MinimalFilter = xml.GetAttributeLogFilter( "MinimalFilter", true ).Value;
            Initialize( m, xml );
        }
        
        /// <summary>
        /// Must initializes this configuration object with its specific data from an xml element.
        /// </summary>
        /// <param name="m">Monitor to use.</param>
        /// <param name="xml">The xml element.</param>
        protected abstract void Initialize( IActivityMonitor m, XElement xml );
    }
}
