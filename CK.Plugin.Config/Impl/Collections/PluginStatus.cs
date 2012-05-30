#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\Collections\PluginStatus.cs) is part of CiviKey. 
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
    internal class PluginStatus : IPluginStatus
    {
        PluginStatusCollection _holder;
        ConfigPluginStatus _status;
        Guid _pluginId;

        public Guid PluginId
        {
            get { return _pluginId; }
            set { _pluginId = value; }
        }

        public ConfigPluginStatus Status
        { 
            get { return _status; } 
            set 
            {
                if( _status != value &&  _holder.CanChange( ChangeStatus.Update, _pluginId, value ) )
                {
                    _status = value;
                    _holder.Change( ChangeStatus.Update, _pluginId, value );
                }
            } 
        }

        public void Destroy()
        {
            if( _holder.OnDestroy( this ) ) _holder = null;
        }

        public PluginStatus(PluginStatusCollection holder, Guid pluginId, ConfigPluginStatus status)
        {
            _holder = holder;
            _pluginId = pluginId;
            _status = status;
        }
    }
}
