#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\ZoneCollection.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;

namespace CK.Keyboard
{
    class ZoneCollection : IZoneCollection, IEnumerable<Zone>, IStructuredSerializable
    {
        Keyboard			             _kb;
        Dictionary<string,Zone>  _zones;
		Zone		             _defaultZone;

        public event EventHandler<ZoneEventArgs> ZoneCreated;
        public event EventHandler<ZoneEventArgs> ZoneDestroyed;
        public event EventHandler<ZoneEventArgs> ZoneRenamed;

        internal ZoneCollection( Keyboard kb )
		{
			_kb = kb;
            _defaultZone = new Zone( this, String.Empty );
            _zones = new Dictionary<string, Zone>();
            _zones.Add( _defaultZone.Name, _defaultZone );
        }

        IKeyboardContext IZoneCollection.Context
        {
            get { return _kb.Context; }
        }

        internal KeyboardContext Context
        {
            get { return _kb.Context; }
        }

        IKeyboard IZoneCollection.Keyboard
        {
            get { return _kb; }
        }

        internal Keyboard Keyboard
        {
            get { return _kb; }
        }

        public bool Contains( object item )
        {
            IZone z = item as IZone;
            return z != null && z.Keyboard == Keyboard;
        }

        public int Count
        {
            get { return _zones == null ? 1 : _zones.Count; }
        }

        IEnumerator<IZone> IEnumerable<IZone>.GetEnumerator()
        {
            return Wrapper<IZone>.CreateEnumerator( _zones.Values );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _zones.Values.GetEnumerator();
        }

        public IEnumerator<Zone> GetEnumerator()
        {
            return _zones.Values.GetEnumerator();
        }

        IZone IZoneCollection.this[string name]
		{
			get { return this[ name ]; }
		}

        internal Zone this[ string name ]
		{
			get 
            {
                Zone l;
                _zones.TryGetValue( name, out l );
                return l;
            }
		}

        IZone IZoneCollection.Default
        {
            get { return _defaultZone; }
        }

        internal Zone Default
        {
            get { return _defaultZone; }
        }

        IZone IZoneCollection.Create( string name )
        {
            return Create( name );
        }

        internal Zone Create( string name )
        {
            name = name.Trim();
            Zone zone = new Zone( this, KeyboardContext.EnsureUnique( name, null, _zones.ContainsKey ) );
            _zones.Add( zone.Name, zone );

            if( ZoneCreated != null ) ZoneCreated( this, new ZoneEventArgs( zone ) );
            Context.SetKeyboardContextDirty();
            return zone;
        }

		internal void OnDestroy( Zone z )
		{
            Debug.Assert( z.Keyboard == Keyboard && z != Default, "It is not the default." );
            _zones.Remove( z.Name );
            foreach( Layout l in _kb.Layouts ) l.DestroyConfig( z, true );
            if( ZoneDestroyed != null ) ZoneDestroyed( this, new ZoneEventArgs( z ) );
            Context.SetKeyboardContextDirty();
        }

        internal void RenameZone( Zone z, ref string zoneName, string newName )
		{
            Debug.Assert( z.Keyboard == Keyboard && z.Name != newName, "It is not the default" );
            string previous = zoneName;
            newName = KeyboardContext.EnsureUnique( newName, previous, _zones.ContainsKey );
            if( newName != previous )
            {
                _zones.Remove( z.Name );
                _zones.Add( newName, z );
                zoneName = newName;
                if( ZoneRenamed != null ) ZoneRenamed( this, new ZoneRenamedEventArgs( z, previous ) );
                Context.SetKeyboardContextDirty();
            }
		}

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            // We are on the <Zones> tag, we move on to the content
            r.Read(); 
            
            while( r.IsStartElement( "Zone" ) )
            {
                // Gets normalized zone name.
                string n = r.GetAttribute( "Name" );
                if( n == null ) n = String.Empty;
                else n = n.Trim();

                Zone z;
                // If empty name, it is the default zone.
                if( n.Length == 0 ) z = _defaultZone;
                else z = Create( n );

                sr.ReadInlineObjectStructured( z );
            }
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;            
            foreach( Zone z in _zones.Values )
            {
                sw.WriteInlineObjectStructuredElement( "Zone", z );
            }
        }
    }
}