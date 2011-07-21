#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\ConfigManager.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion


namespace CK.Plugin.Config
{
    /// <summary>
    /// Gets the different types of configuration support.
    /// </summary>
    public enum ConfigSupportType
    {
        None = 0,
        File = 1,
        Other = 2
    }

    /// <summary>
    /// Describes what's a user profile
    /// </summary>
    public interface IUserProfile
    {
        /// <summary>
        /// Friendly name of the profile.
        /// Defaults to "config-GUID".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Support type of the profile (File, ...)
        /// </summary>
        ConfigSupportType Type { get; }

        /// <summary>
        /// Gets the physical address, if it's a File it can be "C:/CiviKey/Contexts/AzertyLike.xml".
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Sets a new name for the profile.
        /// </summary>
        /// <param name="newName">New name of the profile</param>
        void Rename( string newName );

        /// <summary>
        /// Destroys the profile, and remove it from its parent collection.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets whether this profile is the last used profile (and the current).
        /// </summary>
        bool IsLastProfile { get; }
    }
}