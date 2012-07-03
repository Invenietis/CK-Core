#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\StructuredReaderAndWriterExtension.cs) is part of CiviKey. 
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
using System.Text;
using System.Diagnostics;
using System.Xml;
using CK.Core;

namespace CK.Storage
{

    /// <summary>
    /// Implements extension methods for <see cref="IStructuredWriter"/> and <see cref="IStructuredReader"/> interfaces.
    /// </summary>
    static public class StructuredReaderAndWriterExtension
    {
        /// <summary>
        /// Writes the given object into the structured output with the given element name.
        /// </summary>
        /// <param name="sw">This <see cref="IStructuredWriter"/> object.</param>
        /// <param name="elementName">Name of the xml element.</param>
        /// <param name="o">Object to write. May be null.</param>
        static public void WriteObjectElement( this IStructuredWriter sw, string elementName, object o )
        {
            sw.Xml.WriteStartElement( elementName );
            sw.WriteInlineObject( o );
        }

        /// <summary>
        /// Writes the given object into the structured output with the given element name.
        /// The object can be null, or a <see cref="IStructuredSerializer{T}"/> for its type can be found 
        /// in the services or the object must implement <see cref="IStructuredSerializable"/> interface.
        /// </summary>
        /// <param name="sw">This <see cref="IStructuredWriter"/> object.</param>
        /// <param name="elementName">Name of the xml element.</param>
        /// <param name="o">Object to write. May be null.</param>
        static public void WriteInlineObjectStructuredElement( this IStructuredWriter sw, string elementName, object o )
        {
            sw.Xml.WriteStartElement( elementName );
            sw.WriteInlineObjectStructured( o );
        }

        /// <summary>
        /// Reads an object. The reader must be positioned on the element name.
        /// </summary>
        /// <param name="sr">This <see cref="IStructuredReader"/> object.</param>
        /// <param name="elementName">Name of the xml element.</param>
        /// <returns>Object read.</returns>
        static public object ReadObjectElement( this IStructuredReader sr, string elementName )
        {
            // This does the trick: it raises an XmlException.
            if( !sr.Xml.IsStartElement( elementName ) ) sr.Xml.ReadStartElement( elementName );
            StandardReadStatus status;
            object o = sr.ReadInlineObject( out status );
            if( (status & StandardReadStatus.ErrorMask) != 0 ) throw new CKException( R.UnableToReadInlineObject );
            return o;
        }

        /// <summary>
        /// Reads an object. The reader must be positioned on the element name.
        /// </summary>
        /// <param name="sr">This <see cref="IStructuredReader"/> object.</param>
        /// <param name="elementName">Name of the xml element.</param>
        /// <param name="o">The object read. Null on error.</param>
        /// <returns>A <see cref="StandardReadStatus"/> that may define an error.</returns>
        static public StandardReadStatus ReadObjectElement( this IStructuredReader sr, string elementName, out object o )
        {
            // This does the trick: it raises an XmlException.
            if( !sr.Xml.IsStartElement( elementName ) )
            {
                o = null; 
                return StandardReadStatus.ErrorXmlRead;
            }
            StandardReadStatus status;
            o = sr.ReadInlineObject( out status );
            return status;
        }

        /// <summary>
        /// Reads an object that has been written with <see cref="IStructuredWriter.WriteInlineObjectStructured"/>.
        /// </summary>
        /// <param name="sr">This <see cref="IStructuredReader"/> object.</param>
        /// <param name="type">Type of the object to read. If a <see cref="IStructuredSerializer{T}"/> is available in the services, it is used, 
        /// otherwise, the type must both offer a default construtor and implement <see cref="IStructuredSerializable"/>.</param>
        /// <returns>Deserialized object (can be null).</returns>
        static public object ReadInlineObjectStructured( this IStructuredReader sr, Type type )
        {
            if( type == null ) throw new ArgumentNullException( "type" );
            return sr.ReadInlineObjectStructured( type, null );
        }

        /// <summary>
        /// Reads an object data that has been written with <see cref="IStructuredWriter.WriteInlineObjectStructured"/>.
        /// </summary>
        /// <param name="sr">This <see cref="IStructuredReader"/> object.</param>
        /// <param name="o">Object to read that has been previously created or reinitialized. Can not be null.</param>
        /// <returns>The object.</returns>
        /// <remarks>
        /// If a <see cref="IStructuredSerializer{T}"/> for the runtime type (obtained by <see cref="Object.GetType"/>) is available in the services,
        /// it will be used. Otherwise, the object must implement <see cref="IStructuredSerializable"/>.
        /// </remarks>
        static public object ReadInlineObjectStructured( this IStructuredReader sr, object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            return sr.ReadInlineObjectStructured( o.GetType(), o );
        }

        /// <summary>
        /// Reads an object data that has been written with <see cref="WriteInlineObjectStructuredElement"/>.
        /// If the reader is not positioned on the <paramref name="elementName"/>, an <see cref="XmlException"/> is thrown.
        /// </summary>
        /// <param name="sr">This <see cref="IStructuredReader"/> object.</param>
        /// <param name="elementName">Name of the Xml element.</param>
        /// <param name="o">Object to read that has been previously created or reinitialized. Can not be null.</param>
        /// <returns>The object.</returns>
        /// <remarks>
        /// If a <see cref="IStructuredSerializer{T}"/> for the runtime type (obtained by <see cref="Object.GetType"/>) is available in the services,
        /// it will be used. Otherwise, the object must implement <see cref="IStructuredSerializable"/>.
        /// </remarks>
        static public object ReadInlineObjectStructuredElement( this IStructuredReader sr, string elementName, object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( elementName == null ) throw new ArgumentNullException( "elementName" );
            // This does the trick: it raises an XmlException.
            if( !sr.Xml.IsStartElement( elementName ) ) sr.Xml.ReadStartElement( elementName );
            return sr.ReadInlineObjectStructured( o.GetType(), o );
        }


    }
}
