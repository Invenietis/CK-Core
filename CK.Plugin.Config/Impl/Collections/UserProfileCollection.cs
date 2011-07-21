#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\ConfigManager.cs) is part of CiviKey. 
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
using CK.Storage;
namespace CK.Plugin.Config
{
    internal class UserProfileCollection : IUserProfileCollection, IStructuredSerializable
    {
        Dictionary<string,UserProfile> _internalDic;
        IReadOnlyCollection<IUserProfile> _profiles;
        ConfigurationBase _holder;
        IUserProfile _last;
        
        public event EventHandler<UserProfileCollectionChangingEventArgs>  Changing;

        public event EventHandler<UserProfileCollectionChangedEventArgs>  Changed;

        public IUserProfile LastProfile
        {
            get { return _last; }
            set
            {
                if( CanChange( ChangeStatus.ContainerUpdate, value.Name, value.Type, value.Address ) )
                {
                    _last = value;
                    _holder.OnCollectionChanged();
                    Change( ChangeStatus.ContainerUpdate, value.Name, value.Type, value.Address );
                }
            }
        }

        IUserProfile IUserProfileCollection.LastProfile { get { return _last; } }
        
        public UserProfileCollection( ConfigurationBase holder )
        {
            _holder = holder;
            _internalDic = new Dictionary<string, UserProfile>();
            _profiles = new ReadOnlyCollectionTypeAdapter<IUserProfile, UserProfile>( _internalDic.Values );
        }

        bool CanChange( ChangeStatus action, string name, ConfigSupportType type, string address )
        {
            if( Changing != null )
            {
                UserProfileCollectionChangingEventArgs eCancel = new UserProfileCollectionChangingEventArgs( this, action, name, type, address );
                Changing( this, eCancel );
                return !eCancel.Cancel;
            }
            return true;
        }

        void Change( ChangeStatus action, string name, ConfigSupportType type, string address )
        {
            if( Changed != null )
            {
                UserProfileCollectionChangedEventArgs e = new UserProfileCollectionChangedEventArgs( this, action, name, type, address );
                Changed( this, e );
            }
        }

        void Add( UserProfile profile )
        {
            Debug.Assert( !_internalDic.ContainsKey( profile.Name ) );

            if( CanChange( ChangeStatus.Add, profile.Name, profile.Type, profile.Address ) )
            {
                _internalDic.Add( profile.Address, profile );
                _holder.OnCollectionChanged();
                Change( ChangeStatus.Add, profile.Name, profile.Type, profile.Address );
            }
        }

        internal void OnDestroy( UserProfile profile )
        {
            if( CanChange( ChangeStatus.Delete, profile.Name, profile.Type, profile.Address ) )
            {
                _internalDic.Remove( profile.Address );
                _holder.OnCollectionChanged();

                Change( ChangeStatus.Delete, profile.Name, profile.Type, profile.Address );
            }
        }

        internal void OnRename( UserProfile profile )
        {
            if( CanChange( ChangeStatus.Update, profile.Name, profile.Type, profile.Address ) )
            {
                _holder.OnCollectionChanged();
                Change( ChangeStatus.Update, profile.Name, profile.Type, profile.Address );
            }
        }

        public IUserProfile Find( string address )
        {
            UserProfile found;
            _internalDic.TryGetValue( address, out found );
            return found;
        }

        public IUserProfile AddOrSet( string name, string address, ConfigSupportType type, bool setAsLast )
        {
            UserProfile p;
            if( _internalDic.ContainsKey( address ) )
            {
                if( _internalDic[address].Name != name )
                    _internalDic[address].Rename( name );
                p = _internalDic[address];
            }
            else
            {
                p = new UserProfile( this, name, type, address );
                Add( p );
            }
            if( setAsLast ) LastProfile = p;
            return p;
        }

        #region IReadOnlyCollection

        public bool Contains( object item )
        {
            return _profiles.Contains( item );
        }

        public int Count
        {
            get { return _profiles.Count; }
        }

        public IEnumerator<IUserProfile> GetEnumerator()
        {
            return _profiles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            r.Read();
            Dictionary<string,UserProfile> newContent = new Dictionary<string, UserProfile>();
            UserProfile newLast = null;
            while( r.IsStartElement( "UserProfile" ) )
            {
                string name = r.GetAttribute( "Name" );
                ConfigSupportType type  = (ConfigSupportType)Enum.Parse( typeof( ConfigSupportType ), r.GetAttribute( "Type" ) );
                string address = r.GetAttribute( "Address" );

                UserProfile p = new UserProfile( this, name, type, address );
                newContent.Add( address, p );

                if( newLast == null || r.GetAttributeBoolean( "IsLast", false ) ) newLast = p;

                r.Skip();
            }
            _internalDic.Clear();
            _internalDic.AddRange( newContent );
            _last = newLast;
            Change( ChangeStatus.ContainerUpdate, string.Empty, ConfigSupportType.None, string.Empty );
        }

        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            foreach( UserProfile profile in _internalDic.Values )
            {
                w.WriteStartElement( "UserProfile" );
                w.WriteAttributeString( "Name", profile.Name );
                w.WriteAttributeString( "Type", profile.Type.ToString() );
                w.WriteAttributeString( "Address", profile.Address );
                w.WriteAttributeString( "IsLast", XmlConvert.ToString( profile.IsLastProfile ) );
                w.WriteFullEndElement();
            }
        }

    }
}
