#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\IPluginProxy.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Plugin proxy.
    /// </summary>
    public interface IPluginProxy : INamedVersionedUniqueId
    {
        /// <summary>
        /// Gets a key object that uniquely identifies a plugin.
        /// </summary>
        IPluginInfo PluginKey { get; }

        /// <summary>
        /// Gets the real instance of the underlying plugin.
        /// </summary>
        object RealPluginObject { get; }

        /// <summary>
        /// Exception raised when the plugin was last activated. Null if no error occured.
        /// </summary>
        Exception LoadError { get; }

        /// <summary>
        /// True if the concrete plugin has been activated without error.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Current running status of the plugin.
        /// </summary>
        RunningStatus Status { get; }

    }
}
