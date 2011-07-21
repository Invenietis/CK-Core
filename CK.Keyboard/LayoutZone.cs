#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\LayoutZone.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Xml;
using CK.Core;
using System.Collections.Generic;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;
using CK.Plugin.Config;

namespace CK.Keyboard
{
    internal sealed class LayoutZone : ILayoutZone, ILayoutKeyCollection, IStructuredSerializable
    {
        Layout  _layout;
        Zone    _zone;

        public LayoutZone( Layout layout, Zone zone )
        {
            _layout = layout;
            _zone = zone;
        }

        internal KeyboardContext Context
        {
            get { return _zone.Context; }
        }

        IKeyboardContext ILayoutZone.Context
        {
            get { return _zone.Context; }
        }

        internal Keyboard Keyboard
        {
            get { return _zone.Keyboard; }
        }

        IKeyboard ILayoutZone.Keyboard
        {
            get { return _zone.Keyboard; }
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _zone.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _zone.Keyboard; }
        }

        internal Zone Zone
        {
            get { return _zone; }
        }

        IZone ILayoutZone.Zone
        {
            get { return _zone; }
        }

        internal Layout Layout
        {
            get { return _layout; }
        }

        ILayout ILayoutZone.Layout
        {
            get { return _layout; }
        }

        internal LayoutKey this[int i]
        {
            get { return _layout.FindOrCreate( _zone[i] ); }
        }

        internal void OnAvailableModeRemoved( IReadOnlyList<IKeyboardMode> modes )
        {
            foreach( LayoutKey kl in this )
            {
                kl.OnAvailableModeRemoved( modes );
            }
        }

        internal void OnCurrentModeChanged()
        {
            foreach( LayoutKey kl in this )
            {
                kl.OnCurrentModeChanged();
            }
        }

        #region Auto implementation of ILayoutKeyCollection

        public ILayoutKeyCollection LayoutKeys
        {
            get { return this; }
        }

        ILayoutZone ILayoutKeyCollection.LayoutZone
        {
            get { return this; }
        }

        int IReadOnlyList<ILayoutKey>.IndexOf( object item )
        {
            ILayoutKey l = item as ILayoutKey;
            return l != null ? _zone.IndexOf( l.Key ) : -1;
        }

        ILayoutKey IReadOnlyList<ILayoutKey>.this[int i]
        {
            get { return _layout.FindOrCreate( _zone[i] ); }
        }

        bool IReadOnlyCollection<ILayoutKey>.Contains( object item )
        {
            ILayoutKey l = item as ILayoutKey;
            return l != null ? l.LayoutZone == this : false;
        }

        int IReadOnlyCollection<ILayoutKey>.Count
        {
            get { return _zone.Count; }
        }

        IEnumerator<ILayoutKey> IEnumerable<ILayoutKey>.GetEnumerator()
        {
            return Wrapper<ILayoutKey>.CreateEnumerator<Key>( _zone.Keys, _layout.FindOrCreate );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Wrapper<object>.CreateEnumerator<Key>( _zone.Keys, _layout.FindOrCreate );
        }

        #endregion

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "Zone" );
            r.ReadToDescendant( "LayoutKeys" );
            if( !r.IsEmptyElement )
            {
                r.Read();
                while( r.IsStartElement( "LayoutKey" ) )
                {
                    int idx = r.GetAttributeInt( "Index", -1 );
                    if( idx >= 0 && idx < LayoutKeys.Count )
                    {
                        LayoutKey z = (LayoutKey)LayoutKeys[idx];
                        sr.ReadInlineObjectStructured( z );
                    }
                    else r.Skip();
                }

                r.ReadEndElement();
            }
            else r.Read();
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Name", _zone.Name );
            w.WriteStartElement( "LayoutKeys" );
            foreach( LayoutKey keyLayout in this )
            {
                sw.WriteInlineObjectStructuredElement( "LayoutKey", keyLayout );                
            }
            w.WriteEndElement();
        }
    }
}
