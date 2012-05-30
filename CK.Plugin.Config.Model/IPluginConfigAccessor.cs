#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IPluginConfigAccessor.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{
    /// <summary>
    /// This interface is the primary interface that enables a plugin to interact with the 
    /// configuration associated to any object in the system.
    /// </summary>
    public interface IPluginConfigAccessor
    {
        /// <summary>
        /// Fires whenever a configuration related to the plugin changes.
        /// </summary>
        event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        /// <summary>
        /// Gets the system configuration for the plugin. This configuration is bound to
        /// the system itself and is shared among users.
        /// </summary>
        IObjectPluginConfig System { get; }

        /// <summary>
        /// Gets the user configuration for the plugin. This configuration is bound to
        /// the current user.
        /// </summary>
        IObjectPluginConfig User { get; }

        /// <summary>
        /// Gets the context configuration for the plugin. This configuration is bound to
        /// the current context. Other context dependant configurations can be obtained for any object of the context.
        /// </summary>
        IObjectPluginConfig Context { get; }

        /// <summary>
        /// Gets or creates the configuration related to any object managed by the plugin.
        /// </summary>
        /// <param name="o">Object for which configuration must be obtained.</param>
        /// <returns>An interface to the configurations of the object for the plugin.</returns>
        IObjectPluginConfig this[object o] { get; }
    }

}
