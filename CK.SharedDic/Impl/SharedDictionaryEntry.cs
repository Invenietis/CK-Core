#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SharedDictionaryEntry.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Core;
using System.Diagnostics;

namespace CK.SharedDic
{
    internal sealed class SharedDictionaryEntry : IConfigEntry, IObjectPluginAssociation, IConfigObjectEntry, IConfigPluginEntry
    {
        readonly object _obj;
        readonly INamedVersionedUniqueId _pluginId;
        readonly string _key;
        object _value;

        public object Obj
        {
            get { return _obj; }
        }

        public INamedVersionedUniqueId PluginId
        {
            get { return _pluginId; }
        }

        public string Key
        {
            get { return _key; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public SharedDictionaryEntry( object o, INamedVersionedUniqueId p, string k )
        {
            Debug.Assert( o != null && p != null );
            _obj = o;
            _pluginId = p;
            _key = k;
        }

        public SharedDictionaryEntry( object o, INamedVersionedUniqueId p, string k, object value )
        {
            Debug.Assert( o != null && p != null );
            _obj = o;
            _pluginId = p;
            _key = k;
            _value = value;
        }

        public override bool Equals( object o )
        {
            Debug.Assert( _key != null, "Entries are compared by its Equals method only for real entries (where key is not null) when searching in SharedDictionaryImpl._values. Search for FinalDictionary use the FinalDictionaryComparer that ignores the key." );
            SharedDictionaryEntry e = o as SharedDictionaryEntry;
            if( e == null ) return false;
            return _obj == e._obj && _pluginId == e._pluginId && _key == e._key;
        }

        public override int GetHashCode()
        {
            Debug.Assert( _key != null, "Entries are compared by its Equals method only for real entries (where key is not null) when searching in SharedDictionaryImpl._values. Search for FinalDictionary use the FinalDictionaryComparer that ignores the key." );
            return _obj.GetHashCode() ^ _pluginId.GetHashCode() ^ _key.GetHashCode();
        }
    }

}
