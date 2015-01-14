#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Configuration\XmlMonitoringExtensions.cs) is part of CiviKey. 
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
using System.Xml;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Helpers to read XML configurations.
    /// </summary>
    public static class XmlMonitoringExtensions
    {
        /// <summary>
        /// Reads a <see cref="LogFilter"/> that must exist.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        /// <returns>A LogFilter.</returns>
        static public LogFilter GetRequiredAttributeLogFilter( this XElement @this, string name )
        {
            return LogFilter.Parse( @this.AttributeRequired( name ).Value );
        }

        /// <summary>
        /// Reads a <see cref="LogFilter"/>.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="fallbackToUndefined">True to return <see cref="LogFilter.Undefined"/> instead of null when not found.</param>
        /// <returns>A nullable LogFilter.</returns>
        static public LogFilter? GetAttributeLogFilter( this XElement @this, string name, bool fallbackToUndefined )
        {
            XAttribute a = @this.Attribute( name );
            return a != null ? LogFilter.Parse( a.Value ) : (fallbackToUndefined ? LogFilter.Undefined : (LogFilter?)null);
        }

    }
}
