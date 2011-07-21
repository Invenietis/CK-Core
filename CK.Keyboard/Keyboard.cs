#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Keyboard.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Keyboard.Model;
using CK.Context;
using CK.Plugin.Config;
using CK.Storage;
using CK.Core;

namespace CK.Keyboard
{
    sealed partial class Keyboard : IKeyboard, IStructuredSerializable
    {
        KeyboardCollection  _keyboards;
        LayoutCollection    _layouts;
        ZoneCollection      _zones;
        string              _name;
        RequirementLayer    _reqLayer;
        
        internal Keyboard( KeyboardCollection holder, string name )
        {
            Debug.Assert( holder != null );
            _keyboards = holder;
            _zones = new ZoneCollection( this );            
            _layouts = new LayoutCollection( this );
            _name = name;
            _availableMode = _keyboards.Context.EmptyMode;
            _currentMode = _keyboards.Context.EmptyMode;

            _reqLayer = new RequirementLayer( "Keyboard" );
        }

        internal KeyboardCollection Keyboards
        {
            get { return _keyboards; }
            set
            {
                Debug.Assert( value.Context == _keyboards.Context, "We stay in the same context (used in Replace during whole Context read)." );
                _keyboards = value;
            }
        }

        IKeyboardContext IKeyboard.Context
        {
            get { return _keyboards.Context; }
        }

        internal KeyboardContext Context
        {
            get { return _keyboards.Context; }
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _keyboards.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return this; }
        }

        ILayoutCollection IKeyboard.Layouts
        {
            get { return _layouts; }
        }

        internal LayoutCollection Layouts
        {
            get { return _layouts; }
        }

        IZoneCollection IKeyboard.Zones
        {
            get { return _zones; }
        }

        internal ZoneCollection Zones
        {
            get { return _zones; }
        }

        public RequirementLayer RequirementLayer
        {
            get { return _reqLayer; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Rename( string name )
        {
            if( name == null ) throw new ArgumentNullException( "name" );
            name = name.Trim();
            if( _name != name )
            {
                if( _keyboards != null )
                    _keyboards.RenameKeyboard( this, ref _name, name );
                else _name = name;
            }
            return _name;
        }

        public void Destroy()
        {
            if( _keyboards != null )
            {
                _keyboards.OnDestroy( this );
                _keyboards = null;
            }
        }

        ILayout IKeyboard.CurrentLayout
        {
            get { return Layouts.Current; }
            set { Layouts.Current = (Layout)value; }
        }

        internal Layout CurrentLayout
        {
            get { return Layouts.Current; }
            set { Layouts.Current = value; }
        }

        public event EventHandler<KeyboardCurrentLayoutChangedEventArgs> CurrentLayoutChanged
        {
            add { Layouts.CurrentChanged += value; }
            remove { Layouts.CurrentChanged -= value; }
        }

        internal void DestroyConfig()
        {
            foreach( Layout l in Layouts ) l.DestroyConfig();
            foreach( Zone z in Zones ) z.DestroyConfig();
            Context.ConfigContainer.Destroy( this );
        }		

        public override string ToString()
        {
            return Name;
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "Keyboard" );

            _name = r.GetAttribute( "Name" );
            r.ReadStartElement( "Keyboard" );

            sr.ReadInlineObjectStructuredElement( "RequirementLayer", _reqLayer );

            if( r.IsStartElement( "Modes" ) )
            {
                string currentMode = r.GetAttribute( "Current" );
                if( r.IsEmptyElement )  r.Read();
                else
                {
                    r.Read();

                    IKeyboardMode futureAvailableMode = Context.EmptyMode;
                    while( r.IsStartElement( "Mode" ) )
                    {
                        futureAvailableMode = futureAvailableMode.Add( Context.ObtainMode( r.ReadElementContentAsString() ) );
                    }
                    AvailableMode = futureAvailableMode;
                    CurrentMode = Context.ObtainMode( currentMode );
                    r.ReadEndElement(); // Modes;
                }
            }
            sr.ReadInlineObjectStructuredElement( "Zones", Zones );
            sr.ReadInlineObjectStructuredElement( "Layouts", Layouts );
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Name", Name );

            if( _keyboards.Current == this ) w.WriteAttributeString( "IsCurrent", "1" );

            sw.WriteInlineObjectStructuredElement( "RequirementLayer", _reqLayer );

            w.WriteStartElement( "Modes" );
            w.WriteAttributeString( "Current", CurrentMode.ToString() );

            foreach( IKeyboardMode m in AvailableMode.AtomicModes )
                w.WriteElementString( "Mode", m.ToString() );

            w.WriteEndElement();

            sw.WriteInlineObjectStructuredElement( "Zones", Zones );
            sw.WriteInlineObjectStructuredElement( "Layouts", Layouts );
        }
    }
}
