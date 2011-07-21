#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Keyboard.Keys.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    sealed partial class Keyboard : IReadOnlyList<IKey>
    {
        public event EventHandler<KeyEventArgs> KeyCreated;
        public event EventHandler<KeyEventArgs> KeyDestroyed;
        public event EventHandler<KeyMovedEventArgs> KeyMoved;

        public event EventHandler<KeyModeEventArgs> KeyModeCreated;
        public event EventHandler<KeyModeEventArgs> KeyModeDestroyed;
        public event EventHandler<KeyModeModeChangedEventArgs> KeyModeModeChanged;

        public event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeCreated;
        public event EventHandler<LayoutKeyModeEventArgs> LayoutKeyModeDestroyed;
        public event EventHandler<LayoutKeyModeModeChangedEventArgs> LayoutKeyModeModeChanged;

        public event EventHandler<KeyPropertyChangedEventArgs> KeyPropertyChanged;
        public event EventHandler<KeyPropertyChangedEventArgs> KeyOtherPropertyChanged;

        public event EventHandler<KeyInteractionEventArgs> KeyUp;
        public event EventHandler<KeyInteractionEventArgs> KeyDown;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public IReadOnlyList<IKey> Keys 
        {
            get { return this; } 
        }

        internal IEnumerable<KeyMode> KeyModes
        {
            get
            {
                foreach( Key k in Keys )
                    foreach( KeyMode ak in k.KeyModes )
                        yield return ak;
            }
        }

        internal IEnumerable<KeyMode> CurrentKeyModes
        {
            get
            {
                foreach( Key k in Keys )
                    yield return k.Current;
            }
        }

        internal IEnumerable<LayoutKeyMode> LayoutKeyModes
        {
            get
            {
                foreach( Layout l in Layouts )
                    foreach( KeyMode ak in KeyModes )
                    {
                        LayoutKeyMode akl = l.Find( ak );
                        if( akl != null ) yield return akl;
                    }
            }
        }

        internal void OnKeyMoved( KeyMovedEventArgs e )
        {
            EventHandler<KeyMovedEventArgs> moved = KeyMoved;
            if( moved != null ) moved( this, e );
        }

        internal void OnKeyCreated( KeyEventArgs e )
        {
            EventHandler<KeyEventArgs> created = KeyCreated;
            if( created != null ) created( this, e );
        }

        internal void OnKeyDestroyed( KeyEventArgs e )
        {
            EventHandler<KeyEventArgs> destroyed = KeyDestroyed;
            if( destroyed != null ) destroyed( this, e );
        }

        internal void OnKeyModeDestroy( KeyModeEventArgs e )
        {
            EventHandler<KeyModeEventArgs> destroyed = KeyModeDestroyed;
            if( destroyed != null ) destroyed( this, e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnKeyModeCreated( KeyModeEventArgs e )
        {
            EventHandler<KeyModeEventArgs> created = KeyModeCreated;
            if( created != null ) created( this, e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnLayoutKeyModeCreated( LayoutKeyModeEventArgs e )
        {
            EventHandler<LayoutKeyModeEventArgs> created = LayoutKeyModeCreated;
            if( created != null ) created( this, e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnLayoutKeyModeModeChanged( LayoutKeyModeModeChangedEventArgs e )
        {
            EventHandler<LayoutKeyModeModeChangedEventArgs> changed = LayoutKeyModeModeChanged;
            if( changed != null ) changed( this, e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnLayoutKeyModeDestroy( LayoutKeyModeEventArgs e )
        {
            EventHandler<LayoutKeyModeEventArgs> destroyed = LayoutKeyModeDestroyed;
            if( destroyed != null ) destroyed( this, e );
            Context.SetKeyboardContextDirty();
        }

        internal void OnKeyModeModeChanged( KeyModeModeChangedEventArgs e )
        {
            EventHandler<KeyModeModeChangedEventArgs> changed = KeyModeModeChanged;
            if( changed != null ) changed( this, e );
        }

        internal void OnKeyPropertyChanged( KeyPropertyChangedEventArgs e )
        {
            if( e.PropertyHolder.IsCurrent )
            {
                EventHandler<KeyPropertyChangedEventArgs> changed = KeyPropertyChanged;
                if( changed != null ) changed( this, e );
            }
            else
            {
                EventHandler<KeyPropertyChangedEventArgs> changed = KeyOtherPropertyChanged;
                if( changed != null ) changed( this, e );
            }
        }

        internal void OnKeyDown( KeyInteractionEventArgs e )
        {
            EventHandler<KeyInteractionEventArgs> h = KeyDown;
            if( h != null ) h( this, e );
        }

        internal void OnKeyUp( KeyInteractionEventArgs e )
        {
            EventHandler<KeyInteractionEventArgs> h = KeyUp;
            if( h != null ) h( this, e );
        }

        internal void OnKeyPressed( KeyPressedEventArgs e )
        {
            EventHandler<KeyPressedEventArgs> h = KeyPressed;
            if( h != null ) h( this, e );
        }

        #region IReadOnlyList<IKey> Members

        /// <summary>
        /// Zones are not ordered so indicies can change after each change in zones collection.
        /// </summary>
        int IReadOnlyList<IKey>.IndexOf( object key )
        {
            IKey k = key as IKey;
            if( k != null )
            {
                int count = 0;
                foreach( Zone z in _zones )
                {
                    if( k.Zone == z ) return count + k.Index;
                    count += z.Count;
                }
            }
            return -1;
        }

        IKey IReadOnlyList<IKey>.this[int index]
        {
            get
            {
                int count = 0;
                foreach( Zone z in _zones )
                {
                    if( z.Count > 0 )
                    {
                        if( index - count < z.Count )
                            return z[index - count];
                        count += z.Count;
                    }
                }
                return null;
            }
        }

        bool IReadOnlyCollection<IKey>.Contains( object key )
        {
            IKey k = key as IKey;
            return k != null ? k.Keyboard == this : false;
        }

        int IReadOnlyCollection<IKey>.Count
        {
            get 
            {
                int count = 0;
                foreach( Zone z in _zones )
                    count += z.Count;
                return count;
            }
        }

        /// <summary>
        /// Use public implementation (will be visible only inside our assembly since the class
        /// itself is internal).
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IKey> GetEnumerator()
        {
            foreach( Zone z in _zones )
                foreach( Key k in z.Keys )
                    yield return k;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
