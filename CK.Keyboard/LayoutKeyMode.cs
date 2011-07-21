#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\LayoutKeyMode.cs) is part of CiviKey. 
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

namespace CK.Keyboard
{
    sealed class LayoutKeyMode : ILayoutKeyMode, IModeDependantObjectImpl<LayoutKeyMode>, ILayoutKeyModeCurrent, IStructuredSerializable
    {
        LayoutKey _keyLayout;
        IKeyboardMode _mode;
        int _x;
        int _y;
        int _width;
        int _height;
        bool _visible;

        public LayoutKeyMode( LayoutKey layout, IKeyboardMode mode )
        {
            _keyLayout = layout;
            _mode = mode;
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _keyLayout.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _keyLayout.Keyboard; }
        }

        IZone IZoneElement.Zone
        {
            get { return _keyLayout.Zone; }
        }

        IKey IKeyPropertyHolder.Key
        {
            get { return _keyLayout.Key; }
        }

        ILayoutKey ILayoutKeyMode.LayoutKey
        {
            get { return LayoutKey; }
        }

        ILayout ILayoutKeyMode.Layout
        {
            get { return _keyLayout.Layout; }
        }

        internal IKeyboardContext Context
        {
            get { return _keyLayout.Context; }
        }

        internal Keyboard Keyboard
        {
            get { return _keyLayout.Keyboard; }
        }

        internal Zone Zone
        {
            get { return _keyLayout.Zone; }
        }

        internal Key Key
        {
            get { return _keyLayout.Key; }
        }

        internal Layout Layout
        {
            get { return _keyLayout.Layout; }
        }

        internal LayoutKey LayoutKey
        {
            get { return _keyLayout; }
        }

        public IKeyboardMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// The current actual key is a fallback if its mode is not exactly the 
        /// same as the current keyboard mode. 
        /// </summary>
        bool ILayoutKeyModeCurrent.IsFallBack
        {
            get { return _mode != Keyboard.CurrentMode; }
        }

        bool IKeyPropertyHolder.IsCurrent
        {
            get { return Layout.IsCurrent && _keyLayout.Current == this; }
        }

        public void Destroy()
        {
            if( _mode.IsEmpty )
            {
                throw new InvalidOperationException( R.DestroyDefaultLayoutKeyMode );
            }
            if( _keyLayout != null )
            {
                _keyLayout.Destroy( this );
                _keyLayout = null;
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if( _visible != value )
                {
                    _visible = value;
                    OnPropertyChanged( "Visible" );
                }
            }
        }

        public int X
        {
            get { return _x; }
            set 
            {
                if( _x != value )
                {
                    _x = value;
                    OnPropertyChanged( "X" );
                }
            }
        }

        public int Y
        {
            get { return _y; }
            set 
            {
                if( _y != value )
                {
                    _y = value;
                    OnPropertyChanged( "Y" );
                }
            }
        }

        public int Width
        {
            get { return _width; }
            set 
            {
                if( _width != value )
                {
                    _width = value;
                    OnPropertyChanged( "W" );
                }
            }
        }

        public int Height
        {
            get { return _height; }
            set 
            {
                if( _height != value )
                {
                    _height = value;
                    OnPropertyChanged( "H" );
                }
            }
        }

        public bool ChangeMode( IKeyboardMode mode )
        {
            if( mode == _mode ) return true;
            if( _mode.IsEmpty ) return false;
            return  _keyLayout.ChangeObjectMode( this, mode );
        }

        public void SwapModes( ILayoutKeyMode other )
        {
            if( other.Context == Context )//This ILayoutKeyMode is from the same context as this LayoutKeyMode, so its implementation must be an instance of this class
                _keyLayout.SwapModes( this, (LayoutKeyMode)other );
        }

        private void OnPropertyChanged( string propertyName )
        {
            _keyLayout.OnActualLayoutPropertyChanged( new LayoutKeyModePropertyChangedEventArgs( this, propertyName ) );
        }


        #region IModeDependantObjectImpl<LayoutKeyMode> Members

        LayoutKeyMode _prev;

        IKeyboardMode IModeDependantObjectImpl<LayoutKeyMode>.Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        LayoutKeyMode IModeDependantObjectImpl<LayoutKeyMode>.Prev
        {
            get { return _prev; }
            set { _prev = value; }
        }

        #endregion

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "LayoutKeyMode" );

            X = int.Parse( r.GetAttribute( "X" ) );
            Y = int.Parse( r.GetAttribute( "Y" ) );
            Width = int.Parse( r.GetAttribute( "W" ) );
            Height = int.Parse( r.GetAttribute( "H" ) );

            string visible = r.GetAttribute( "Visible" );
            if( visible != null ) _visible = XmlConvert.ToBoolean( visible );
            r.Read();
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Mode", _mode.ToString() );
            w.WriteAttributeString( "X", XmlConvert.ToString( X ) );
            w.WriteAttributeString( "Y", XmlConvert.ToString( Y ) );
            w.WriteAttributeString( "W", XmlConvert.ToString( Width ) );
            w.WriteAttributeString( "H", XmlConvert.ToString( Height ) );
            w.WriteAttributeString( "Visible", XmlConvert.ToString( _visible ) );
        }

    }
}
