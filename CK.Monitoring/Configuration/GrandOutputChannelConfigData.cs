#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Configuration\GrandOutputChannelConfigData.cs) is part of CiviKey. 
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
using System.Xml;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Configuration for a channel. A channel is a route that receive outputs from monitors: this configuration is associated to a <see cref="CK.RouteConfig.RouteConfiguration"/> 
    /// or a <see cref="CK.RouteConfig.SubRouteConfiguration"/> object.
    /// Currently, the only configuration is the <see cref="MinimalFilter"/> of the channel.
    /// </summary>
    public class GrandOutputChannelConfigData
    {
        /// <summary>
        /// The minimal filter that will be applied to moniors that are bound (by their current <see cref="IActivityMonitor.Topic"/>) to this channel.
        /// Defaults to <see cref="LogFilter.Undefined"/>.
        /// </summary>
        public LogFilter MinimalFilter;

        /// <summary>
        /// Initializes a new instance of <see cref="GrandOutputChannelConfigData"/>.
        /// The <see cref="MinimalFilter"/> is <see cref="LogFilter.Undefined"/>.
        /// </summary>
        public GrandOutputChannelConfigData()
        {
            MinimalFilter = LogFilter.Undefined;
        }

        /// <summary>
        /// Reads a <see cref="XElement"/> with an optional "MinimalFilter" attribute.
        /// </summary>
        /// <param name="xml">The xml element.</param>
        public GrandOutputChannelConfigData( XElement xml )
        {
            MinimalFilter = xml.GetAttributeLogFilter( "MinimalFilter", true ).Value;
        }
    }
}
