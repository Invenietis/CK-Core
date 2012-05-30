#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\Collections\UriHistoryCollection.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using CK.Core;
using CK.Storage;

namespace CK.Plugin.Config
{

    internal class UriHistoryCollection : List<UriHistory>, IUriHistoryCollection, IStructuredSerializable
    {
        Dictionary<Uri,UriHistory> _byAddress;
        ConfigurationBase _holder;

        internal readonly string EntryName;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler  PropertyChanged;

        internal UriHistoryCollection( ConfigurationBase holder, string entryName )
        {
            _previous = null;
            _holder = holder;
            _byAddress = new Dictionary<Uri, UriHistory>();
            EntryName = entryName;
        }

        public IUriHistory Current
        {
            get { return Count > 0 ? this[0] : null; }
            set 
            {
                if( value == null ) throw new ArgumentNullException();
                UriHistory u = value as UriHistory;
                if( u == null || u.Holder != this ) throw new ArgumentException( R.UriHistoryNotInCollection );
                u.Index = 0;
            }
        }

        IUriHistory _previous;
        public IUriHistory Previous
        {
            get { return _previous; }
        }

        public IUriHistory Find( Uri address )
        {
            return _byAddress.GetValueWithDefault( address, null );
        }

        public IUriHistory FindOrCreate( Uri address )
        {
            UriHistory found;
            if( !_byAddress.TryGetValue( address, out found ) )
            {
                found = new UriHistory( this, address, Count );
                Add( found );
                _byAddress.Add( address, found );
                var h = CollectionChanged;
                if( h != null ) h( this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, found ) );
                if( Count == 1 ) FireCurrentChangedEvent();
            }
            return found;
        }

        public void Remove( IUriHistory profile )
        {
            UriHistory p = profile as UriHistory;
            if( profile != null && p.Holder == this )
            {
                bool isCurrent = p.Index == 0;
                _byAddress.Remove( p.Address );
                RemoveAt( p.Index );
                var h = CollectionChanged;
                if( h != null ) h( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, p ) );
                if( isCurrent ) FireCurrentChangedEvent();
            }
        }

        #region IReadOnlyList<IUriHistory>

        public new IEnumerator<IUriHistory> GetEnumerator()
        {
            IEnumerable<UriHistory> b = this;
            return b.GetEnumerator();
        }

        public new IUriHistory this[int i]
        {
            get { return base[i]; }
        }

        public bool Contains( object item )
        {
            UriHistory u = item as UriHistory;
            return u != null ? base.Contains( u ) : false;
        }

        public int IndexOf( object item )
        {
            UriHistory u = item as UriHistory;
            return u != null ? base.IndexOf( u ) : -1;
        }
        #endregion

        internal void FireLoadedChangedEvents()
        {
            FireResetEvent();
            FireCurrentChangedEvent();
        }

        void FireCurrentChangedEvent()
        {
            var hp = PropertyChanged;
            if( hp != null ) hp( this, new PropertyChangedEventArgs( "LastActive" ) );
            if( _holder != null ) _holder.FireCurrentHistoChangedEvent( EntryName );
        }

        void FireResetEvent()
        {
            var hc = CollectionChanged;
            if( hc != null ) hc( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

        internal void OnSetAddress( Uri previous, UriHistory u )
        {
            UriHistory uExist;
            _byAddress.Remove( previous );
            if( _byAddress.TryGetValue( u.Address, out uExist ) )
            {
                Debug.Assert( uExist != u );
                _byAddress[u.Address] = u;
                var hc = CollectionChanged;
                if( hc != null ) hc( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, uExist ) );
            }
            else
            {
                _byAddress.Add( u.Address, u );
            }
        }

        internal void OnSetIndex( UriHistory u, int newIndex )
        {
            var current = Current;
            RemoveAt( u.Index );
            Insert( newIndex, u );
            UpdateIndices( this );
            _previous = this.Count > 1 ? this[1] : null;
            if( current != Current ) FireCurrentChangedEvent();
        }

        static void UpdateIndices( IEnumerable<UriHistory> list )
        {
            int i = 0;
            foreach( var u in list ) u.SetIndex( i++ );
        }

        void IStructuredSerializable.ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            r.Read();
            var newDic = new Dictionary<Uri, UriHistory>();
            var newList = new List<UriHistory>();
            UriHistory last = null;
            while( r.IsStartElement( EntryName ) )
            {
                string uri = r.GetAttribute( "Uri" );
                if( uri == null )
                {
                    string address = r.GetAttribute( "Address" );
                    if( address != null ) uri = "file:///" + address.Replace( '\\', '/' );
                }
                if( uri != null )
                {
                    UriHistory p = newDic.GetOrSet( new Uri( uri ), u => 
                    { 
                        var newU = new UriHistory( this, u, newList.Count ); 
                        newList.Add( newU ); 
                        return newU; 
                    } );
                    p.DisplayName = r.GetAttribute( "DisplayName" ) ?? r.GetAttribute( "Name" );
                    
                    if( last == null || r.GetAttributeBoolean( "IsLast", false ) ) last = p;
                }
                r.Skip();
            }
            if( last != null && last != newList[0] )
            {
                newList.RemoveAt( last.Index );
                newList.Insert( 0, last );
                UpdateIndices( newList );
            }
            _byAddress.Clear();
            _byAddress.AddRange( newDic );
            Clear();
            AddRange( newList );
        }

        void IStructuredSerializable.WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            foreach( UriHistory profile in this )
            {
                w.WriteStartElement( EntryName );
                w.WriteAttributeString( "DisplayName", profile.DisplayName );
                w.WriteAttributeString( "Uri", profile.Address.ToString() );
                w.WriteFullEndElement();
            }
        }


    }
}
