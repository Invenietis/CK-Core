#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Keyboard.Modes.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    sealed partial class Keyboard
    {
        IKeyboardMode _availableMode;
        IKeyboardMode _currentMode;

        public event EventHandler<KeyboardModeChangingEventArgs> AvailableModeChanging;

        public event EventHandler<KeyboardModeChangedEventArgs> AvailableModeChanged;

        public event EventHandler<KeyboardModeChangingEventArgs> CurrentModeChanging;

        public event EventHandler<KeyboardModeChangedEventArgs> CurrentModeChanged;

        public IKeyboardMode AvailableMode
        {
            get { return _availableMode; }
            set
            {
                if( value == null ) value = Context.EmptyMode;
                if( _availableMode != value )
                {
                    if( value.Context != Context ) throw new ArgumentException( R.ModeFromAnotherContext );

                    EventHandler<KeyboardModeChangingEventArgs> changing = AvailableModeChanging;
                    if( changing != null )
                    {
                        KeyboardModeChangingEventArgs e = new KeyboardModeChangingEventArgs( this, value );
                        changing( this, e );
                        if( e.Cancel ) return;
                    }

                    // If CurrentMode is impacted, tries to change it: the CurrentModeChanging
                    // will be triggered and if it is cancelled we must cancel this setting of AvailableMode.
                    IKeyboardMode newCurrentMode = _currentMode.Intersect( value );
                    if( newCurrentMode != _currentMode && !SetCurrentMode( newCurrentMode ) ) return;

                    // The AvailableModeChanging (and CurrentModeChanging if needed) have been fired 
                    // and the setting has not been canceled: we can take the change into account.
                    // If atomic modes have been removed, we must handle the actual keys accordingly
                    // (note that the impact on the current ones has already been handled by the CurrentMode
                    // setting above).
                    IKeyboardMode previous = _availableMode;
                    _availableMode = value;

                    IKeyboardMode removed = _availableMode.Remove( value );
                    if( removed.AtomicModes.Count > 0 )
                    {
                        foreach( Key k in Keys )
                        {
                            k.OnAvailableModeRemoved( removed.AtomicModes );
                        }
                        _layouts.OnAvailableModeRemoved( removed.AtomicModes );
                    }

                    EventHandler<KeyboardModeChangedEventArgs> changed = AvailableModeChanged;
                    if( changed != null ) changed( this, new KeyboardModeChangedEventArgs( this, previous ) );
                    Context.SetKeyboardContextDirty();
                }
            }
        }

        public IKeyboardMode CurrentMode
        {
            get { return _currentMode; }
            set
            {
                if( value == null ) value = Context.EmptyMode;
                if( _currentMode != value )
                {
                    if( value.Context != Context ) throw new ArgumentException( R.ModeFromAnotherContext );
                    SetCurrentMode( value );
                    Context.SetKeyboardContextDirty();
                }
            }
        }

        bool SetCurrentMode( IKeyboardMode newCurrentMode )
        {
            Debug.Assert( _currentMode != newCurrentMode && newCurrentMode.Context == this.Context, "These checks have been done before." );

            EventHandler<KeyboardModeChangingEventArgs> changing = CurrentModeChanging;
            if( changing != null )
            {
                KeyboardModeChangingEventArgs e = new KeyboardModeChangingEventArgs( this, newCurrentMode );
                changing( this, e );
                if( e.Cancel ) return false;
            }

            IKeyboardMode previous = _currentMode;
            _currentMode = newCurrentMode;

            // Listeners had the opportunity to cancel the setting. We can now
            // impact the choice of the actual key and actual key layout for each key.
            foreach( Key k in Keys )
            {
                k.OnCurrentModeChanged();
            }
            Layouts.OnCurrentModeChanged();

            EventHandler<KeyboardModeChangedEventArgs> changed = CurrentModeChanged;
            if( changed != null ) changed( this, new KeyboardModeChangedEventArgs( this, previous ) );
            
            return true;
        }
    }
}
