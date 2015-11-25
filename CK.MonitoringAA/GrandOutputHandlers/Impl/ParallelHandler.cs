#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\Impl\ParallelHandler.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    internal class ParallelHandler : HandlerBase
    {
        readonly HandlerBase[] _children;

        public ParallelHandler( ActionParallelConfiguration c, HandlerBase[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handler in parallel.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        /// <param name="parrallelCall">True if this is called in parallel.</param>
        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            Parallel.For( 0,  _children.Length, i => _children[i].Handle( logEvent, true ) );
        }

    }
}
