#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\UserAndSystemConfig\LiveConfiguration\ConfigUserAction.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{
    /// <summary>
    /// Defines the configuration made by the user for a plugin (or a service).
    /// </summary>
    public enum ConfigUserAction
    {
        /// <summary>
        /// User has not explictely started nor stopped the plugin.
        /// </summary>
        None = 0,

        /// <summary>
        /// User explicitely started the plugin.
        /// </summary>
        Started = 1,

        /// <summary>
        /// User explicitely stopped the plugin.
        /// </summary>
        Stopped = 2
    }
}
