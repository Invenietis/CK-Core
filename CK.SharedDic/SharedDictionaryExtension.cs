#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\SharedDictionaryExtension.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CK.SharedDic;
using CK.Storage;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Extension methods for shared dictionary objects and interfaces.
    /// </summary>
    public static class SharedDictionaryExtension
    {
        /// <summary>
        /// Writes plugins data for an object.
        /// Even if no data exists, the element is written as an empty &lt;<paramref name="elemenName"/> /&gt; element.
        /// </summary>
        /// <param name="w">This <see cref="ISharedDictionaryWriter"/> object.</param>
        /// <param name="elementName">Name of the element that will contain the configuration.</param>
        /// <param name="o">Object for which configuration must be written.</param>
        /// <returns>The number of plugins for which data has been written.</returns>
        static public int WritePluginsDataElement( this ISharedDictionaryWriter w, string elementName, object o )
        {
            return w.WritePluginsDataElement( elementName, o, false );
        }


    }
}
