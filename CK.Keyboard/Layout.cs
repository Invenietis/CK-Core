#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Layout.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using CK.Core;
using CK.Plugin;
using CK.Keyboard.Model;
using CK.Context;
using CK.Plugin.Config;
using CK.Storage;
using System.Globalization;

namespace CK.Keyboard
{
    sealed class Layout : ILayout, ILayoutZoneCollection, IStructuredSerializable
    {
        private const int DefaultWidth = 1200;
        private const int DefaultHeight = 400;

        RequirementLayer _reqLayer;
        LayoutCollection    _layouts;
        Hashtable           _assoc;
        string              _name;
        int                 _height;
        int                 _width;
       
        internal Layout( LayoutCollection c, string name )
        {
            _layouts = c;
            _name = name;
            _height = DefaultHeight;
            _width = DefaultWidth;
            _assoc = new Hashtable();
            _reqLayer = new RequirementLayer( "Layout" );
        }

        IKeyboardContext ILayout.Context
        {
            get { return _layouts.Context; }
        }

        IKeyboard ILayout.Keyboard
        {
            get { return _layouts.Keyboard; }
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _layouts.Context; }
        }

        IKeyboard IKeyboardElement.Keyboard
        {
            get { return _layouts.Keyboard; }
        }

        internal KeyboardContext Context
        {
            get { return _layouts.Context; }
        }

        internal Keyboard Keyboard
        {
            get { return _layouts.Keyboard; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Rename( string name )
        {
            if( name == null ) throw new ArgumentNullException();
            name = name.Trim();
            if( _name != name )
            {
				if( _name.Length == 0 )
				{
                    throw new CKException( R.LayoutDefaultRenamed );
				}
				if( _layouts != null )
				{
					_layouts.OnRenameLayout( this, ref _name, name );
				}
				else _name = name;
            }
            return _name;
        }

        public bool IsDefault
        {
            get { return _name.Length == 0; }
        }

        public bool IsCurrent
        {
            get { return Keyboard.CurrentLayout == this; }
        }

        public void Destroy()
        {
            if( _name.Length == 0 )
                throw new CKException( R.LayoutDefaultDestroyed );
            if( _layouts != null )
            {
                _layouts.OnDestroyLayout( this );
                _layouts = null;
            }
        }

        public int W
        {
            get { return _width; }
            set 
            {
                if( _width != value )
                {
                    _width = value; 
                    _layouts.OnResizeLayout( this );
                }
            }
        }
        
        public int H
        {
            get { return _height; }
            set 
            { 
                if( _height != value )
                {
                    _height = value;
                    _layouts.OnResizeLayout( this );
                }
            }
        }

        public RequirementLayer RequirementLayer
        {
            get { return _reqLayer; }
        }

        internal LayoutZone Find( IZone zone )
        {
            return (LayoutZone)_assoc[zone];
        }

        internal LayoutKey Find( Key k )
        {
            return (LayoutKey)_assoc[k];
        }

        internal LayoutKeyMode Find( KeyMode k )
        {
            return (LayoutKeyMode)_assoc[k];
        }

        internal LayoutZone FindOrCreate( Zone zone )
        {
            LayoutZone l = (LayoutZone)_assoc[zone];
            if( l != null ) return l;

            if( zone.Keyboard != Keyboard ) throw new ArgumentException( R.ZoneFromAnotherKeyboard );

            l = new LayoutZone( this, zone );
            _assoc.Add( zone, l );
            return l;
        }

        internal LayoutKey FindOrCreate( Key key )
        {
            LayoutKey l = (LayoutKey)_assoc[key];
            if( l != null ) return l;

            if( key.Keyboard != Keyboard ) throw new ArgumentException( R.KeyFromAnotherKeyboard );

            l = new LayoutKey( this, key );
            _assoc.Add( key, l );
            return l;
        }

        internal void OnAvailableModeRemoved( IReadOnlyList<IKeyboardMode> modes )
        {
            foreach( LayoutZone zl in LayoutZones )
            {
                zl.OnAvailableModeRemoved( modes );
            }
        }

        internal void OnCurrentModeChanged()
        {
            foreach( LayoutZone zl in LayoutZones )
            {
                zl.OnCurrentModeChanged();
            }
        }

        /// <summary>
        /// Destroy the configuration from the shared dictionary for this layout 
        /// and all the subordinated objects (zone, key and actual key layouts).
        /// </summary>
        internal void DestroyConfig()
        {
            foreach( Zone z in _layouts.Keyboard.Zones )
            {
                DestroyConfigObject( z, false );
            }
            Context.ConfigContainer.Destroy( this );
        }

        internal void DestroyConfig( Zone z, bool removeLayout )
        {
            foreach( Key k in z.Keys )
            {
                DestroyConfigObject( k, removeLayout );
            }
            DestroyConfigObject( z, removeLayout );
        }

        internal void DestroyConfig( Key k, bool removeLayout )
        {
            foreach( KeyMode a in k.KeyModes )
            {
                DestroyConfigObject( a, removeLayout );
            }
            DestroyConfigObject( k, removeLayout );
        }

        internal void DestroyConfig( KeyMode a, bool removeLayout )
        {
            DestroyConfigObject( a, removeLayout );
        }

        void DestroyConfigObject( object keyboardObject, bool removeLayout )
        {
            object l = _assoc[keyboardObject];
            if( l != null )
            {
                Context.ConfigContainer.Destroy( l );
                if( removeLayout ) _assoc.Remove( keyboardObject );
            }
        }

        

        public ILayoutZoneCollection LayoutZones
        {
            get { return this; }
        }

        #region Auto implementation of the ILayoutZoneCollection

        ILayout ILayoutZoneCollection.Layout
        {
            get { return this; }
        }

        ILayoutZone ILayoutZoneCollection.this[string zoneName]
        {
            get 
            {
                Zone z = Keyboard.Zones[zoneName];
                return z == null ? null : FindOrCreate( z );
            }
        }

        ILayoutZone ILayoutZoneCollection.Default
        {
            get { return FindOrCreate( Keyboard.Zones.Default ); }
        }

        bool IReadOnlyCollection<ILayoutZone>.Contains( object item )
        {
            ILayoutZone l = item as ILayoutZone;
            return l != null ? l.Layout == this : false;
        }

        int IReadOnlyCollection<ILayoutZone>.Count
        {
            get { return Keyboard.Zones.Count; }
        }

        IEnumerator<ILayoutZone> IEnumerable<ILayoutZone>.GetEnumerator()
        {
            return Wrapper<ILayoutZone>.CreateEnumerator<Zone>( Keyboard.Zones, FindOrCreate );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Wrapper<object>.CreateEnumerator<Zone>( Keyboard.Zones, FindOrCreate );
        }

        #endregion

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "Layout" );

            if( r.GetAttribute( "Width" ) != null )
                _width = int.Parse( r.GetAttribute( "Width" ) );

            if( r.GetAttribute( "Height" ) != null )
                _height = int.Parse( r.GetAttribute( "Height" ) );            

            r.Read();

            sr.ReadInlineObjectStructuredElement( "RequirementLayer", _reqLayer );

            r.ReadStartElement( "Zones" );
            while( r.IsStartElement( "Zone" ) )
            {
                Zone z = _layouts.Keyboard.Zones[r.GetAttribute( "Name" )];
                if( z != null )
                {
                    LayoutZone lz = FindOrCreate( z );
                    sr.ReadInlineObjectStructured( lz );
                }
                else r.Skip();
            }
            r.Read(); 
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "IsCurrent", XmlConvert.ToString( _layouts.Current == this ) );
            w.WriteAttributeString( "Name", _name );
            w.WriteAttributeString( "Width", _width.ToString( CultureInfo.InvariantCulture ) );
            w.WriteAttributeString( "Height", _height.ToString( CultureInfo.InvariantCulture ) );

            sw.WriteInlineObjectStructuredElement( "RequirementLayer", _reqLayer );            

            w.WriteStartElement( "Zones" );
            foreach( LayoutZone z in this )
            {
                sw.WriteInlineObjectStructuredElement( "Zone", z );                
            }
            w.WriteFullEndElement();
        }
    }
}
