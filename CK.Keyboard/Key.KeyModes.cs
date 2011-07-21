#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Key.KeyModes.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    /// <summary>
    /// Auto implementation of the <see cref="IKeyModeCollection"/> for <see cref="IKey.KeyModes"/> property.
    /// </summary>
    sealed partial class Key : ModeObjectRoot<KeyMode, IKeyMode>, IKeyModeCollection
    {
        public event EventHandler<KeyModeEventArgs> KeyModeCreated;

        public event EventHandler<KeyModeEventArgs> KeyModeDestroyed;

        public event EventHandler<KeyModeModeChangedEventArgs> KeyModeModeChanged;

        IKeyModeCollection IKey.KeyModes
        {
            get { return this; }
        }

        IKey IKeyModeCollection.Key
        {
            get { return this; }
        }

        internal IReadOnlyCollection<KeyMode> KeyModes
        {
            get { return Objects; }
        }

        IKeyModeCurrent IKey.Current
        {
            get { return Current; }
        }
       
        IKeyMode IKeyModeCollection.Create( IKeyboardMode mode )
        {
            return FindOrCreate( mode );
        }

        public override bool Contains( object item )
        {
            IKeyMode km = item as IKeyMode;
            return km != null ? km.Key == this : false;
        }

        protected override KeyMode DoCreate( IKeyboardMode mode )
        {
            return new KeyMode( this, mode );
        }

        protected override void OnCreated( KeyMode ak )
        {
            KeyModeEventArgs e = new KeyModeEventArgs( ak );
            if( KeyModeCreated != null ) KeyModeCreated( this, e );
            Keyboard.OnKeyModeCreated( e );
        }

        protected override void OnDestroyed( KeyMode ak )
        {
            KeyModeEventArgs e = new KeyModeEventArgs( ak );
            EventHandler<KeyModeEventArgs> destroyed = KeyModeDestroyed;
            if( destroyed != null ) destroyed( this, e );
            Keyboard.OnKeyModeDestroy( e );
        }

        protected override IKeyboardMode GetCurrentMode()
        {
            return Keyboard.CurrentMode;
        }

        protected override IKeyboardMode GetAvailableMode()
        {
            return Keyboard.AvailableMode;
        }

        protected override void OnCurrentChanged()
        {
            OnPropertyChanged( new KeyPropertyChangedEventArgs( this, "Current" ) );
        }

        protected override void OnModeChanged( KeyMode ak, IKeyboardMode prevMode )
        {
            KeyModeModeChangedEventArgs ev = new KeyModeModeChangedEventArgs( ak, prevMode );
            EventHandler<KeyModeModeChangedEventArgs> changed = KeyModeModeChanged;
            if( changed != null ) changed( this, ev );
            Keyboard.OnKeyModeModeChanged( ev );
        }

        /// <summary>
        /// Internal relay for strongly typed actual key changes.
        /// </summary>
        internal void OnActualPropertyChanged( KeyModePropertyChangedEventArgs e )
        {
            OnPropertyChanged( e );
        }
    }
}
