#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Key.cs) is part of CiviKey. 
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
using System.Xml;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;
using CK.Plugin.Config;
using CK.Core;
using CK.Plugin;

namespace CK.Keyboard
{
    sealed partial class Key : IKey, IKeyPropertyHolder, IStructuredSerializable
    {
        Zone _zone;
        int _index;
        int _repeatCount;

        internal Key( Zone z, int index )
        {
            _zone = z;
            _index = index;
            _repeatCount = -1;
            Initialize( z.Context );
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _zone.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _zone.Keyboard; }
        }

        IZone IZoneElement.Zone
        {
            get { return _zone; }
        }

        /// <summary>
        /// By design, a key is always current since it does not 
        /// depend on the keyboard mode nor keyboard layout.
        /// </summary>
        bool IKeyPropertyHolder.IsCurrent
        {
            get { return true; }
        }

        IKey IKeyPropertyHolder.Key
        {
            get { return this; }
        }

        public int Index
        {
            get { return _index; }
            set { _zone.OnMove( this, value ); }
        }

        internal KeyboardContext Context
        {
            get { return _zone.Context; }
        }

        internal Keyboard Keyboard
        {
            get { return _zone.Keyboard; }
        }

        internal Zone Zone
        {
            get { return _zone; }
        }

        internal void SetIndex( Zone newZone, int i )
        {
            _zone = newZone;
            _index = i;
        }

		internal void DestroyConfig()
		{
            foreach( KeyMode k in KeyModes ) k.DestroyConfig();
			Context.ConfigContainer.Destroy( this );
		}

        public event EventHandler<KeyPropertyChangedEventArgs> KeyPropertyChanged;
        public event EventHandler<KeyPropertyChangedEventArgs> KeyOtherPropertyChanged;

        public ILayoutKey CurrentLayout
        {
            get { return _zone.Keyboard.CurrentLayout.FindOrCreate( this ); }
        }

        internal void OnPropertyChanged( KeyPropertyChangedEventArgs e )
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
            Keyboard.OnKeyPropertyChanged( e );
        }		

        public void Destroy()
        {
            if( _zone != null )
            {
                _zone.OnDestroy( this );
                _zone = null;
            }
        }

        public event EventHandler<KeyInteractionEventArgs> KeyDown;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        public event EventHandler<KeyInteractionEventArgs> KeyUp;

        public void Push()
        {
            if( _repeatCount >= 0 )
            {
                throw new InvalidOperationException( R.KeyPushWhenDown );
            }
            Debug.Assert( _repeatCount == -1, "Since Push has not been called yet, repeatCount must be -1." );
            _repeatCount = 0;
            KeyInteractionEventArgs e = new KeyInteractionEventArgs( this, Current.OnKeyDownCommands, KeyInteractionEventType.Down  );
            EventHandler<KeyInteractionEventArgs> keyDown = KeyDown;
            if( keyDown != null ) keyDown( this, e );
            Keyboard.OnKeyDown( e );
        }

        public bool IsDown
        {
            get { return _repeatCount >= 0; }
        }

        public void RepeatPressed()
        {
            if( _repeatCount < 0 )
            {
                throw new InvalidOperationException( R.KeyRepeatPressedWhenUp );
            }
            OnKeyPressed();
        }

        void OnKeyPressed()
        {
            KeyPressedEventArgs e = new KeyPressedEventArgs( this, Current.OnKeyPressedCommands, _repeatCount++ );
            EventHandler<KeyPressedEventArgs> keyPressed = KeyPressed;
            if( keyPressed != null ) keyPressed( this, e );
            Keyboard.OnKeyPressed( e );
        }

        public void Release()
        {
            Release( true );
        }

        public void Release( bool doPress )
        {
            if( _repeatCount < 0 )
            {
                throw new InvalidOperationException( R.KeyReleaseWhenUp );
            }
            if( doPress ) OnKeyPressed();
            _repeatCount = -1;
            // Last event: KeyUp
            KeyInteractionEventArgs e = new KeyInteractionEventArgs( this, Current.OnKeyUpCommands, KeyInteractionEventType.Up );
            EventHandler<KeyInteractionEventArgs> keyUp = KeyUp;
            if( keyUp != null ) keyUp( this, e );
            Keyboard.OnKeyUp( e );
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "Key" );
            r.Read();
            r.ReadStartElement( "KeyModes" );
            while( r.IsStartElement( "KeyMode" ) )
            {
                // We consider a missing attribute Mode (GetAttribute will return null) as Mode="": we'll use the Context EmptyMode.
                IKeyboardMode keyMode = Context.ObtainMode( r.GetAttribute( "Mode" ) );
                IKeyboardMode availableKeyMode = Keyboard.AvailableMode.Intersect( keyMode );

                if( keyMode == availableKeyMode )
                {
                    // If the mode is defined at the keyboard level,
                    // we create a new or update the existing actual key.
                    KeyMode k = FindOrCreate( keyMode );
                    sr.ReadInlineObjectStructured( k );
                }
                else
                {
                    // Key mode is not defined... This is not a
                    // standard case. Since defining the mode at the keyboard level just because a 
                    // key uses it is NOT an option, we choose to create the key with the actually available mode
                    // only if it does not already exist and we DO NOT change an existing key.
                    // This is a conservative approach that avoids blindly accepting biased data from the context.
                    KeyMode k = Find( availableKeyMode );
                    if( k == null )
                    {
                        k = FindOrCreate( keyMode );
                        sr.ReadInlineObjectStructured( k ); 
                    }
                    else r.Skip();
                }
            }
            r.Read();
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteStartElement( "KeyModes" );
            foreach( KeyMode vk in KeyModes )
            {
                sw.WriteInlineObjectStructuredElement( "KeyMode", vk );
            }
            w.WriteFullEndElement();
        }
    
    }
}
