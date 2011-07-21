#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\LayoutKey.cs) is part of CiviKey. 
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
    sealed partial class LayoutKey : ILayoutKey, IKeyPropertyHolder, IStructuredSerializable
    {
        Layout  _layout;
        Key             _key;

        public LayoutKey( Layout layout, Key key )
        {
            _layout = layout;
            _key = key;
            Initialize( layout.Context );
        }

        internal KeyboardContext Context
        {
            get { return _key.Context; }
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _key.Context; }
        }

        internal Keyboard Keyboard
        {
            get { return _key.Keyboard; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _key.Keyboard; }
        }

        internal Layout Layout
        {
            get { return _layout; }
        }

        ILayout ILayoutKey.Layout
        {
            get { return _layout; }
        }

        internal Zone Zone
        {
            get { return _key.Zone; }
        }

        IZone IZoneElement.Zone
        {
            get { return _key.Zone; }
        }

        internal LayoutZone LayoutZone
        {
            get { return _layout.FindOrCreate( _key.Zone ); }
        }

        ILayoutZone ILayoutKey.LayoutZone 
        {
            get { return _layout.FindOrCreate( _key.Zone ); }
        }

        internal Key Key
        {
            get { return _key; }
        }

        IKey IKeyPropertyHolder.Key
        {
            get { return _key; }
        }

        ILayoutKeyModeCurrent ILayoutKey.Current
        {
            get { return Current; }
        }
        
        public bool IsCurrent
        {
            get { return Layout.IsCurrent; }
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "LayoutKey" );
            r.Read();
            while( r.IsStartElement( "LayoutKeyMode" ) )
            {
                IKeyboardMode keyMode = _key.Context.ObtainMode( r.GetAttribute( "Mode" ) );
                IKeyboardMode availableKeyMode = _key.Keyboard.AvailableMode.Intersect( keyMode );

                LayoutKeyMode k = FindOrCreate( availableKeyMode );
                sr.ReadInlineObjectStructured( k );
            }
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Index", XmlConvert.ToString( _key.Index ) );
            foreach( LayoutKeyMode vk in this )
            {
                sw.Xml.WriteStartElement( "LayoutKeyMode" );
                sw.WriteInlineObjectStructured( vk );
            }
        }
    }
}
