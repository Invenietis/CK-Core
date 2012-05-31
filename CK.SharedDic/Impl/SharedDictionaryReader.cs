#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SharedDictionaryReader.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Xml;
using CK.Storage;
using CK.Core;
using CK.Plugin.Config;
using System.Diagnostics;

namespace CK.SharedDic
{
    internal class SharedDictionaryReader : ISharedDictionaryReader, INamedVersionedUniqueId
	{
        SharedDictionaryImpl _dic;
        IStructuredReader _reader;
		IList<ReadElementObjectInfo> _errorCollector;
        MergeMode _mergeMode;
        Guid _currentPluginId;
        Version _currentPluginVersion;
        string _currentPluginName;

		internal SharedDictionaryReader( SharedDictionaryImpl dic, IStructuredReader reader, MergeMode mergeMode )
		{
			_dic = dic;
            _mergeMode = mergeMode;
            _reader = reader;
			_errorCollector = new List<ReadElementObjectInfo>();
            _currentPluginId = SimpleUniqueId.InvalidId.UniqueId;
            _currentPluginVersion = null;
            reader.Current.ObjectReadExData += new EventHandler<ObjectReadExDataEventArgs>( Current_ObjectReadExData );
        }

        void Current_ObjectReadExData( object sender, ObjectReadExDataEventArgs e )
        {
            if( e.Handled ) return;
            if( ReadPluginsDataElement( "PluginsData", e.Obj ) )
            {
                e.Handled = true;
            }
        }

        public event EventHandler<SharedDictionaryReaderEventArgs> BeforePluginsData;

        public event EventHandler<SharedDictionaryReaderEventArgs> AfterPluginsData;

        public INamedVersionedUniqueId ReadPluginInfo
        {
            get { return _currentPluginVersion != null ? this : null; }
        }

        #region Auto implementation of IVersionedUniqueId for CurrentPlugin backup.

        Version IVersionedUniqueId.Version { get { return _currentPluginVersion; } }

        Guid IUniqueId.UniqueId { get { return _currentPluginId; } }

        string INamedVersionedUniqueId.PublicName { get { return _currentPluginName; } }

        #endregion

		public ISharedDictionary SharedDictionary
		{
			get { return _dic; }
		}

        public IStructuredReader StructuredReader
        {
            get { return _reader; } 
        }

        public MergeMode MergeMode 
        { 
            get { return _mergeMode; } 
        }

		public IList<ReadElementObjectInfo> ErrorCollector
		{
			get { return _errorCollector; }
		}

        public bool ReadPluginsDataElement( string elementName, object o )
        {
            XmlReader r = _reader.Xml;
            if( r.IsStartElement( elementName ) )
            {
                if( r.IsEmptyElement ) r.Read();
                else
                {
                    r.Read();
                    ReadPluginsData( o );
                    r.ReadEndElement();
                }
                return true;
            }
            return false;
        }

        public void ReadPluginsData( object o )
        {
            Guid prevPluginId = _currentPluginId;
            Version prevPluginVersion = _currentPluginVersion;
            string prevPluginName = _currentPluginName;
            try
            {
                XmlReader r = _reader.Xml;
                while( r.IsStartElement( "p" ) )
                {
                    Guid p = SimpleUniqueId.InvalidId.UniqueId;
                    Version v = null;
                    string n = null;
                    try
                    {
                        p = new Guid( r.GetAttribute( "guid" ) );
                        v = r.GetAttributeVersion( "version", Util.EmptyVersion );
                        // Since the public name of the plugin is not a key data in any manner,
                        // we may use here the actual uid.PublicName but this would introduce
                        // a diff between the exposed version (which is the version form the input stream)
                        // and the public name. 
                        // We prefer here to stay consistent: we expose read information for version and name.
                        n = r.GetAttribute( "name" ) ?? String.Empty;
                        INamedVersionedUniqueId uid = _dic.FindPlugin( p );
                        if( uid != null )
                        {
                            FinalDictionary fDic = _dic.GetFinalDictionary( o, uid, true );
                            
                            r.Read();
                            _currentPluginId = p;
                            _currentPluginVersion = v;
                            _currentPluginName = n;

                            SharedDictionaryReaderEventArgs ev = null;
                            if( BeforePluginsData != null ) BeforePluginsData( this, (ev = new SharedDictionaryReaderEventArgs( this, fDic )) );
                            
                            if( _mergeMode == MergeMode.None ) fDic.Clear();
                            fDic.ReadData( this );

                            if( AfterPluginsData != null ) AfterPluginsData( this, ev ?? new SharedDictionaryReaderEventArgs( this, fDic ) );

                            r.ReadEndElement();
                        }
                        else
                        {
                            _dic.StoreSkippedFragment( o, p, v, _reader.CreateBookmark() );
                        }
                    }
                    catch( Exception ex )
                    {
                        if( ErrorCollector != null )
                        {
                            if( p == SimpleUniqueId.InvalidId.UniqueId )
                                ErrorCollector.Add( new ReadElementObjectInfo( ReadElementObjectInfo.ReadStatus.ErrorGuidAttributeMissing, r, ex.Message ) );
                            else if( v == null )
                                ErrorCollector.Add( new ReadElementObjectInfo( ReadElementObjectInfo.ReadStatus.ErrorVersionAttributeInvalid, r, ex.Message ) );
                            else
                                ErrorCollector.Add( new ReadElementObjectInfo( ReadElementObjectInfo.ReadStatus.ErrorXmlRead, r, ex.Message ) );
                        }
                        r.Skip();
                    }
                }
            }
            finally
            {
                _currentPluginId = prevPluginId;
                _currentPluginVersion = prevPluginVersion;
                _currentPluginName = prevPluginName;
            }
        }

        public void Dispose()
        {
            if( _reader != null )
            {
                _reader.ServiceContainer.Remove( typeof( ISharedDictionaryReader ) );
                _reader = null;
                _dic = null;
            }
        }

        public ReadElementObjectInfo PreProcessReadInfo( object o, INamedVersionedUniqueId pluginID, ReadElementObjectInfo info )
        {
            return info;
        }


    }
}
