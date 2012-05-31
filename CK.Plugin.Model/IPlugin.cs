#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\IPlugin.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// This interface defines the minimal properties and behavior of a plugin.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// This method initializes the plugin: own resources must be acquired and running conditions should be tested.
        /// No interaction with other plugins must occur (interactions must be in <see cref="Start"/>).
        /// </summary>
        /// <param name="info">Enables the implementation to give detailed information in case of error.</param>
        /// <returns>True on success. When returning false, <see cref="IPluginSetupInfo"/> should be used to return detailed explanations.</returns>
        bool Setup( IPluginSetupInfo info );

        /// <summary>
        /// This method must start the plugin: it is called only if <see cref="Setup"/> returned true.
        /// Implementations can interact with other components (such as subscribing to their events).
        /// </summary>
        void Start();

        /// <summary>
        /// This method uninitializes the plugin (it is called after <see cref="Stop"/>).
        /// Implementations MUST NOT interact with any other external components: only internal resources should be freed.
        /// </summary>
        void Teardown();

        /// <summary>
        /// This method is called by the host when the plugin must not be running anymore.
        /// Implementations can interact with other components (such as unsubscribing to their events). 
        /// <see cref="Teardown"/> will be called to finalize the stop.
        /// </summary>
        void Stop();

    }
}
