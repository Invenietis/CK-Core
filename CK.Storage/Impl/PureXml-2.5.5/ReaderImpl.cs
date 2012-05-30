#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\ReaderImpl.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using CK.Core;
using System.Collections.Generic;
using System.Reflection;

namespace CK.Storage
{
    /// <summary>
    /// Simple implementation of <see cref="IStructuredReader"/>.
    /// </summary>
    internal class ReaderImpl : ReaderBase, IStructuredReader
    {
        internal ReaderBase Current;
        XmlReader _xmlReader;
        SimpleServiceContainer _serviceContainer;
        List<string> _path;
        IReadOnlyList<string> _pathEx;
        Version _storageVersion;
        string _openingElementName;
        bool _mustCloseReader;
        bool _isEmpty;

        class SubReaderJail : IDisposable
        {
            ReaderImpl _holder;
            XmlReader _previous;

            public SubReaderJail( ReaderImpl holder )
            {
                _previous = holder._xmlReader;
                holder._xmlReader = _previous.ReadSubtree();
                // Moves the reader on the current node.
                holder._xmlReader.Read();
                _holder = holder;
            }

            public void Dispose()
            {
                _holder._xmlReader.Close();
                _holder._xmlReader = _previous;
                _previous.Skip();
            }
        }

        public ReaderImpl( XmlReader reader, IServiceProvider baseServiceProvider, bool autoCloseReader )
            : base( null )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );
            Current = this;
            _xmlReader = reader;
            _serviceContainer = new SimpleServiceContainer( baseServiceProvider );
            _path = new List<string>();
            _pathEx = new ReadOnlyListOnIList<string>( _path );
            if( reader.IsStartElement( "CK-Structured" ) )
            {
                _storageVersion = new Version( reader.GetAttribute( "version" ) );
                _isEmpty = reader.IsEmptyElement;
                if( !_isEmpty ) _openingElementName = reader.Name;
                reader.Read();
            }
            _mustCloseReader = autoCloseReader;
        }

        public ReaderImpl( XmlReader reader, IServiceProvider baseServiceProvider, Version storageVersion, bool autoCloseReader )
            : base( null )
        {
            Current = this;
            _xmlReader = reader;
            _serviceContainer = new SimpleServiceContainer( baseServiceProvider );
            _path = new List<string>();
            _pathEx = new ReadOnlyListOnIList<string>( _path );
            _storageVersion = storageVersion;
            _mustCloseReader = autoCloseReader;
        }

        protected override void OnDispose()
        {
            if( _openingElementName != null )
            {
                if( Xml.NodeType == XmlNodeType.EndElement && Xml.Name == _openingElementName )
                {
                    Xml.ReadEndElement();
                }
            }
            if( _mustCloseReader ) Xml.Close();
            _serviceContainer.Dispose();
        }

        internal IDisposable CreateJail()
        {
            return new SubReaderJail( this );
        }

        public override XmlReader Xml
        {
            get { return _xmlReader; }
        }

        public override Version StorageVersion
        {
            get { return _storageVersion; }
        }

        public override IStructuredReaderBookmark CreateBookmark()
        {
            return new ReaderBookmark( this );
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public override IServiceProvider BaseServiceProvider
        {
            get { return _serviceContainer.BaseProvider; }
        }

        public override ISimpleServiceContainer ServiceContainer
        {
            get { return _serviceContainer; }
        }

        public override object GetService( Type serviceType )
        {
            return _serviceContainer.GetService( serviceType );
        }

        public void EnterScope( string name )
        {
            _path.Add( name );
        }

        public void LeaveScope()
        {
            _path.RemoveAt( _path.Count - 1 );
        }

        public IReadOnlyList<string> ScopePath
        {
            get { return _pathEx; }
        }

    }
}
