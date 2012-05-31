#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\PluginConfigByPlugin.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin.Config;

namespace CK.SharedDic
{
    internal sealed class PluginConfigByPlugin : List<SharedDictionaryEntry>
    {
        public readonly INamedVersionedUniqueId PluginId;

        internal PluginConfigByPlugin( INamedVersionedUniqueId pluginId )
        {
            PluginId = pluginId;
        }
        
        internal void ForEach( object o, Action<SharedDictionaryEntry> a )
        {
            for( int i = 0; i < Count; ++i )
            {
                SharedDictionaryEntry e = this[i];
                if( e.Obj == o ) a( e );
            }
        }

    }
}
