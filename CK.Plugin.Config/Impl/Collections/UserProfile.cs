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
    internal class UserProfile : IUserProfile
    {
        UserProfileCollection _holder;

        public UserProfile( UserProfileCollection holder, string name, ConfigSupportType type, string address )
        {
            _holder = holder;
            Name = name;
            Type = type;
            Address = address;
        }

        internal UserProfileCollection Holder { get { return _holder; } }

        public string Name { get; private set; }

        public ConfigSupportType Type { get; private set; }

        public string Address { get; private set; }

        public bool IsLastProfile { get { return _holder.LastProfile == this; } }

        public void Rename( string newName )
        {
            Name = newName;
            _holder.OnRename( this );
        }

        public void Destroy()
        {
            _holder.OnDestroy( this );
            _holder = null;
        }
    }
}
