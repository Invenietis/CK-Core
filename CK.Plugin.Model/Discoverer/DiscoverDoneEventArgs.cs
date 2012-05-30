#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\DiscoverDoneEventArgs.cs) is part of CiviKey. 
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
    public class DiscoverDoneEventArgs : EventArgs
    {
        IReadOnlyList<IAssemblyInfo> _newAssemblies;
        IReadOnlyList<IAssemblyInfo> _changedAssemblies;
        IReadOnlyList<IAssemblyInfo> _deletedAssemblies;

        IReadOnlyList<IPluginInfo> _newPlugins;
        IReadOnlyList<IPluginInfo> _changedPlugins;
        IReadOnlyList<IPluginInfo> _deletedPlugins;

        IReadOnlyList<IPluginConfigAccessorInfo> _newEditors;
        IReadOnlyList<IPluginConfigAccessorInfo> _changedEditors;
        IReadOnlyList<IPluginConfigAccessorInfo> _deletedEditors;

        IReadOnlyList<IServiceInfo> _newServices;
        IReadOnlyList<IServiceInfo> _changedServices;
        IReadOnlyList<IServiceInfo> _deletedServices;

        IReadOnlyList<IPluginInfo> _newOldPlugins;
        IReadOnlyList<IPluginInfo> _deletedOldPlugins;

        IReadOnlyList<string> _newMissingAssemblies;
        IReadOnlyList<string> _deletedMissingAssemblies;

        public IReadOnlyList<IAssemblyInfo> NewAssemblies { get { return _newAssemblies; } }
        public IReadOnlyList<IAssemblyInfo> ChangedAssemblies { get { return _changedAssemblies; } }
        public IReadOnlyList<IAssemblyInfo> DeletedAssemblies { get { return _deletedAssemblies; } }

        /// <summary>
        /// Gets the list of new discovered plugins (contains also plugins on error).
        /// </summary>
        public IReadOnlyList<IPluginInfo> NewPlugins { get { return _newPlugins; } }
        public IReadOnlyList<IPluginInfo> ChangedPlugins { get { return _changedPlugins; } }
        public IReadOnlyList<IPluginInfo> DeletedPlugins { get { return _deletedPlugins; } }

        public IReadOnlyList<IPluginConfigAccessorInfo> NewEditors { get { return _newEditors; } }
        public IReadOnlyList<IPluginConfigAccessorInfo> ChangedEditors { get { return _changedEditors; } }
        public IReadOnlyList<IPluginConfigAccessorInfo> DeletedEditors { get { return _deletedEditors; } }
        
        /// <summary>
        /// Gets the list of new discovered services (contains also services on error).
        /// </summary>
        public IReadOnlyList<IServiceInfo> NewServices { get { return _newServices; } }
        public IReadOnlyList<IServiceInfo> ChangedServices { get { return _changedServices; } }
        public IReadOnlyList<IServiceInfo> DeletedServices { get { return _deletedServices; } }

        /// <summary>
        /// Gets the list of appearing old plugins. They may be previously active plugins replaced by a newer version
        /// or a "new" old plugin (when both plugins plugin versions are discovered at once).
        /// </summary>
        public IReadOnlyList<IPluginInfo> NewOldPlugins { get { return _newOldPlugins; } }
        public IReadOnlyList<IPluginInfo> DeletedOldPlugins { get { return _deletedOldPlugins; } }

        /// <summary>
        /// Gets the list of missing assemblies.
        /// </summary>
        public IReadOnlyList<string> NewDisappearedAssemblies { get { return _newMissingAssemblies; } }
        public IReadOnlyList<string> DeletedDisappearedAssemblies { get { return _deletedMissingAssemblies; } }

        public int ChangeCount { get; private set; }

        public DiscoverDoneEventArgs(
            IReadOnlyList<IAssemblyInfo> newAssemblies,IReadOnlyList<IAssemblyInfo> changedAssemblies,IReadOnlyList<IAssemblyInfo> deletedAssemblies,
            IReadOnlyList<IPluginInfo> newPlugins,IReadOnlyList<IPluginInfo> changedPlugins,IReadOnlyList<IPluginInfo> deletedPlugins,
            IReadOnlyList<IPluginConfigAccessorInfo> newEditors, IReadOnlyList<IPluginConfigAccessorInfo> changedEditors, IReadOnlyList<IPluginConfigAccessorInfo> deletedEditors,
            IReadOnlyList<IServiceInfo> newServices,IReadOnlyList<IServiceInfo> changedServices,IReadOnlyList<IServiceInfo> deletedServices, 
            IReadOnlyList<IPluginInfo> newOldPlugins,IReadOnlyList<IPluginInfo> deletedOldPlugins,
            IReadOnlyList<string> newMissingAssemblies,IReadOnlyList<string> deletedMissingAssemblies)
        {
            _newAssemblies = newAssemblies;
            _changedAssemblies = changedAssemblies;
            _deletedAssemblies = deletedAssemblies;
            _newPlugins = newPlugins;
            _changedPlugins = changedPlugins;
            _deletedPlugins = deletedPlugins;
            ChangeCount = newAssemblies.Count + changedAssemblies.Count + deletedAssemblies.Count + newPlugins.Count + changedPlugins.Count + deletedPlugins.Count;
            
            _newEditors = newEditors;
            _changedEditors = changedEditors;
            _deletedEditors = deletedEditors;
            _newServices = newServices;
            _changedServices = changedServices;
            _deletedServices = deletedServices;
            ChangeCount += newEditors.Count + changedEditors.Count + deletedEditors.Count + newServices.Count + changedServices.Count + deletedServices.Count;
            
            _newOldPlugins = newOldPlugins;
            _deletedOldPlugins = deletedOldPlugins;
            _newMissingAssemblies = newMissingAssemblies;
            _deletedMissingAssemblies = deletedMissingAssemblies;
            ChangeCount += newOldPlugins.Count + deletedOldPlugins.Count + newMissingAssemblies.Count + deletedMissingAssemblies.Count;
        }
    }
}
