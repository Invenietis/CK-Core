#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\LayoutKey.LayoutKeyModes.cs) is part of CiviKey. 
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
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    sealed partial class LayoutKey : ModeObjectRoot<LayoutKeyMode, ILayoutKeyMode>, ILayoutKeyModeCollection
    {
        
        /// <summary>
        /// Internal relay for strongly typed actual key layout changes.
        /// </summary>
        internal void OnActualLayoutPropertyChanged( LayoutKeyModePropertyChangedEventArgs e )
        {
            Key.OnPropertyChanged( e );
        }

        public ILayoutKeyModeCollection LayoutKeyModes
        {
            get { return this; }
        }

        public override bool Contains( object item )
        {
            ILayoutKeyMode lkm = item as ILayoutKeyMode;
            return lkm != null ? lkm.LayoutKey == this : false;
        }

        internal LayoutKeyMode Create(IKeyboardMode mode)
        {
            return FindOrCreate( mode );
        }

        ILayoutKeyMode ILayoutKeyModeCollection.Create( IKeyboardMode mode )
        {
            return FindOrCreate( mode );
        }

        protected override LayoutKeyMode DoCreate( IKeyboardMode mode )
        {
            return new LayoutKeyMode( this, mode );
        }

        protected override void OnCreated( LayoutKeyMode lkm )
        {
            LayoutKeyModeEventArgs e = new LayoutKeyModeEventArgs( lkm );
            EventHandler<LayoutKeyModeEventArgs> created = LayoutKeyModeCreated;
            if( created != null ) created( this, e );
            Keyboard.OnLayoutKeyModeCreated( e );
        }

        protected override void OnDestroyed( LayoutKeyMode lkm )
        {
            LayoutKeyModeEventArgs e = new LayoutKeyModeEventArgs( lkm );
            EventHandler<LayoutKeyModeEventArgs> destroyed = LayoutKeyModeDestroyed;
            if( destroyed != null ) destroyed( this, e );
            Keyboard.OnLayoutKeyModeDestroy( e );
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
            Key.OnPropertyChanged( new LayoutKeyPropertyChangedEventArgs( this, "CurrentLayout" ) );
        }

        protected override void OnModeChanged( LayoutKeyMode o, IKeyboardMode prevMode )
        {
            LayoutKeyModeModeChangedEventArgs e = new LayoutKeyModeModeChangedEventArgs( o, prevMode );
            EventHandler<LayoutKeyModeModeChangedEventArgs> changed = LayoutKeyModeModeChanged;
            if( changed != null ) changed( this, e );
            Keyboard.OnLayoutKeyModeModeChanged( e );
        }        

        #region Auto implementation of the ILayoutKeyModeCollection

        public event EventHandler<LayoutKeyModeEventArgs>  LayoutKeyModeCreated;

        public event EventHandler<LayoutKeyModeModeChangedEventArgs>  LayoutKeyModeModeChanged;

        public event EventHandler<LayoutKeyModeEventArgs>  LayoutKeyModeDestroyed;

        ILayoutKey ILayoutKeyModeCollection.LayoutKey
        {
            get { return this; }
        }

        #endregion
    }
}
