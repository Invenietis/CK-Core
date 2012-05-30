#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Host\IHostInformation.cs) is part of CiviKey. 
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
using System.IO;
using CK.Plugin.Config;

namespace CK.Context
{
    
    /// <summary>
    /// Exposes standard host information.
    /// </summary>
    public interface IHostInformation
    {
        /// <summary>
        /// Gets the host configuration associated to the current user.
        /// </summary>
        IObjectPluginConfig UserConfig { get; }

        /// <summary>
        /// Gets the host configuration associated to the system.
        /// <remarks>
        IObjectPluginConfig SystemConfig { get; }

        /// <summary>
        /// Gets the System configuration's file path.
        /// </summary>
        /// <returns></returns>
        Uri GetSystemConfigAddress();

        /// <summary>
        /// Gets the name of the application. Civikey-Standard for instance for the Civikey Standard application. 
        /// It is an identifier (no /, \ or other special characters in it: see <see cref="Path.GetInvalidPathChars"/>).
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Gets an optional second name (can be null).
        /// When not null, it is an identifier just like <see cref="AppName"/>.
        /// </summary>
        string SubAppName { get; }

        /// <summary>
        /// Gets the current application version.
        /// </summary>
        Version AppVersion { get; }

        /// <summary>
        /// Gets the full path of application-specific data repository for the current user if 
        /// the host handles it. Null otherwise.
        /// When not null, it ends with <see cref="Path.DirectorySeparatorChar"/> and the directory exists.
        /// </summary>
        string ApplicationDataPath { get; }

        /// <summary>
        /// Gets the full path of application-specific data repository for all users if 
        /// the host handles it. Null otherwise.
        /// When not null, it ends with <see cref="Path.DirectorySeparatorChar"/> and the directory exists.
        /// </summary>
        string CommonApplicationDataPath { get; }


    }
}
