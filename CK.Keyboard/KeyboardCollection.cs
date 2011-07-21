#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\KeyboardCollection.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Collections;
using CK.Core;
using System.Diagnostics;
using System.Xml;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;

namespace CK.Keyboard
{
    sealed class KeyboardCollection : IKeyboardCollection
    {
        KeyboardContext _context;
        IDictionary<string, Keyboard>   _keyboards;
        Keyboard                        _current;

        internal KeyboardCollection( KeyboardContext c )
        {
            _context = c;
            _keyboards = new Dictionary<string, Keyboard>();
        }

        IKeyboardContext IKeyboardCollection.Context
        {
            get { return _context; }
        }

        internal KeyboardContext Context
        {
            get { return _context; }
        }

        IKeyboard IKeyboardCollection.this[string name]
        {
            get { return this[ name ]; }
        }

        internal Keyboard this[string name]
        {
            get
            {              
                Keyboard k;
                _keyboards.TryGetValue( name, out k );
                return k;
            }
        }

        IKeyboard IKeyboardCollection.Create( string name )
        {
            return Create( name );
        }

        internal Keyboard Create( string name )
        {
            Keyboard kb = new Keyboard( this, KeyboardContext.EnsureUnique( name.Trim(), null, _keyboards.ContainsKey ) );
            return Add( kb );
        }

        private Keyboard Add( Keyboard kb )
        {
            Debug.Assert( kb.Keyboards == this, "The keyboard container should already have been set." );
            _keyboards.Add( kb.Name, kb );
            if( KeyboardCreated != null ) KeyboardCreated( this, new KeyboardEventArgs( kb ) );
            if( _current == null ) Current = kb;
            else Context.SetKeyboardContextDirty();
            return kb;
        }

        internal void OnStart()
        {
            if( _current != null )
            {
                Context.PluginRunner.Add( _current.RequirementLayer, true );
            }
        }

        internal void OnStop()
        {
            if( _current != null )
            {
                Context.PluginRunner.Remove( _current.RequirementLayer, false );
            }
        }

        internal void OnDestroy( Keyboard kb )
        {
            Debug.Assert( kb != null && kb.Context == _context && _context.Keyboards == this, 
                "This must be called only for keyboard of this context and only for actual keyboard collection." );
            // First, fires the keyboard changed event if needed:
            // current keyboard (the one that will be removed) is not yet modified: clients
            // will simply see the current keyboard be set to null.
            if( kb == _current ) Current = null;
            else Context.SetKeyboardContextDirty();
            // Removes keyboard from keyboard map.
            _keyboards.Remove( kb.Name );
            // When KeyboardRemoved event fires, the keyboard's Context is always reachable
            // and its configuration is still here.
            if( KeyboardDestroyed != null ) KeyboardDestroyed( this, new KeyboardEventArgs( kb ) );
            // Removes any configuration for this keyboard.
            kb.DestroyConfig();
        }


        public void Clear()
        {
            if( _keyboards.Count > 0 )
            {
                // To remove them, first captures the whole collection 
                // in an array to avoid iterating and removing in the 
                // same time.
                Keyboard[] kbs = new Keyboard[_keyboards.Count];
                _keyboards.Values.CopyTo( kbs, 0 );
                foreach( Keyboard k in kbs ) k.Destroy();
            }
            Debug.Assert( _current == null, "Current is necessarily null." );
        }

        public event EventHandler<CurrentKeyboardChangedEventArgs> CurrentChanged;

        public event EventHandler<KeyboardEventArgs>  KeyboardCreated;

        public event EventHandler<KeyboardEventArgs>  KeyboardDestroyed;

        public event EventHandler<KeyboardRenamedEventArgs>  KeyboardRenamed;

        public bool Contains( object item )
        {
            Keyboard k = item as Keyboard;
            return k != null ? _keyboards.Values.Contains( k ) : false; 
        }

        public int Count
        {
            get { return _keyboards.Count; }
        }

        public IEnumerator<IKeyboard> GetEnumerator()
        {
            return Wrapper<IKeyboard>.CreateEnumerator( _keyboards.Values );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyboards.Values.GetEnumerator();
        }

        IKeyboard IKeyboardCollection.Current
        {
            get { return _current; }
            set { Current = (Keyboard)value; }
        }

        internal Keyboard Current
        {
            get { return _current; }
            set
            {
                if( value != _current )
                {
                    Keyboard previous = _current;
                    if( value != null && value.Context != _context ) throw new ApplicationException( R.KeyboardErrorUnknown );

                    if( previous != null && Context.PluginRunner != null ) Context.PluginRunner.Remove( previous.RequirementLayer, false );
                    _current = value;
                    if( _current != null && Context.PluginRunner != null ) Context.PluginRunner.Add( _current.RequirementLayer, true );
                    
                    if( CurrentChanged != null )
                    {
                        CurrentChanged( this, new CurrentKeyboardChangedEventArgs( Context, previous ) );
                    }
                    Context.SetKeyboardContextDirty();
                }
            }
        }

        internal void RenameKeyboard( Keyboard k, ref string kbName, string newName )
        {
            Debug.Assert( k.Context == Context && k.Name == kbName && k.Name != newName );
            string previous = kbName;
            newName = KeyboardContext.EnsureUnique( newName, previous, _keyboards.ContainsKey );
            if( newName != previous )
            {
                _keyboards.Remove( previous );
                _keyboards.Add( newName, k );
                kbName = newName;
                if( KeyboardRenamed != null ) KeyboardRenamed( this, new KeyboardRenamedEventArgs( k, previous ) );
            }
        }      

        internal void ReplaceWith( KeyboardCollection c )
        {
            // First, adds the current one if it exists: it will be restored as the current one.
            // If no current keyboard exists, the first one added will become the current one.
            if( c.Current != null ) 
            {
                c.Current.Keyboards = this;
                Add( c.Current );
            }
            foreach( Keyboard k in c )
            {
                if( k != c.Current )
                {
                    k.Keyboards = this;
                    Add( k );
                }
            }
        }

        internal void ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Keyboard current = null;
            r.Read();
            while( r.IsStartElement( "Keyboard" ) )
            {
                string n = r.GetAttribute( "Name" );
                Keyboard kb = Create( n );
                bool isCurrent = r.GetAttribute( "IsCurrent" ) == "1";
                sr.ReadInlineObjectStructured( kb );
                if( isCurrent ) current = kb;
            }
            if( current != null ) Current = current;
        }

        internal void WriteInlineContent( IStructuredWriter sw )
        {
            foreach( Keyboard k in _keyboards.Values )
            {
                sw.WriteInlineObjectStructuredElement( "Keyboard", k );
            }
        }

    }
}
