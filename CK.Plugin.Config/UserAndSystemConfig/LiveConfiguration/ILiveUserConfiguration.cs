#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\LiveConfiguration\ILiveUserConfiguration.cs) is part of CiviKey. 
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
    /// Holds the UserActions for each plugin
    /// </summary>
    public interface ILiveUserConfiguration : IReadOnlyCollection<ILiveUserAction>
    {
        event EventHandler<LiveUserConfigurationChangingEventArgs> Changing;

        event EventHandler<LiveUserConfigurationChangedEventArgs> Changed;

        /// <summary>
        /// Sets a <see cref="ConfigUserAction"/> to a given plugin.
        /// </summary>
        ILiveUserAction SetAction( Guid pluginId, ConfigUserAction actionType );

        /// <summary>
        /// Gets the <see cref="ConfigUserAction"/> related to the given plugin.
        /// </summary>
        ConfigUserAction GetAction( Guid pluginId );

        /// <summary>
        /// Remove the <see cref="ConfigUserAction"/> for the given plugin.
        /// </summary>
        void ResetAction( Guid pluginId );
    }
}
