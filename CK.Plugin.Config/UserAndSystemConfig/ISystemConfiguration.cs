#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\ISystemConfiguration.cs) is part of CiviKey. 
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
using System.ComponentModel;
namespace CK.Plugin.Config
{
    /// <summary>
    /// System related configuration. 
    /// This is the first level of configuration that applies to all users.
    /// </summary>
    public interface ISystemConfiguration : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets all the <see cref="IUriHistory">user profiles</see> previously used by the system.
        /// </summary>
        IUriHistoryCollection UserProfiles { get; }

        /// <summary>
        /// Gets or sets the profile that must be considered as the current one.
        /// When setting it, the value must already belong to the profiles in <see cref="UserProfiles"/> (otherwise an exception is thrown)
        /// and it becomes the first one.
        /// </summary>
        IUriHistory CurrentUserProfile { get; set; }

        /// <summary>
        /// Gets the previous user. 
        /// Returns null if the current user is the only one used since application start.
        /// </summary>
        IUriHistory PreviousUserProfile { get; }

        /// <summary>
        /// Gets <see cref="IPluginStatus">plugins status</see> configured at the system level.
        /// </summary>
        IPluginStatusCollection PluginsStatus { get; }

        /// <summary>
        /// Gets the host dictionary for System wide configuration.
        /// </summary>
        IObjectPluginConfig HostConfig { get; }

    }
}