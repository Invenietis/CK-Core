#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SharedDictionaryWriter.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Xml;
using CK.Storage;
using CK.Core;
using CK.Plugin.Config;
using System;

namespace CK.SharedDic
{
    internal sealed class SharedDictionaryWriter : ISharedDictionaryWriter
	{
		SharedDictionaryImpl _dic;
        IStructuredWriter _writer;
        HashSet<object> _alreadyWritten;

		public SharedDictionaryWriter( SharedDictionaryImpl dic, IStructuredWriter writer )
		{
			_dic = dic;
            _writer = writer;
            _alreadyWritten = new HashSet<object>();
            _writer.Current.ObjectWriteExData += new EventHandler<ObjectWriteExDataEventArgs>( _writer_ObjectWriteExData );
        }

        void _writer_ObjectWriteExData( object sender, ObjectWriteExDataEventArgs e )
        {
            if( !_alreadyWritten.Contains( e.Obj ) )
            {
                WritePluginsDataElement( "PluginsData", e.Obj, false );
            }
        }

        public event EventHandler<SharedDictionaryWriterEventArgs> BeforePluginsData;

        public event EventHandler<SharedDictionaryWriterEventArgs> AfterPluginsData;

        public IStructuredWriter StructuredWriter { get { return _writer; } }

        public ISharedDictionary SharedDictionary { get { return _dic; } }
	
        public int WritePluginsDataElement( string elementName, object o, bool writeEmptyElement )
		{
            if( elementName == null ) throw new ArgumentNullException( "elementName" );
            if( o == null ) throw new ArgumentNullException( "o" );
            int nb = WritePluginsData( o, elementName );
            if( nb == 0 && writeEmptyElement )
            {
                StructuredWriter.Xml.WriteStartElement( elementName );
                StructuredWriter.Xml.WriteEndElement();
            }
            return nb;
		}

        public int WritePluginsData( object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            return WritePluginsData( o, null );
        }

        int WritePluginsData( object o, string elementName )
        {
            _alreadyWritten.Add( o );
            int writeCount = 0;
            XmlWriter w = StructuredWriter.Xml;
            bool mustEndElement = false;
            PluginConfigByObject c;
            if( _dic.TryGetPluginConfigByObject( o, out c ) && c.Count > 0 )
            {
                HashSet<FinalDictionary> done = new HashSet<FinalDictionary>();
                foreach( SharedDictionaryEntry e in c )
                {
                    FinalDictionary f = _dic.GetFinalDictionary( e, false );
                    if( !done.Contains( f ) )
                    {
                        done.Add( f );
                        if( elementName != null )
                        {
                            w.WriteStartElement( elementName );
                            mustEndElement = true;
                            elementName = null;
                        }
                        w.WriteStartElement( "p" );
                        w.WriteAttributeString( "guid", e.PluginId.UniqueId.ToString() );
                        w.WriteAttributeString( "version", e.PluginId.Version.ToString() );
                        if( e.PluginId.PublicName.Length > 0 ) w.WriteAttributeString( "name", e.PluginId.PublicName );
                        ++writeCount;

                        SharedDictionaryWriterEventArgs ev = null;
                        if( BeforePluginsData != null ) BeforePluginsData( this, (ev = new SharedDictionaryWriterEventArgs( this, f )) );

                        f.WriteData( this );

                        if( AfterPluginsData != null ) AfterPluginsData( this, ev ?? new SharedDictionaryWriterEventArgs( this, f ) );

                        w.WriteEndElement();
                    }
                }
            }
            // Obtains the list of SkippedFragments associated to the object.
            IEnumerable<SkippedFragment> fragments = _dic.GetSkippedFragments( o );
            if( fragments != null )
            {
                foreach( SkippedFragment f in fragments )
                {
                    if( elementName != null )
                    {
                        w.WriteStartElement( elementName );
                        mustEndElement = true;
                        elementName = null;
                    }
                    ++writeCount;
                    f.Bookmark.WriteBack( StructuredWriter );
                }
            }
            if( mustEndElement ) w.WriteEndElement();
            return writeCount;
        }

        public void Dispose()
        {
            if( _writer != null )
            {
                _writer.ServiceContainer.Remove( typeof( ISharedDictionaryWriter ) );
                _writer = null;
                _dic = null;
            }
        }
	}
}
