#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\KeyProgram.cs) is part of CiviKey. 
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
using System.Xml;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;

namespace CK.Keyboard
{
    internal sealed class KeyProgram : IKeyProgram, IList<string>
    {
        IKeyboardContext _ctx;
        List<string> _commands;

        internal KeyProgram( IKeyboardContext ctx )
        {
            _ctx = ctx;
            _commands = new List<string>();
        }

        public IList<string> Commands
        {
            get { return this; }
        }

        IKeyboardContext IKeyProgram.Context
        {
            get { return _ctx; }
        }

        internal IKeyboardContext Context
        {
            get { return _ctx; }
        }

        public event EventHandler<KeyProgramCommandsEventArgs> CommandInserted;
        public event EventHandler<KeyProgramCommandsEventArgs> CommandUpdated;
        public event EventHandler<KeyProgramCommandsEventArgs> CommandDeleted;
        public event EventHandler<KeyProgramCommandsEventArgs> CommandsCleared;

        void OnCommandInserted(KeyProgramCommandsEventArgs e)
        {
            if( CommandInserted != null ) CommandInserted( this, e );
        }

        internal void Write( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteStartElement( "KeyProgram" );
            foreach( String cmd in _commands )
                w.WriteElementString( "Cmd", cmd );
            w.WriteEndElement();
        }

        #region IList<string> Members

        int IList<string>.IndexOf( string item )
        {
            return _commands.IndexOf( item );
        }

        void IList<string>.Insert( int index, string item )
        {
            _commands.Insert( index, item );
            OnCommandInserted( new KeyProgramCommandsEventArgs( this, KeyProgramCommandsEventType.Inserted, index ) );
        }

        void IList<string>.RemoveAt( int index )
        {
            _commands.RemoveAt( index );
            if( CommandDeleted != null ) CommandDeleted( this, new KeyProgramCommandsEventArgs( this, KeyProgramCommandsEventType.Deleted, index ) );
        }

        string IList<string>.this[int index]
        {
            get
            {
                return _commands[index];
            }
            set
            {
                _commands[index] = value;
                if( CommandUpdated != null ) CommandUpdated( this, new KeyProgramCommandsEventArgs( this, KeyProgramCommandsEventType.Updated, index ) );
            }
        }

        void ICollection<string>.Add( string item )
        {
            _commands.Add( item );
            OnCommandInserted( new KeyProgramCommandsEventArgs( this, KeyProgramCommandsEventType.Inserted, _commands.Count-1 ) );
        }

        void ICollection<string>.Clear()
        {
            _commands.Clear();
            if( CommandsCleared != null ) CommandsCleared( this, new KeyProgramCommandsEventArgs( this, KeyProgramCommandsEventType.Cleared, -1 ) );
        }

        bool ICollection<string>.Contains( string item )
        {
            return _commands.Contains( item );
        }

        void ICollection<string>.CopyTo( string[] array, int arrayIndex )
        {
            _commands.CopyTo( array, arrayIndex );
        }

        int ICollection<string>.Count
        {
            get { return _commands.Count; }
        }

        bool ICollection<string>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<string>.Remove( string item )
        {
            int index = _commands.IndexOf( item );
            if( index != -1 )
            {
                this.Commands.RemoveAt( index );
                return true;
            }
            return false;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        #endregion
    }
}
