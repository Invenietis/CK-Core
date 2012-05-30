#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\IConfigManager.cs) is part of CiviKey. 
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
using System.ComponentModel;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Defines simple functionalities related to configuration. 
    /// The <see cref="Extended"/> property offers more complete configuration management.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Gets an extended interface that offers methods to manage configuration.
        /// </summary>
        IConfigManagerExtended Extended { get; }

        /// <summary>
        /// Gets the system configuration object.
        /// </summary>
        ISystemConfiguration SystemConfiguration { get; }

        /// <summary>
        /// Gets the current user configuration object.
        /// </summary>
        IUserConfiguration UserConfiguration { get; }

        /// <summary>
        /// Synchronized view of the <see cref="SystemConfiguration"/> and <see cref="UserConfiguration"/>
        /// regarding plugin configuration.
        /// </summary>
        ISolvedPluginConfiguration SolvedPluginConfiguration { get; }

    }

}
