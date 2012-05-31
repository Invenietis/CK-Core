#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\IUserConfiguration.cs) is part of CiviKey. 
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

using System.ComponentModel;
namespace CK.Plugin.Config
{
    /// <summary>
    /// User related configuration. 
    /// This is the second level of configuration that comes above <see cref="ISystemConfiguration"/>.
    /// </summary>
    public interface IUserConfiguration : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets all the <see cref="IUriHistory">contexts</see> previously seeb by the user.
        /// </summary>
        IUriHistoryCollection ContextProfiles { get; }

        /// <summary>
        /// Gets or sets the context that must be considered as the current one.
        /// When setting it, the value must already belong to the profiles in <see cref="ContextProfiles"/> (otherwise an exception is thrown)
        /// and it becomes the first one.
        /// </summary>
        IUriHistory CurrentContextProfile { get; set; }

        /// <summary>
        /// Gets the host dictionary for user configuration.
        /// </summary>
        IObjectPluginConfig HostConfig { get; }
       
        /// <summary>
        /// Gets <see cref="IPluginStatus">plugins status</see> configured at the user level.
        /// </summary>
        IPluginStatusCollection PluginsStatus { get; }

        /// <summary>
        /// Gets the "live" configuration level. 
        /// Live configuration can override <see cref="PluginsStatus"/>.
        /// </summary>
        ILiveUserConfiguration LiveUserConfiguration { get; }

    }
}
