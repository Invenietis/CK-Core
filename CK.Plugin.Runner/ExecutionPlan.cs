#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\ExecutionPlan.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Describes the final state that must be reached to satisfy 
    /// a new plugins and service requirements description.
    /// </summary>
    /// <remarks>
    /// The current running status of the plugins can be used to compute the best execution plan but does not change the content 
    /// of the <see cref="PluginsToStart"/>, <see cref="PluginsToStop"/> and <see cref="PluginsToDisable"/>.
    /// </remarks>
    public class ExecutionPlan
    {
        /// <summary>
        /// Gets the collection of plugins that must be started.
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToStart { get; private set; }

        /// <summary>
        /// Gets the collection of plugins that must be stopped.
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToStop { get; private set; }

        /// <summary>
        /// Gets the collection of plugins that must be disabled. 
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToDisable { get; private set; }

        /// <summary>
        /// Gets whether the execution is not possible (no running configuration that satisfy
        /// the requirements can be found).
        /// </summary>
        public bool Impossible { get { return PluginsToStart == null; } }

        internal ExecutionPlan() 
        { 
            // Let properties be null: This is the Impossible one.
        }

        internal ExecutionPlan( IEnumerable<IPluginInfo> pluginsToStart, IEnumerable<IPluginInfo> pluginsToStop, IReadOnlyCollection<IPluginInfo> pluginsToDisable )
        {
            Debug.Assert( pluginsToStart != null && pluginsToStop != null && pluginsToDisable != null );
            PluginsToStart = pluginsToStart.ToReadOnlyCollection();
            PluginsToStop = pluginsToStop.ToReadOnlyCollection();
            PluginsToDisable = pluginsToDisable;
        }

    }
}
