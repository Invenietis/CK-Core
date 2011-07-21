#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\KeyMode.cs) is part of CiviKey. 
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
using CK.Storage;
using CK.Keyboard.Model;
using CK.Context;
using CK.Plugin.Config;
using CK.Core;

namespace CK.Keyboard
{
    /// <summary>
    /// Implements both IKeyMode and IKeyModeCurrent for readability.
    /// </summary>
    sealed class KeyMode : IKeyMode, IKeyModeCurrent, IModeDependantObjectImpl<KeyMode>, IStructuredSerializable
    {
        Key             _key;
        IKeyboardMode   _mode;
        string          _desc;
        string          _upLabel;
        string          _downLabel;
        KeyProgram      _onKeyDown;
        KeyProgram      _onKeyUp;
        KeyProgram      _onKeyPressed;
        bool            _enabled;

        internal KeyMode( Key key, IKeyboardMode mode )
        {
            Debug.Assert( key.Context == mode.Context );
            _key = key;
            _mode = mode;
            _onKeyDown = new KeyProgram( key.Context );
            _onKeyPressed = new KeyProgram( key.Context );
            _onKeyUp = new KeyProgram( key.Context );
            _upLabel = _downLabel = String.Empty;
            _desc = String.Empty;
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _key.Context; }
        }

        internal KeyboardContext Context
        {
            get { return _key.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _key.Keyboard; }
        }

        internal Keyboard Keyboard
        {
            get { return _key.Keyboard; }
        }

        IZone IZoneElement.Zone
        {
            get { return _key.Zone; }
        }

        internal Zone Zone
        {
            get { return _key.Zone; }
        }

        IKey IKeyPropertyHolder.Key
        {
            get { return _key; }
        }

        public bool IsCurrent
        {
            get { return _key.Current == this; }
        }

        internal Key Key
        {
            get { return _key; }
        }

        public IKeyboardMode Mode 
        {
            get { return _mode; }
        }

        public bool ChangeMode( IKeyboardMode mode ) 
        {
            if( mode == _mode ) return true;
            if( _mode.IsEmpty ) return false;
            return Key.ChangeObjectMode( this, mode );
        }

        public void SwapModes( IKeyMode other )
        {
            if(other.Context == Context)//This IKeyMode is from the same context as this KeyMode, so its implementation must be an instance of this class
            _key.SwapModes( this, (KeyMode)other );
        }

        public void Destroy()
        {
            if( _mode.IsEmpty )
            {
                throw new InvalidOperationException( R.DestroyDefaultKeyMode );
            }
            if( _key != null )
            {
                _key.Destroy( this );
                _key = null;
            }
        }

        /// <summary>
        /// The current actual key is a fallback if its mode is not exactly the 
        /// same as the current keyboard mode. 
        /// </summary>
        bool IKeyModeCurrent.IsFallBack
        {
            get { return _mode != Keyboard.CurrentMode; }
        }

        internal void DestroyConfig()
        {
            Context.ConfigContainer.Destroy( this );
        }

        public bool Enabled
        {
            get { return _enabled; }
            set 
            {
                if( _enabled != value )
                {
                    _enabled = value;
                    OnPropertyChanged( "Enabled" );
                }
            }
        }

        public string Description
        {
            get { return _desc; }
            set 
            {
                if( value == null ) value = String.Empty;
                if( _desc != value )
                {
                    _desc = value;
                    OnPropertyChanged( "Description" );
                }
            }
        }

        public string UpLabel
        {
            get { return _upLabel.Length == 0 ? _downLabel : _upLabel; }
            set
            {
                if( value == null ) value = String.Empty;
                if( _upLabel != value )
                {
                    _upLabel = value;
                    OnPropertyChanged( "UpLabel" );
                }
            }
        }

        public string DownLabel
        {
            get { return _downLabel.Length == 0 ? _upLabel : _downLabel; }
            set
            {
                if( value == null ) value = String.Empty;
                if( _downLabel != value )
                {
                    _downLabel = value;
                    OnPropertyChanged( "DownLabel" );
                }
            }
        }

        public IKeyProgram OnKeyDownCommands
        {
            get { return _onKeyDown; }
        }

        public IKeyProgram OnKeyUpCommands
        {
            get { return _onKeyUp; }
        }

        public IKeyProgram OnKeyPressedCommands
        {
            get { return _onKeyPressed; }
        }

        private void OnPropertyChanged( string propertyName )
        {
            _key.OnActualPropertyChanged( new KeyModePropertyChangedEventArgs( this, propertyName ) );
        }

        #region Xml

        private static void WriteKeyState( IStructuredWriter sw, string name, string label, KeyProgram keyprogram )
        {
            if( label.Length > 0 || keyprogram.Commands.Count > 0 )
            {
                XmlWriter w = sw.Xml;
                w.WriteStartElement( name );
                if( label.Length > 0 )
                    w.WriteAttributeString( "Label", label );
                if( keyprogram.Commands.Count > 0 )
                {
                    keyprogram.Write( sw );
                }
                w.WriteEndElement();
            }
        }

        private void ReadKeyState( IStructuredReader sr, string name, ref string label, ref KeyProgram keyprogram )
        {
            XmlReader r = sr.Xml;
            if( r.Name == name )
            {
                string l = r.GetAttribute( "Label" );
                if( l != null ) label = l;
                if( !r.IsEmptyElement )
                {
                    if( r.ReadToDescendant( "KeyProgram" ) )
                    {
                        r.Read();
                        while( r.IsStartElement( "Cmd" ) )
                        {
                            r.Read();
                            keyprogram.Commands.Add( r.ReadContentAsString() );
                            r.ReadEndElement(); // Cmd
                        }
                        r.ReadEndElement(); // KeyProgram
                    }                    

                }
                r.Read();
            }
        }

        #endregion

        #region IModeDependantObjectImpl<KeyMode> Members

        KeyMode _prev;

        IKeyboardMode IModeDependantObjectImpl<KeyMode>.Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        KeyMode IModeDependantObjectImpl<KeyMode>.Prev
        {
            get { return _prev; }
            set { _prev = value; }
        }

        #endregion

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "KeyMode" );

            _enabled = r.GetAttributeBoolean( "Enabled", _enabled );
            _desc = r.GetAttribute( "Description" ) ?? _desc;
            r.Read();

            ReadKeyState( sr, "KeyUp", ref _upLabel, ref _onKeyUp );
            ReadKeyState( sr, "KeyDown", ref _downLabel, ref _onKeyDown );
            string sArtifact = String.Empty;
            ReadKeyState( sr, "KeyPressed", ref sArtifact, ref _onKeyPressed );
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Mode", Mode.ToString() );
            w.WriteAttributeString( "Enabled", XmlConvert.ToString( _enabled ) );
            w.WriteAttributeString( "Description", _desc );            

            WriteKeyState( sw, "KeyUp", _upLabel, _onKeyUp );
            WriteKeyState( sw, "KeyDown", _downLabel, _onKeyDown );
            WriteKeyState( sw, "KeyPressed", String.Empty, _onKeyPressed );
        }

    }
}
