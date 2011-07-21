#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\IPluginConfigAccessor.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.ComponentModel;

namespace CK.Plugin.Config
{
    public class UserProfileCollectionChangedEventArgs : EventArgs
    {
        public ChangeStatus Action { get; private set; }

        public IUserProfileCollection Collection { get; private set; }

        public string Name { get; private set; }

        public ConfigSupportType Type { get; private set; }

        public string Address { get; private set; }

        public UserProfileCollectionChangedEventArgs( IUserProfileCollection c, ChangeStatus action, string name, ConfigSupportType type, string address )
        {
            Collection = c;
            Action = action;
            Name = name;
            Type = type;
            Address = address;
        }
    }

    public class UserProfileCollectionChangingEventArgs : CancelEventArgs
    {
        public ChangeStatus Action { get; private set; }

        public IUserProfileCollection Collection { get; private set; }

        public string Name { get; private set; }

        public ConfigSupportType Type { get; private set; }

        public string Address { get; private set; }

        public UserProfileCollectionChangingEventArgs( IUserProfileCollection c, ChangeStatus action, string name, ConfigSupportType type, string address )
        {
            Collection = c;
            Action = action;
            Name = name;
            Type = type;
            Address = address;
        }
    }

    /// <summary>
    /// Used by the <see cref="ISystemConfiguration"/> to keep an history of all user profiles used.
    /// </summary>
    public interface IUserProfileCollection : IReadOnlyCollection<IUserProfile>
    {
        event EventHandler<UserProfileCollectionChangingEventArgs> Changing;

        event EventHandler<UserProfileCollectionChangedEventArgs> Changed;

        /// <summary>
        /// Gets a <see cref="IUserProfile"/> by its address.
        /// </summary>
        IUserProfile Find( string address );

        /// <summary>
        /// Gets the last <see cref="IUserProfile"/> used.
        /// At runtime this profile is the current profile.
        /// </summary>
        IUserProfile LastProfile { get; }

        /// <summary>
        /// Add or set a new <see cref="IUserProfile"/> in the collection. If the profile already exists, it will be updated with the given data.
        /// </summary>
        IUserProfile AddOrSet( string name, string address, ConfigSupportType type, bool setAsLast );
    }
}
