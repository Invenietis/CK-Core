#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\Impl\SequenceHandler.cs) is part of CiviKey. 
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

using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    internal class SequenceHandler : HandlerBase
    {
        readonly HandlerBase[] _children;

        public SequenceHandler( ActionSequenceConfiguration c, HandlerBase[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handle in sequence.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        /// <param name="parrallelCall">True if this is called in parallel.</param>
        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            foreach( var c in _children ) c.Handle( logEvent, parrallelCall );
        }

    }
}
