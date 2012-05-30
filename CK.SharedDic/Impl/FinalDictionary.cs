#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\FinalDictionary.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin.Config;
using CK.Storage;
using System.Diagnostics;

namespace CK.SharedDic
{
    internal sealed class FinalDictionary : IObjectPluginConfig, IObjectPluginAssociation
    {
        readonly SharedDictionaryImpl _dic;
        readonly object _obj;
        readonly INamedVersionedUniqueId _pluginId;
        int _count;

        internal FinalDictionary( SharedDictionaryImpl dic, object obj, INamedVersionedUniqueId p )
        {
            _dic = dic;
            _obj = obj;
            _pluginId = p;
        }

        public event EventHandler<ConfigChangedEventArgs> Changed;

        internal void RaiseChanged( ConfigChangedEventArgs e )
        {
            Debug.Assert( e.MultiPluginId.Contains( _pluginId ) );
            Debug.Assert( e.MultiObj.Contains( _obj ) );
            if( Changed != null ) Changed( this, e );
        }

        public int Count
        {
            get { return _count; }
            internal set { _count = value; }
        }

        public bool Contains( string key )
        {
            return _dic.Contains( _obj, _pluginId, key );
        }

        public void Clear()
        {
            _dic.Clear( _obj, _pluginId );
        }

        public object this[string key]
        {
            get { return _dic[_obj, _pluginId, key]; }
            set { _dic[_obj, _pluginId, key] = value; }
        }

        public void Add( string key, object value )
        {
            _dic.Add( _obj, _pluginId, key, value );
        }

        public T GetOrSet<T>( string key, T value )
        {
            return _dic.GetOrSet( _obj, _pluginId, key, value );
        }

        public T GetOrSet<T>( string key, T value, Func<object, T> converter )
        {
            return _dic.GetOrSet( _obj, _pluginId, key, value, converter );
        }

        public T GetOrSet<T>( string key, Func<T> value )
        {
            return _dic.GetOrSet( _obj, _pluginId, key, value );
        }

        public T GetOrSet<T>( string key, Func<T> value, Func<object, T> converter )
        {
            return _dic.GetOrSet( _obj, _pluginId, key, value, converter );
        }

        public ChangeStatus Set( string key, object value )
        {
            return _dic.Set( _obj, _pluginId, key, value );
        }

        [Obsolete( "Use GetOrSet instead", true )]
        public void TryAdd( string key, object value )
        {
            if ( this[key] == null )
                Add( key, value );
        }

        public bool Remove( string key )
        {
            return _dic.Remove( _obj, _pluginId, key );
        }

        internal void WriteData( SharedDictionaryWriter writer )
        {
            _dic.ForEach( _obj, _pluginId, e => WriteElementObject( writer.StructuredWriter.Current, e.Key, e.Value ) );
        }

        static void WriteElementObject( IStructuredWriter sw, string key, object o )
        {
            sw.Xml.WriteStartElement( "data" );
            sw.Xml.WriteAttributeString( "key", key );
            sw.WriteInlineObject( o );
        }         

        internal void ReadData( SharedDictionaryReader reader )
        {
            XmlReader r = reader.StructuredReader.Xml;
			while( r.MoveToContent() == XmlNodeType.Element && r.Name == "data" )
			{
                ReadElementObjectInfo info = reader.PreProcessReadInfo( _obj, _pluginId, ReadObjectInfo( reader.StructuredReader.Current ) );
				if( info != null )
				{
					if( info.HasError )
					{
						if( reader.ErrorCollector != null )
                            reader.ErrorCollector.Add( info );
					}
					else
					{
                        _dic.ImportValue( new SharedDictionaryEntry( _obj, _pluginId, info.Key, info.ReadObject ), reader.MergeMode );
					}
				}
			}
		}

        static ReadElementObjectInfo ReadObjectInfo( IStructuredReader sr )
        {
            Debug.Assert( sr.Xml.Name == "data" );

            object o = null;
            string errorMessage = "Missing or empty 'key' attribute.";
            ReadElementObjectInfo.ReadStatus status = ReadElementObjectInfo.ReadStatus.ErrorKeyAttributeMissing;
            string key = sr.Xml.GetAttribute( "key" );

            bool readDone = false;
            if( key != null )
            {
                key = key.Trim();
                if( key.Length > 0 )
                {
                    try
                    {
                        readDone = true;
                        StandardReadStatus readStatus;
                        o = sr.ReadInlineObject( out readStatus );
                        status = (ReadElementObjectInfo.ReadStatus)readStatus;
                    }
                    catch( Exception ex )
                    {
                        status |= ReadElementObjectInfo.ReadStatus.ErrorWhileReadingElementObject;
                        errorMessage = ex.Message;
                    }
                }
            }
            if( (status & ReadElementObjectInfo.ReadStatus.ErrorMask) == 0 )
            {
                return new ReadElementObjectInfo( status, key, o );
            }
            if( !readDone ) sr.Xml.Skip();
            return new ReadElementObjectInfo( status, sr.Xml, errorMessage );
        }

        INamedVersionedUniqueId IObjectPluginAssociation.PluginId
		{
			get { return _pluginId; }
		}

		object IObjectPluginAssociation.Obj
		{
			get { return _obj; }
		}

	}
}
