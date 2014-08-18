#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\ChannelOption.cs) is part of CiviKey. 
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

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Captures centralized information among the different <see cref="HandlerBase">Handlers</see> of a channel.
    /// </summary>
    public sealed class ChannelOption
    {
        LogFilter _currentFilter;

        internal ChannelOption( LogFilter mainRouteFilter )
        {
            _currentFilter = mainRouteFilter;
        }

        /// <summary>
        /// Enables any handler to publish the minimal filter level it requires (if any).
        /// </summary>
        /// <param name="filter">Filter required by a <see cref="HandlerBase"/>.</param>
        public void SetMinimalFilter( LogFilter filter )
        {
            _currentFilter = _currentFilter.Combine( filter );
        }

        /// <summary>
        /// Gets the minimal <see cref="LogFilter"/>.
        /// Since a handler can publish its minimal filter requirement, we can optimize the filtering levels on 
        /// monitors bound to a channel.
        /// </summary>
        public LogFilter CurrentMinimalFilter { get { return _currentFilter; } }

    }
}
