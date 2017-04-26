#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\SourceLogFilter.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Immutable encapsulation of the two source filters: this enables overriding or filtering per source file.
    /// </summary>
    public struct SourceLogFilter
    {
        /// <summary>
        /// Undefined filter is <see cref="LogFilter.Undefined"/> for both <see cref="Override"/> and <see cref="Minimal"/>.
        /// This is the same as using the default constructor for this structure (it is exposed here for clarity).
        /// </summary>
        static public readonly SourceLogFilter Undefined = new SourceLogFilter( LogFilter.Undefined, LogFilter.Undefined );

        /// <summary>
        /// The filter to be applied before challenging the <see cref="IActivityMonitor.ActualFilter"/>.
        /// When not <see cref="LogFilter.Undefined"/>, the ActualFilter is ignored  (as well as this <see cref="Minimal"/>).
        /// </summary>
        public readonly LogFilter Override;

        /// <summary>
        /// The filter that when defined is combined with the  <see cref="IActivityMonitor.ActualFilter"/>.
        /// </summary>
        public readonly LogFilter Minimal;

        /// <summary>
        /// Initializes a new <see cref="SourceLogFilter"/> with a given filter for <see cref="Override"/>s and <see cref="Minimal"/>.
        /// </summary>
        /// <param name="overrideFilter">Overridden filter.</param>
        /// <param name="minimalFilter">Minimal filter.</param>
        public SourceLogFilter( LogFilter overrideFilter, LogFilter minimalFilter )
        {
            Override = overrideFilter;
            Minimal = minimalFilter;
        }

        /// <summary>
        /// Gets whether this is equal to <see cref="SourceLogFilter.Undefined"/>.
        /// </summary>
        public bool IsUndefined
        {
            get { return Override == LogFilter.Undefined && Minimal == LogFilter.Undefined; }
        }

        /// <summary>
        /// Combines this filter with another one. <see cref="Override"/> and <see cref="Minimal"/> level filters
        /// are combined with <see cref="LogFilter.Combine(LogFilter)"/>.
        /// </summary>
        /// <param name="other">The other filter to combine with this one.</param>
        /// <returns>The resulting filter.</returns>
        public SourceLogFilter Combine( SourceLogFilter other )
        {
            return new SourceLogFilter( Override.Combine( other.Override ), Minimal.Combine( other.Minimal ) );
        }

        /// <summary>
        /// Gets a combined integer: high word contains Override and low word contains Minimal filter for lines.
        /// </summary>
        internal int LineFilter
        {
            get { return (((UInt16)Override.Line) << 16) | (UInt16)Minimal.Line; }
        }

        /// <summary>
        /// Gets a combined integer: high word contains Override and low word contains Minimal filter for groups.
        /// </summary>
        internal int GroupFilter
        {
            get { return (((UInt16)Override.Group) << 16) | (UInt16)Minimal.Group; }
        }
    }
}
