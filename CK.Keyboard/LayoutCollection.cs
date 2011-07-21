#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\LayoutCollection.cs) is part of CiviKey. 
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
    sealed class LayoutCollection : ILayoutCollection, IStructuredSerializable
    {
        Keyboard			        _kb;
        Dictionary<string,Layout>   _layouts;
		Layout		                _currentLayout;
		Layout		                _defaultLayout;

        public event EventHandler<KeyboardCurrentLayoutChangedEventArgs> CurrentChanged;
        public event EventHandler<LayoutEventArgs> LayoutCreated;
        public event EventHandler<LayoutEventArgs> LayoutDestroyed;
        public event EventHandler<LayoutEventArgs> LayoutRenamed;
        public event EventHandler<LayoutEventArgs> LayoutSizeChanged;

        internal LayoutCollection( Keyboard kb )
		{
			_kb = kb;
            _defaultLayout = _currentLayout = new Layout( this, String.Empty );
            Debug.Assert( _layouts == null, "Since in 95% of the case, there will be only 1 layout, we create the dictionary on demand." );
        }

        IKeyboardContext ILayoutCollection.Context
        {
            get { return _kb.Context; }
        }

        internal KeyboardContext Context
        {
            get { return _kb.Context; }
        }

        IKeyboard ILayoutCollection.Keyboard
        {
            get { return _kb; }
        }

        internal Keyboard Keyboard
        {
            get { return _kb; }
        }

        public bool Contains( object item )
        {
            ILayout l = item as ILayout;
            return l != null && l.Keyboard == Keyboard;
        }

        public int Count
        {
            get { return _layouts == null ? 1 : _layouts.Count; }
        }

        public IEnumerator<ILayout> GetEnumerator()
        {
            return _layouts == null ? new EnumMono<ILayout>( _defaultLayout ) : Wrapper<ILayout>.CreateEnumerator( _layouts.Values );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _layouts == null ? (IEnumerator)(new EnumMono<ILayout>( _defaultLayout )) : (IEnumerator)_layouts.Values.GetEnumerator();
        }
        
        ILayout ILayoutCollection.this[string name]
		{
			get { return this[ name ]; }
		}

        internal Layout this[ string name ]
		{
			get 
            {
                if( _currentLayout.Name == name ) return _currentLayout;
                if( _layouts == null ) return null;
                Layout l;
                _layouts.TryGetValue( name, out l );
                return l;
            }
		}

        ILayout ILayoutCollection.Create( string name )
        {
            return Create( name );
        }

        internal Layout Create( string name )
        {
            name = name.Trim();
            if( _layouts == null )
            {
                _layouts = new Dictionary<string, Layout>();
                _layouts.Add( _defaultLayout.Name, _defaultLayout );
            }
            Layout layout = new Layout( this, KeyboardContext.EnsureUnique( name, null, _layouts.ContainsKey ) );
            _layouts.Add( layout.Name, layout );

            if( LayoutCreated != null ) LayoutCreated( this, new LayoutEventArgs( layout ) );

            return layout;
        }

        public ILayout Default
        {
            get { return _defaultLayout; }
        }

        ILayout ILayoutCollection.Current
		{
			get { return _currentLayout; }
            set { Current = (Layout)value;  }
        }

		internal Layout Current
		{
			get { return _currentLayout; }
			set 
			{ 
				if( _currentLayout != value )
				{
					if( value == null ) throw new ArgumentNullException( "value" );
					if( _currentLayout.Keyboard != _kb ) throw new ApplicationException( R.InvalidSetLayout );

                    if( _kb == _kb.Keyboards.Current )
                    {
                        //using( IBrainRequirementsAccessor brainRequirement = Context.PluginManager.PluginLoader.Brain.GetRequirementsAccessor() )
                        //{
                        //    // Pop previous requirements
                        //    brainRequirement.Pop();
                        //    // Push new requirements
                        //    brainRequirement.Push( value.PluginRequirements, value.ServiceRequirements );
                        //}
                    }
                    
                    Layout previous = _currentLayout;
					_currentLayout = (Layout)value;
                    if( CurrentChanged != null ) CurrentChanged( this, new KeyboardCurrentLayoutChangedEventArgs( Keyboard, previous ) );
				}
			}
        }

        internal void OnDestroy( Zone z )
        {
            foreach( Layout l in _layouts.Values )
            {
                l.DestroyConfig( z, true );
            }
        }

        internal void OnDestroy( Key k )
        {
            foreach( Layout l in _layouts.Values )
            {
                l.DestroyConfig( k, true );
            }
        }

        internal void OnDestroy( KeyMode k )
        {
            foreach( Layout l in _layouts.Values )
            {
                l.DestroyConfig( k, true );
            }
        }

        internal void OnDestroyLayout( Layout l )
		{
            Debug.Assert( l.Keyboard == Keyboard && l != Default && _layouts != null, "It is not the default: we have more than one layout." );
			// First, handle the selected layout change if needed.
            if( l == _currentLayout ) Current = _defaultLayout;
			// Then triggers the event: the layout is still functionnal.
            if( LayoutDestroyed != null ) LayoutDestroyed( this, new LayoutEventArgs( l ) );
            _layouts.Remove( l.Name );
            // Removes any configuration for this layout.
            l.DestroyConfig();
            Context.SetKeyboardContextDirty();
        }

        internal void OnRenameLayout( Layout l, ref string layoutName, string newName )
		{
            Debug.Assert( l.Keyboard == Keyboard && l.Name != newName && _layouts != null, "It is not the default: we have more than one layout." );
            string previous = layoutName;
            newName = KeyboardContext.EnsureUnique( newName, previous, _layouts.ContainsKey );
            if( newName != previous )
            {
                _layouts.Remove( l.Name );
                _layouts.Add( newName, l );
                layoutName = newName;
                if( LayoutRenamed != null ) LayoutRenamed( this, new LayoutRenamedEventArgs( l, previous ) );
                Context.SetKeyboardContextDirty();
            }
		}

        internal void OnResizeLayout( Layout l )
        {
            Debug.Assert( l.Keyboard == Keyboard, "It is one of our layouts." );
            if( LayoutSizeChanged != null )
            {
                LayoutSizeChanged( this, new LayoutEventArgs( l ) );
            }
        }	

        internal void OnAvailableModeRemoved( IReadOnlyList<IKeyboardMode> modes )
        {
            if( _layouts == null ) _defaultLayout.OnAvailableModeRemoved( modes );
            else
            {
                foreach( Layout l in _layouts.Values )
                {
                    l.OnAvailableModeRemoved( modes );
                }
            }
        }

        internal void OnCurrentModeChanged()
        {
            if( _layouts == null ) _defaultLayout.OnCurrentModeChanged();
            else
            {
                foreach( Layout l in _layouts.Values )
                {
                    l.OnCurrentModeChanged();
                }
            }
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            bool isCurrent = false;

            //We are on the <Layouts> tag, we mode on to the content
            r.Read();

            while( r.IsStartElement( "Layout" ) )
            {
                Layout l = null;
                string n = r.GetAttribute( "Name" );
                if( n == null ) n = String.Empty;

                l = this[n];
                if( l == null ) l = Create( n );

                isCurrent = r.GetAttributeBoolean( "IsCurrent", false );
                sr.ReadInlineObjectStructured( l );
                if( isCurrent ) _currentLayout = l;
            }
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            if( _layouts != null )
            {
                foreach( Layout l in _layouts.Values )
                {
                    sw.WriteInlineObjectStructuredElement( "Layout", l );
                }
            }
            else
            {
                sw.WriteInlineObjectStructuredElement( "Layout", _defaultLayout );                
            }
        }
    }
}