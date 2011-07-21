#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\Zone.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Xml;
using CK.Core;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;
using CK.Plugin.Config;
using System.ComponentModel.Design;

namespace CK.Keyboard
{
    sealed partial class Zone : IZone, IStructuredSerializable
    {
        ZoneCollection  _zones;
        string          _name;

        internal Zone( ZoneCollection c, string name )
        {
            _zones = c;
            _name = name;
            _keys = new List<Key>();
        }

        IKeyboardContext IKeyboardElement.Context
        {
            get { return _zones.Context; }
        }

        internal KeyboardContext Context
         {
             get { return _zones.Context; }
         }

         IKeyboard IKeyboardElement.Keyboard
         {
             get { return _zones.Keyboard; }
         }

         internal Keyboard Keyboard
         {
             get { return _zones.Keyboard; }
         }

         ILayoutZone IZone.CurrentLayout
         {
             get { return CurrentLayout; }
         }

         public LayoutZone CurrentLayout
         {
             get { return _zones.Keyboard.CurrentLayout.FindOrCreate( this ); }
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
                     throw new InvalidOperationException( R.ZoneDefaultRenamed );
                 }
                 if( _zones != null )
                 {
                     _zones.RenameZone( this, ref _name, name );
                 }
                 else _name = name;
             }
             return _name;
         }

         public void Destroy()
         {
             if( _name.Length == 0 )
                 throw new InvalidOperationException( R.ZoneDefaultDestroyed );
             if( _zones != null )
             {
                 _zones.OnDestroy( this );
                 _zones = null;
             }
         }

        public bool IsDefault
        {
            get { return _name.Length == 0; }
        }


        internal void DestroyConfig()
		{
			foreach( Key k in _keys ) k.DestroyConfig();
            Context.ConfigContainer.Destroy( this );
		}

        public override string ToString()
        {
            return Name;
        }

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            Debug.Assert( r.Name == "Zone" );
            if( !r.IsEmptyElement )
            {
                r.Read();                

                if( r.IsStartElement( "Keys" ) )
                {
                    r.Read();
                    while( r.IsStartElement( "Key" ) )
                    {
                        Key k = Create( _keys.Count );
                        sr.ReadInlineObjectStructured( k );
                    }
                    r.Read();
                }
            }            
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Name", Name );            

            if( _keys.Count > 0 )
            {
                w.WriteStartElement( "Keys" );
                foreach( Key k in _keys )
                {
                    sw.WriteInlineObjectStructuredElement( "Key", k );                    
                }
                w.WriteFullEndElement();
            }
        }

    }
}
