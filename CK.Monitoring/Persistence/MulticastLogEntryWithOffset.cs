#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\MulticastLogEntryWithOffset.cs) is part of CiviKey. 
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

namespace CK.Monitoring
{
    /// <summary>
    /// Immutable capture of a log <see cref="Entry"/> and its <see cref="Offset"/>.
    /// </summary>
    public struct MulticastLogEntryWithOffset
    {
        /// <summary>
        /// The log entry.
        /// </summary>
        public readonly IMulticastLogEntry Entry;
        
        /// <summary>
        /// The entry's offset.
        /// </summary>
        public readonly long Offset;

        /// <summary>
        /// Initializes a new <see cref="MulticastLogEntryWithOffset"/>.
        /// </summary>
        /// <param name="e">The entry.</param>
        /// <param name="o">The offset.</param>
        public MulticastLogEntryWithOffset( IMulticastLogEntry e, long o )
        {
            Entry = e;
            Offset = o;
        }
    }

}
