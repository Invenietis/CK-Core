#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\Collections\UriHistory.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin.Config
{
    internal class UriHistory : IUriHistory
    {
        UriHistoryCollection _holder;
        Uri _address;
        string _displayName;
        int _index;
        
        public static readonly string UnknownPathPrefix = "x:/?";

        internal UriHistory( UriHistoryCollection holder, Uri address, int index )
        {
            Debug.Assert( holder != null && address != null );
            _holder = holder;
            _displayName = String.Empty;
            _address = address;
            _index = index;
        }

        internal UriHistoryCollection Holder { get { return _holder; } }

        public string DisplayName 
        {
            get { return _displayName; }
            set { _displayName = value ?? String.Empty; } 
        }

        public Uri Address 
        {
            get { return _address; }
            set 
            {
                if( _address != value )
                {
                    Uri previous = _address;
                    _address = value;
                    if( _holder != null ) _holder.OnSetAddress( previous, this );
                }
            }
        }

        public int Index
        {
            get { return _index; }
            set 
            {
                if( _index != value )
                {
                    if( _holder != null ) _holder.OnSetIndex( this, value );
                    else _index = value;
                }
            }
        }

        internal void SetIndex( int newIndex )
        {
            _index = newIndex;
        }

        public bool IsUnknown { get { return Address.LocalPath.StartsWith( UnknownPathPrefix ); } }

        public bool IsLastActive { get { return _holder != null ? _holder.Current == this : false; } }

    }
}
