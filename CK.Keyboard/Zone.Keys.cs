#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Zone.Keys.cs) is part of CiviKey. 
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    sealed partial class Zone : IKeyCollection
    {
        List<Key> _keys;

        public event EventHandler<KeyEventArgs> KeyCreated;

        public event EventHandler<KeyEventArgs> KeyDestroyed;

        public event EventHandler<KeyMovedEventArgs> KeyMoved;

        IKeyCollection IZone.Keys
        {
            get { return this; }
        }

        internal IEnumerable<Key> Keys
        {
            get { return _keys; }
        }

        IZone IKeyCollection.Zone
        {
            get { return this; }
        }

        IKey IKeyCollection.Create()
        {
            return Create( _keys.Count );
        }

        IKey IKeyCollection.Create( int index )
        {
            return Create( index );
        }

        internal Key Create( int index )
        {
            if( index < 0 ) index = 0;
            else if( index > _keys.Count ) index = _keys.Count;
            Key k = new Key( this, index );
            _keys.Insert( index, k );
            while( ++index < _keys.Count ) _keys[index].SetIndex( this, index );
            KeyEventArgs e = new KeyEventArgs( k );
            if( KeyCreated != null ) KeyCreated( this, e );
            _zones.Keyboard.OnKeyCreated( e );
            Context.SetKeyboardContextDirty();
            return k;
        }

        public int IndexOf( object item )
        {
            IKey k = item as IKey;
            return k != null && k.Zone == this ? k.Index : -1;
        }

        IKey IReadOnlyList<IKey>.this[int i]
        {
            get { return _keys[i]; }
        }

        internal Key this[int i]
        {
            get { return _keys[i]; }
        }

        bool IReadOnlyCollection<IKey>.Contains( object item )
        {
            IKey k = item as IKey;
            return k != null ? k.Zone == this : false;
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        IEnumerator<IKey> IEnumerable<IKey>.GetEnumerator()
        {
            return Wrapper<IKey>.CreateEnumerator( _keys );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        internal void OnDestroy( Key key )
        {
            Debug.Assert( key.Zone == this, "This is one of our key." );
            Debug.Assert( _keys[key.Index] == key, "Indices are always synchronized" );
            int idx = key.Index;
            _keys.RemoveAt( key.Index );
            int count = _keys.Count;
            while( idx < count )
            {
                _keys[idx].SetIndex( this, idx );
                ++idx;
            }
            KeyEventArgs e = new KeyEventArgs( key );
            if( KeyDestroyed != null ) KeyDestroyed( this, e );
            _zones.Keyboard.OnKeyDestroyed( e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnMove( Key key, int i )
        {
            Debug.Assert( key != null && key.Zone == this, "This is one of our key." );
            if( i < 0 ) i = 0;
            else if( i > _keys.Count ) i = _keys.Count;
            int prevIndex = key.Index;
            if( i == prevIndex ) return;

            Debug.Assert( _keys[prevIndex] == key, "Indices are always synchronized." );
            _keys.Insert( i, key );
            int min, max;
            if( i > prevIndex )
            {
                min = prevIndex;
                max = i;
                Debug.Assert( _keys[prevIndex] == key, "Remove the key" );
                _keys.RemoveAt( prevIndex );
            }
            else
            {
                min = i;
                max = prevIndex + 1;
                Debug.Assert( _keys[max] == key, "Remove the key" );
                _keys.RemoveAt( max );
            }
            for( int iNew = min; iNew < max; ++iNew )
            {
                _keys[iNew].SetIndex( this, iNew );
            }
            KeyMovedEventArgs e = new KeyMovedEventArgs( key, prevIndex );
            if( KeyMoved != null ) KeyMoved( this, e );
            Keyboard.OnKeyMoved( e );
            Context.SetKeyboardContextDirty();
        }

    }
}
