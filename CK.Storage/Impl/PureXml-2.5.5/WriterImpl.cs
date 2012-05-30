#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\WriterImpl.cs) is part of CiviKey. 
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
using System.Text;
using System.Reflection;

namespace CK.Storage
{
    /// <summary>
    /// Simple implementation of <see cref="IStructuredWriter"/>.
    /// </summary>
    internal class WriterImpl : WriterBase, IStructuredWriter
    {
        internal WriterBase Current;
        SimpleServiceContainer _serviceContainer;
        XmlWriter _xmlWriter;
        bool _mustEndElement;
        bool _mustCloseWriter;

        public WriterImpl( XmlWriter writer, IServiceProvider baseServiceProvider, bool writeElementHeader, bool autoCloseWriter )
            : base( null )
        {
            _xmlWriter = writer;
            _serviceContainer = new SimpleServiceContainer( baseServiceProvider );
            if( writeElementHeader )
            {
                Xml.WriteStartElement( "CK-Structured" );
                Xml.WriteAttributeString( "version", "2.5.5" );
                _mustEndElement = true;
            }
            _mustCloseWriter = autoCloseWriter;
            Current = this;
        }

        /// <summary>
        /// Flushes the inner <see cref="P:XmlWriter"/> and depending on the way this <see cref="SimpleStructuredWriter"/>
        /// has been created, writes the closing tag before and/or closes the inner writer itself.
        /// </summary>
        protected override void OnDispose()
        {
            while( Current != this ) Current.Dispose();
            if( _mustEndElement
                && _xmlWriter.WriteState != WriteState.Start
                && _xmlWriter.WriteState != WriteState.Closed
                && _xmlWriter.WriteState != WriteState.Error ) _xmlWriter.WriteEndElement();
            _xmlWriter.Flush();
            if( _mustCloseWriter && _xmlWriter.WriteState != WriteState.Closed ) _xmlWriter.Close();
            _serviceContainer.Dispose();
        }

        public override XmlWriter Xml
        {
            get { return _xmlWriter; }
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

    }
}
