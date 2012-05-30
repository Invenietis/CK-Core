#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\ConfigExtension.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Extension methods for interfaces related to configuration.
    /// </summary>
    public static class ConfigExtension
    {
        /// <summary>
        /// Gets (creates it if needed) an easy to use <see cref="IObjectPluginConfig"/> that acts as a standard name-value dictionary.
        /// </summary>
        /// <param name="c">This <see cref="IConfigContainer"/> object.</param>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <returns>>An easy accessor for the object/plugin couple.</returns>
        public static IObjectPluginConfig GetObjectPluginConfig( this IConfigContainer c, object o, INamedVersionedUniqueId p )
        {
            return c.GetObjectPluginConfig( o, p, true );
        }

    }
}
